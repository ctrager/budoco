using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Serilog;

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
                return;
            }

            // login - insert row in sessions

            string sql = @"insert into sessions (se_id, se_user)
                values (@se_id, @se_user)";

            var dict = new Dictionary<string, dynamic>();
            dict["@se_id"] = HttpContext.Session.Id;
            dict["@se_user"] = user_id;
            bd_db.exec(sql, dict);
            Response.Redirect("Issues");

        }

        public void Signout()
        {
            string sql = "delete from sessions where se_id = '" + HttpContext.Session.Id + "'";
            bd_db.exec(sql);
            Response.Redirect("/Login");
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("User or email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errs.Add("Password is required.");
            }

            if (errs.Count == 0)
            {
                // check user and password
                string sql = @"select * from users where 
                (us_username = @us_username or us_email = @us_email)";

                var dict = new Dictionary<string, dynamic>();

                dict["@us_username"] = username;
                dict["@us_email"] = username; // on purpose, user can login typing either

                DataRow dr_user = bd_db.get_datarow(sql, dict);

                if (dr_user is null)
                {
                    errs.Add("User or Password incorect.");
                }
                else
                {
                    string password_in_db = (string)dr_user["us_password"];
                    user_id = (int)dr_user[0];

                    // admin user's first time
                    if (user_id == 1 && password_in_db.Length < 48)
                    {

                        // force password reset
                        string guid = bd_util.insert_change_password_request_link(
                            "none", user_id);

                        errs.Add("You must create a password.");
                        Response.Redirect("/ResetPassword?guid=" + guid);

                    }
                    else
                    {
                        if (!bd_util.check_password_against_hash(password, password_in_db))
                        {
                            // on purpose lowercase password so that Corey knows the diff
                            errs.Add("User or password incorrect.");
                        }
                    }
                }
            }

            if (errs.Count == 0)
            {
                return true;
            }
            else
            {
                bd_util.set_flash_err(HttpContext, errs);
                return false;
            }
        }
    }
}
