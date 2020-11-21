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
    public class RegisterModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public string email { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        // bindings end  

        public void OnGet(string action)
        {
        }

        public void OnPost()
        {

            if (!IsValid())
            {
                return;
            }

            // insert unguessable bytes into db for user to confirm registration
            string sql = @"insert into emailed_links 
            (el_guid, el_email, el_action, el_username, el_password)
            values(@el_guid, @el_email, @el_action, @el_username, @el_password)";

            var dict = new Dictionary<string, dynamic>();

            var guid = Guid.NewGuid();
            dict["@el_guid"] = guid;
            dict["@el_username"] = username; // on purpose, user can login typing either
            dict["@el_email"] = email;
            string hashed_password = bd_util.compute_password_hash(password);
            dict["@el_password"] = hashed_password;
            dict["@el_action"] = "register";

            db_util.exec(sql, dict);

            if (Startup.cnfg.AutoConfirmRegistration)
            {
                // send out email...
                Response.Redirect("/RegisterConfirmation?guid=" + guid);
            }
            else
            {
                // send an email
                // and tell user to check it
                bd_util.set_flash_msg(HttpContext, "Please check your email to confirm registration.");
                Response.Redirect("RegisterPleaseConfirm");
            }
        }


        bool IsValid()
        {
            var errs = new List<string>();
            string sql;
            var dict = new Dictionary<string, dynamic>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username required.");
            }
            else
            {
                sql = "select 1 from users where us_username = @us_username";
                dict["@us_username"] = username;
                if (db_util.exists(sql, dict))
                {
                    errs.Add("Username already registered.");
                }
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                errs.Add("Email is required.");
            }
            else
            {
                sql = "select 1 from users where us_email = @us_email";
                dict["@us_email"] = email;
                if (db_util.exists(sql, dict))
                {
                    errs.Add("Email already registered.");
                }
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errs.Add("Password is required.");
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
