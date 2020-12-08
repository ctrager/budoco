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
        public string email_address { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        // bindings end  
        List<string> errs = new List<string>();

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
            string sql = @"insert into registration_requests 
            (rr_guid, rr_email_address, rr_username, rr_password)
            values(@rr_guid, @rr_email_address, @rr_username, @rr_password)";

            var dict = new Dictionary<string, dynamic>();

            var guid = Guid.NewGuid();
            dict["@rr_guid"] = guid;
            dict["@rr_username"] = username; // on purpose, user can login typing either
            dict["@rr_email_address"] = email_address;
            string hashed_password = bd_util.compute_password_hash(password);
            dict["@rr_password"] = hashed_password;

            bd_db.exec(sql, dict);

            if (bd_config.get(bd_config.DebugAutoConfirmRegistration) == 1)
            {
                Response.Redirect("/RegisterConfirmation?guid=" + guid);
            }
            else
            {
                // send an email
                // and tell user to check it

                string body = "Follow or browse to this link to confirm registration:\n"
                    + bd_config.get(bd_config.WebsiteUrlRootWithoutSlash)
                    + "/RegisterConfirmation?guid="
                    + guid;

                bd_email.queue_email("register",
                    email_address, // to
                    bd_config.get(bd_config.AppName) + ": Confirm registration", // subject
                    body);

                bd_util.set_flash_msg(HttpContext, "Please check your email to confirm registration.");
                Response.Redirect("RegisterPleaseConfirm");

            }
        }


        bool IsValid()
        {
            // DRY up this code 
            // Identica; in User and UserSettings
            // and similar in Register

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username required.");
            }
            else
            {
                if (bd_util.is_username_already_taken(username))
                {
                    errs.Add("Username already taken.");
                }
            }

            if (string.IsNullOrWhiteSpace(email_address))
            {
                errs.Add("Email is required.");
            }
            else if (!bd_email.validate_email_address(email_address))
            {
                errs.Add("Email is invalid.");
            }
            else
            {
                if (bd_util.is_email_already_taken(username))
                {
                    errs.Add("Email already taken.");
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
                bd_util.set_flash_errs(HttpContext, errs);
                return false;
            }
        }
    }
}
