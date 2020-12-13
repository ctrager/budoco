using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Serilog;
using System;

namespace budoco.Pages
{
    public class LoginModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        // bindings end  

        int user_id = 0;

        List<string> errs = new List<string>();

        public void OnGet(string action)
        {
            if (action is not null)
            {
                Signout();
                return;
            }
        }

        public void OnPost()
        {
            if (!IsValid())
            {
                bd_util.set_flash_errs(HttpContext, errs);
                return;
            }

            // login - insert row in sessions

            string budoco_session_id = Guid.NewGuid().ToString();
            HttpContext.Response.Cookies.Append(
                bd_util.BUDOCO_SESSION_ID, budoco_session_id, bd_util.get_cookie_options());

            string sql = @"insert into sessions (se_id, se_user)
                values (@se_id, @se_user)";

            var dict = new Dictionary<string, dynamic>();
            dict["@se_id"] = budoco_session_id;
            dict["@se_user"] = user_id;
            bd_db.exec(sql, dict);

            string next_url = HttpContext.Session.GetString(bd_util.NEXT_URL);
            HttpContext.Session.SetString(bd_util.NEXT_URL, "");

            if (!string.IsNullOrEmpty(next_url)
            && next_url.Contains("Issue?id="))
            {
                Response.Redirect(next_url);
            }
            else
            {
                Response.Redirect("/App/Issues");
            }

        }

        public void Signout()
        {
            string session_id = HttpContext.Session.GetString(bd_util.BUDOCO_SESSION_ID);
            HttpContext.Session.SetString(bd_util.BUDOCO_SESSION_ID, "");
            string sql = "delete from sessions where se_id = '" + session_id + "'";
            bd_db.exec(sql);
            Response.Redirect("/Login");
        }

        bool IsValid()
        {

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("User or email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errs.Add("Password is required.");
            }

            if (errs.Count > 0)
            {
                return false;
            }

            // check user and password
            string sql = @"select * from users where 
            (us_username = @us_username or us_email_address = @us_email_address)";

            var dict = new Dictionary<string, dynamic>();

            dict["@us_username"] = username;
            dict["@us_email_address"] = username; // on purpose, user can login typing either

            DataRow dr_user = bd_db.get_datarow(sql, dict);

            if (dr_user is null)
            {
                errs.Add("User or Password is incorect.");
                return false;
            }

            if (!(bool)dr_user["us_is_active"])
            {
                errs.Add("Your user account is set to inactive.");
                return false;
            }

            user_id = (int)dr_user[0];

            string password_in_db = (string)dr_user["us_password"];

            // For users we added with sql to to db, the first time they log in
            if (password_in_db.Length < 48)
            {

                // force password reset
                string guid = bd_util.insert_change_password_request_link(
                    "none", user_id);

                errs.Add("You must create a password.");
                Response.Redirect("/ResetPassword?guid=" + guid);
                return false;
            }

            if (!bd_util.check_password_against_hash(password, password_in_db))
            {
                // on purpose lowercase password so that Corey knows the diff
                errs.Add("User or password is incorrect.");
                return false;
            }

            return true;
        }
    }
}
