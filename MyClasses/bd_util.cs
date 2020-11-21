using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;

namespace budoco
{
    public static class bd_util
    {
        public static int get_int_or_zero_from_string(string s)
        {
            if (s is null)
            {
                return 0;
            }

            return Convert.ToInt32(s);
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

            if (s is null)
            {
                s = "";
            }

            string[] err_array = s.Split('|');

            return new List<string>(err_array);

        }

        public static void set_flash_err(HttpContext context, List<string> errs)
        {
            string s = string.Join('|', errs);
            context.Session.SetString("flash_err", s);
        }

        public static void redirect_if_not_logged_in(HttpContext context)
        {
            // Session was changing every page. Stackoverflow person
            // said session needs at least one thing. This seems to work.
            context.Session.SetString("dummy", "dummy");

            string sql = @"select * from sessions 
                inner join users on se_user = us_id
                where se_id = '" + context.Session.Id + "'";

            DataRow dr = db_util.get_datarow(sql);

            if (dr is null)
            {
                context.Response.Redirect("/Login");
                return;
            }

            // save in session so that downstream pages don't have to reread
            context.Session.SetInt32("us_id", (int)dr["us_id"]);
            context.Session.SetString("us_username", (string)dr["us_username"]);
            context.Session.SetString("us_email", (string)dr["us_email"]);
            context.Session.SetInt32("us_is_admin", Convert.ToInt32((bool)dr["us_is_admin"]));

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
    }
}