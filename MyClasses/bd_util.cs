using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using MailKit;
using MimeKit;
using Serilog;

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


        public static int get_int_or_zero_from_string(string s)
        {
            if (s is null)
            {
                return 0;
            }

            return Convert.ToInt32(s);
        }

        public static string send_email(string to, string from, string subject, string body)
        {
            var message = new MimeMessage();

            message.To.Add(new MailboxAddress("", to));
            message.From.Add(new MailboxAddress("", from));
            message.Subject = subject;
            bd_util.console_write_line("email to: " + to);
            bd_util.console_write_line("email body: " + body);
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            string result = "";


            if (bd_config.get("DebugSkipSendingEmails") == 0)
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    try
                    {
                        client.Connect(
                            bd_config.get("SmtpHost"),
                            bd_config.get("SmtpPort"),
                            MailKit.Security.SecureSocketOptions.Auto);

                        string smtp_user = bd_config.get("SmtpUser");
                        string smtp_password = bd_config.get("SmtpPassword");
                        client.Authenticate(smtp_user, smtp_password);
                        bd_util.console_write_line("sending...");
                        client.Send(message);
                        client.Disconnect(true);
                    }
                    catch (Exception e)
                    {
                        result = e.Message;
                        bd_util.console_write_line(e.Message);
                        bd_util.console_write_line(e.StackTrace);

                    }
                }
            }

            return result;
        }

        public static void console_write_line(string msg)
        {
            Console.WriteLine(msg);
            Log.Information(msg);
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

        public static void set_flash_err(HttpContext context, List<string> errs)
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

            string sql = @"select * from sessions 
                inner join users on se_user = us_id
                where se_id = '" + session_id + "'";

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
            context.Session.SetString("us_email", (string)dr["us_email"]);
            context.Session.SetInt32("us_is_admin", Convert.ToInt32((bool)dr["us_is_admin"]));
            context.Session.SetInt32("us_is_active", Convert.ToInt32((bool)dr["us_is_active"]));
            context.Session.SetInt32("us_is_report_only", Convert.ToInt32((bool)dr["us_is_report_only"]));

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

        public static string insert_change_password_request_link(string email, int user_id)

        {
            // insert unguessable bytes into db for user to confirm registration
            string sql = @"insert into emailed_links 
                     (el_guid, el_email, el_user_id, el_action)
                    values(@el_guid, @el_email, @el_user_id, @el_action)";

            var guid = Guid.NewGuid();
            Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();

            dict["@el_guid"] = guid;
            dict["@el_email"] = email;
            dict["@el_action"] = "reset";
            dict["@el_user_id"] = user_id;

            bd_db.exec(sql, dict);

            return guid.ToString();

        }

    }
}