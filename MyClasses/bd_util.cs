using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;

public class bd { }; // for logging context

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
        public const string NEXT_URL = "next_url";
        public const int SYSTEM_USER_ID = 1;

        static Serilog.ILogger _logger = null;


        public static void init_serilog_context()
        {
            _logger = Serilog.Log.ForContext<bd>();
        }

        public static void log(object msg, Serilog.Events.LogEventLevel level = Serilog.Events.LogEventLevel.Information)
        {
            string s = System.Threading.Thread.CurrentThread.ManagedThreadId + " " + msg.ToString();

            if (_logger is not null)
            {
                _logger.Write(level, s);
            }
            else
            {
                Console.WriteLine(System.DateTime.Now.ToString("HH-mm-ss-fff") + " " + s);
            }
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
            options.SameSite = SameSiteMode.Lax; // so that when we click a link in email it works
            return options;
        }


        public static bool check_user_permissions(HttpContext context)
        {

            // The default behavior expires the session id every app restart which
            // meant I had to re-log in every time I changed code    
            string session_id = null;
            if (context.Request.Cookies.ContainsKey(bd_util.BUDOCO_SESSION_ID))
            {
                session_id = context.Request.Cookies[bd_util.BUDOCO_SESSION_ID];
                context.Session.SetString(bd_util.BUDOCO_SESSION_ID, session_id);
            }

            string url = context.Request.GetDisplayUrl().ToLower();

            bool must_be_admin = false;

            if (url.Contains("admin/"))
            {
                must_be_admin = true;
            }
            else if (!url.Contains("app/"))
            {
                // don't redirect, because Login, About, Register, etc, don't reuire permission checking
                return true; // because user doesn't need to be logged in.
            }

            // at this point, url contains either "Admin/" or "App/", so must be
            // signed in.

            bool redirect = false;
            DataRow dr = null;

            if (string.IsNullOrEmpty(session_id))
            {
                // no cookie
                redirect = true;
            }
            else
            {
                string sql = @"/*check_user_permissions*/ select * from sessions 
                inner join users on se_user = us_id
                where se_id = '" + session_id + "'; /*check_user_permissions */";

                dr = bd_db.get_datarow(sql);

                if (dr is null)
                {
                    redirect = true;
                }
            }

            if (redirect)
            {
                // for user clicking on links in emails
                string url_for_issue = context.Request.GetDisplayUrl();
                if (url_for_issue.Contains("Issue?id="))
                {
                    context.Session.SetString(NEXT_URL, url_for_issue);
                }

                context.Response.Redirect("/Login");
                return false;
            }

            // User is logged in, so stash details in session
            // for downstream

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
                context.Response.Redirect("/Stop");
                return false;
            }

            if (must_be_admin)
            {
                if (!is_admin)
                {
                    bd_util.set_flash_err(context, "Attempt to view admin only page.");
                    context.Response.Redirect("/Stop");
                    return false;
                }
            }

            return true;

        }

        public static string get_username_from_session(HttpContext context)
        {
            string username = context.Session.GetString("us_username");

            if (username is null)
            {
                set_flash_err(context, "Your session has expired (1)");
                //context.Response.Redirect("/Login");
                return null;
            }
            else
            {
                return username;
            }
        }

        public static int get_user_organization_from_session(HttpContext context)
        {
            object org = context.Session.GetInt32("us_organization");

            if (org is null)
            {
                set_flash_err(context, "Your session has expired (2)");
                //context.Response.Redirect("/Login");
                return 0;
            }
            else
            {
                return (int)org;
            }
        }

        public static int get_user_id_from_session(HttpContext context)
        {
            object id = context.Session.GetInt32("us_id");

            if (id is null)
            {
                set_flash_err(context, "Your session has expired (3)");
                //context.Response.Redirect("/Login");
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
            {
                return false;
            }

            return (int)us_is_admin == 0 ? false : true;
        }

        public static bool is_user_report_only(HttpContext context)
        {
            object us_is_report_only = context.Session.GetInt32("us_is_report_only");
            if (us_is_report_only is null)
            {
                set_flash_err(context, "Your session has expired (5)");
                //context.Response.Redirect("/Login");
                return false;
            }

            return (int)us_is_report_only == 0 ? false : true;
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


        public const string AND_ORG = "/*AND_ORG*/";
        public const string WHERE_ORG = "/*WHERE_ORG*/";

        public static string enhance_sql_per_user(HttpContext context, string sql)
        {
            int us_id = bd_util.get_user_id_from_session(context);
            int us_organization = bd_util.get_user_organization_from_session(context); ;

            string modified_sql = sql.Replace("$ME", us_id.ToString());

            if (us_organization == 0)
            {
                return modified_sql;
            }

            // Add the org restriction to the sql.
            // Look for markers where to do the replace.
            // The "/*ORG1*/" style comments that we are injecting back into 
            // the sql is to help with debuging 

            int pos = modified_sql.IndexOf(AND_ORG);
            if (pos > 0)
            {
                modified_sql = modified_sql.Replace(
                    AND_ORG, "/*ORG1*/ AND i_organization = " + us_organization.ToString());
                return modified_sql;
            }

            pos = modified_sql.IndexOf(WHERE_ORG);
            if (pos > 0)
            {
                modified_sql = modified_sql.Replace(
                    WHERE_ORG, "/*ORG2*/ WHERE i_organization = " + us_organization.ToString());
                return modified_sql;
            }

            /*
             If we don't find the markers , then try to guess.
             Look for the FIRST occurence of "where" and the FIRST occurence of "order".

             if yes where and yes order, then inject before order with an "AND":
                 select * from issues where foo AND (i_organiztion = N) order by bar

             if yes where and no order, then inject at the end with an "AND"
                 select * from issues where foo AND (i_organiztion = N) 

             if no where and yes order, then inject before order with "WHERE" instead of "AND"
                 select * from issues WHERE (i_organiztion = N) order by bar

             if no where and no order, then inject at the end with "WHERE" instead of "AND"
                 select * from issues WHERE (i_organiztion = N)
             */

            // we are just using the lowercase sql for the IndexOf calls
            string lowercase_sql = modified_sql.ToLower();
            int where_pos = lowercase_sql.IndexOf("where");
            int order_pos = lowercase_sql.IndexOf("order by");

            if (where_pos > 0 && order_pos > 0)
            {
                modified_sql = modified_sql.Insert(order_pos,
                " /*ORG3*/ AND (i_organization = " + us_organization.ToString() + ") ");
                return modified_sql;
            }

            if (where_pos > 0 && order_pos <= 0)
            {
                modified_sql +=
                " /*ORG4*/ AND (i_organization = " + us_organization.ToString() + ") ";
                return modified_sql;
            }

            if (where_pos <= 0 && order_pos > 0)
            {
                modified_sql = modified_sql.Insert(order_pos,
                " /*ORG5*/ WHERE (i_organization = " + us_organization.ToString() + ") ");
                return modified_sql;
            }

            if (where_pos <= 0 && order_pos <= 0)
            {
                modified_sql +=
                "/*ORG6*/ WHERE (i_organization = " + us_organization.ToString() + ") ";
                return modified_sql;
            }

            return modified_sql;

        }

        public static bool is_username_already_taken(string username)
        {
            string sql = @"select 1 from users                 where us_username = @username1
                     union select 1 from registration_requests where rr_username = @username2";
            var dict = new Dictionary<string, dynamic>();
            dict["@username1"] = username;
            dict["@username2"] = username;
            return bd_db.exists(sql, dict);
        }

        public static bool is_email_already_taken(string email)
        {
            string sql = @"select 1 from users                 where us_email_address = @email1
                     union select 1 from registration_requests where rr_email_address = @email2";
            var dict = new Dictionary<string, dynamic>();
            dict["@email1"] = email;
            dict["@email2"] = email;
            return bd_db.exists(sql, dict);

        }

        public static bool is_username_already_taken_not_by_me(string username, int user_id)
        {
            string sql = @"select 1 from users                 where us_username = @username1 and us_id != @us_id
                     union select 1 from registration_requests where rr_username = @username2";
            var dict = new Dictionary<string, dynamic>();
            dict["@username1"] = username;
            dict["@username2"] = username;
            dict["@us_id"] = user_id;
            return bd_db.exists(sql, dict);
        }

        public static bool is_email_already_taken_not_by_me(string email, int user_id)
        {
            string sql = @"select 1 from users                 where us_email_address = @email1 and us_id != @us_id
                     union select 1 from registration_requests where rr_email_address = @email2";
            var dict = new Dictionary<string, dynamic>();
            dict["@email1"] = email;
            dict["@email2"] = email;
            dict["@us_id"] = user_id;
            return bd_db.exists(sql, dict);

        }


    } // end class
}