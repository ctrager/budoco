using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using Serilog;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace budoco
{

    /* 
    
    A big bunch of heterogenous helper functions. Anytime I find myself
    writing code that isn't DRY, I make a function here.
    
    */

    public static class bd_util
    {
        public const string BUDOCO_SESSION_ID = "budoco_session_id";
        public const bool MUST_BE_ADMIN = true;
        public const string CREATE_WAS_SUCCESSFUL = "Create was successful.";
        public const string UPDATE_WAS_SUCCESSFUL = "Update was successful.";
        public const string NAME_ALREADY_USED = "Name is already being used.";
        public const string NAME_IS_REQUIRED = "Name is required.";


        public static void log(object msg)
        {
            Console.WriteLine(msg.ToString()); // here on purpose
            Log.Information(msg.ToString());
        }

        public static string get_flash_msg(HttpContext context)
        {
            string flash = context.Session.GetString("flash_msg");
            context.Session.SetString("flash_msg", "");
            return flash;
        }

        public static void set_flash_msg(HttpContext context, string msg)
        {
            context.Session.SetString("flash_msg", msg);
        }

        public static List<string> get_flash_errs(HttpContext context)
        {
            string s = context.Session.GetString("flash_err");
            context.Session.SetString("flash_err", "");

            if (string.IsNullOrWhiteSpace(s))
            {
                return new List<string>();
            }
            else
            {
                string[] err_array = s.Split('|');
                return new List<string>(err_array);
            }

        }

        public static void set_flash_err(HttpContext context, string s)
        {
            context.Session.SetString("flash_err", s);
        }

        public static void set_flash_errs(HttpContext context, List<string> errs)
        {
            string s = string.Join('|', errs);
            context.Session.SetString("flash_err", s);
        }

        public static CookieOptions get_cookie_options()
        {
            var options = new CookieOptions();
            options.Expires = DateTime.Now.AddHours(12);
            options.IsEssential = true;
            options.SameSite = SameSiteMode.Strict;
            return options;
        }


        public static bool check_user_permissions(HttpContext context, bool must_be_admin = false)
        {

            string session_id = context.Request.Cookies[bd_util.BUDOCO_SESSION_ID];

            if (session_id is null)
            {
                context.Response.Redirect("/Login");
                return false;
            }

            string sql = @"/*check_user_permissions*/ select * from sessions 
                inner join users on se_user = us_id
                where se_id = '" + session_id + "'; /*check_user_permissions */";

            DataRow dr = bd_db.get_datarow(sql);

            if (dr is null)
            {
                context.Response.Redirect("/Login");
                return false;
            }

            bool is_active = (bool)dr["us_is_active"];
            bool is_admin = (bool)dr["us_is_admin"];

            // save in session so that downstream pages don't have to reread
            context.Session.SetInt32("us_id", (int)dr["us_id"]);
            context.Session.SetString("us_username", (string)dr["us_username"]);
            context.Session.SetString("us_email_address", (string)dr["us_email_address"]);
            context.Session.SetInt32("us_is_admin", Convert.ToInt32((bool)dr["us_is_admin"]));
            context.Session.SetInt32("us_is_active", Convert.ToInt32((bool)dr["us_is_active"]));
            context.Session.SetInt32("us_is_report_only", Convert.ToInt32((bool)dr["us_is_report_only"]));
            context.Session.SetInt32("us_organization", (int)dr["us_organization"]);

            if (!is_active)
            {
                bd_util.set_flash_err(context, "Your user account is set to inactive.");
                context.Response.Redirect("/Login");
                return false;
            }

            if (must_be_admin)
            {
                if (!is_admin)
                {
                    bd_util.set_flash_err(context, "Attempt to view admin only page.");
                    context.Response.Redirect("/Index");
                    return false;
                }
            }

            return true;

        }

        public static string get_username_from_session(HttpContext context)
        {
            return context.Session.GetString("us_username");
        }

        public static int get_user_id_from_session(HttpContext context)
        {
            object id = context.Session.GetInt32("us_id");

            if (id is null)
            {
                return 0;
            }
            else
            {
                return (int)id;
            }
        }

        public static bool is_user_admin(HttpContext context)
        {
            object us_is_admin = context.Session.GetInt32("us_is_admin");
            if (us_is_admin is null)
                return false;

            return (int)us_is_admin == 0 ? false : true;
        }

        // https://stackoverflow.com/questions/4181198/how-to-hash-a-password/10402129#10402129
        public static string compute_password_hash(string password)
        {

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // 48 chars long
            return Convert.ToBase64String(hashBytes);

        }

        // https://stackoverflow.com/questions/4181198/how-to-hash-a-password/10402129#10402129
        public static bool check_password_against_hash(string entered_password, string saved_hash)
        {
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(saved_hash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(entered_password, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;

            return true;
        }



        public static string insert_change_password_request_link(string email_address, int user_id)

        {
            // insert unguessable bytes into db for user to confirm registration
            string sql = @"insert into reset_password_requests 
                     (rp_guid, rp_email_address, rp_user_id)
                    values(@rp_guid, @rp_email_address, @rp_user_id)";

            var guid = Guid.NewGuid();
            Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();

            dict["@rp_guid"] = guid;
            dict["@rp_email_address"] = email_address;
            dict["@rp_action"] = "reset";
            dict["@rp_user_id"] = user_id;

            bd_db.exec(sql, dict);

            return guid.ToString();

        }

        // public static string strip_html_tags(string input)
        // {
        //     return Regex.Replace(input, "<.*?>", String.Empty);
        // }

        // returns the first dangerous word encountered
        public static string find_dangerous_words_in_sql(string sql)
        {

            if (bd_config.get(bd_config.CheckForDangerousSqlKeywords) == 0)
                return null;

            // build array of keywords
            string[] dangerous_keywords = bd_config.get(bd_config.DangerousSqlKeywords).ToLower().Split(",");
            string s = sql.Trim().ToLower();

            // convert some chars to whitespace, like ,;=:

            // [^ means not, so if th
            Regex rgx = new Regex("[^a-z0-9\"\' _]");
            s = rgx.Replace(s, " ");
            string[] sql_words = s.Split(" ");

            // find the first dangerous word
            foreach (string word_in_query in sql_words)
            {
                if (dangerous_keywords.Contains(word_in_query))
                {
                    return word_in_query;
                }
            }

            return null;
        }


        /*

        Budoco injects a where clause into queries when the logged on user belongs to an
        organization and so is restricted to only issues associated with that organization.
        The injection logic works like this:

        First look for "hints" $AND_ORG or $WHERE_ORG.
        You will probably want to use these hints if your queries are complex, like, if they use subquiries.

        if missing, look for first occurence of "where" and first occurence of "order"

        if yes where and yes order, then inject before order WITH AND
            select ... where foo AND (org = N) order by bar

        if yes where and no  order, then inject at the end WITH AND
            select ... where foo AND (org = N) 

        if no  where and yes order, then inject before order WITH WHERE instead of AND
            select ... WHERE (org = N) order by bar

        if no  where and no  order, then inject at the end WITH WHERE instead of AND
            select ... WHERE (org = N)
        */

        public static string enhance_sql_per_user(HttpContext context, string sql)
        {
            string modified_sql = sql.Replace("$ME", context.Session.GetInt32("us_id").ToString());


            return modified_sql;
        }


    } // end class
}