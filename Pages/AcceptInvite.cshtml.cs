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
    public class AcceptInviteModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public string guid { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        [BindProperty]
        public string email_address { get; set; }

        public void OnGet()
        {
            string sql = "select * from registration_requests where rr_guid = @rr_guid";
            var dict = new Dictionary<string, dynamic>();
            dict["@rr_guid"] = guid;
            DataRow dr_registration = bd_db.get_datarow(sql, dict);

            if (dr_registration is null)
            {
                bd_util.set_flash_err(HttpContext, "Invitation not found");
                Response.Redirect("/Stop");
            }
            else
            {
                username = (string)dr_registration["rr_username"];
                email_address = (string)dr_registration["rr_email_address"];
                bd_util.set_flash_err(HttpContext, "Create a password.");
            }
        }

        public void OnPost()
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                bd_util.set_flash_err(HttpContext, "Password is required.");
                return;
            }

            string sql = "select * from registration_requests where rr_guid = @rr_guid";
            var dict = new Dictionary<string, dynamic>();
            dict["@rr_guid"] = guid;
            DataRow dr_registration = bd_db.get_datarow(sql, dict);

            if (dr_registration is null)
            {
                bd_util.set_flash_err(HttpContext, "Invitation not found");
                Response.Redirect("/Stop");
            }

            bool is_active = true;
            bool is_report_only = false;

            if (bd_config.get(bd_config.NewUserStartsInactive) == 1)
                is_active = false;

            if (bd_config.get(bd_config.NewUserStartsReportOnly) == 1)
                is_report_only = true;

            // create user
            sql = @"insert into users 
                    (us_username, us_email_address, us_password, us_is_active, us_is_report_only, us_organization) 
                    values(@us_username, @us_email_address, @us_password, @us_is_active, @us_is_report_only, @us_organization) 
                    returning us_id";

            dict["@us_username"] = (string)dr_registration["rr_username"];
            dict["@us_email_address"] = (string)dr_registration["rr_email_address"];
            dict["@us_is_active"] = is_active;
            dict["@us_is_report_only"] = is_report_only;
            dict["@us_organization"] = (int)dr_registration["rr_organization"];
            string hashed_password = bd_util.compute_password_hash(password);
            dict["@us_password"] = hashed_password;

            // insert user
            bd_db.exec_scalar(sql, dict);

            //delete request
            sql = "delete from registration_requests where rr_guid = @rr_guid ";
            dict["@rr_guid"] = guid;
            bd_db.exec_scalar(sql, dict);

            bd_util.set_flash_msg(HttpContext, "Welcome. You can now log in.");

            Response.Redirect("/Login");

        }

    }
}
