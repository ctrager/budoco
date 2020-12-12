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
    public class RegisterConfirmationModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public string guid { get; set; }

        public void OnGet()
        {
            string sql = "select * from registration_requests where rr_guid = @rr_guid";
            var dict = new Dictionary<string, dynamic>();
            dict["@rr_guid"] = guid;
            DataRow dr_registration = bd_db.get_datarow(sql, dict);

            if (dr_registration is null)
            {
                bd_util.set_flash_err(HttpContext, "Registration request not found. Please register again.");
                Response.Redirect("/Register");
            }
            else
            {
                dict["@us_username"] = (string)dr_registration["rr_username"];
                dict["@us_email_address"] = (string)dr_registration["rr_email_address"];
                dict["@us_password"] = (string)dr_registration["rr_password"];

                bool is_active = true;
                bool is_report_only = false;

                if (bd_config.get(bd_config.NewUserStartsInactive) == 1)
                    is_active = false;

                if (bd_config.get(bd_config.NewUserStartsReportOnly) == 1)
                    is_report_only = true;

                // create user
                sql = @"insert into users 
                    (us_username, us_email_address, us_password, us_is_active, us_is_report_only) 
                    values(@us_username, @us_email_address, @us_password, @us_is_active, @us_is_report_only) 
                    returning us_id";

                dict["@us_username"] = (string)dr_registration["rr_username"];
                dict["@us_email_address"] = (string)dr_registration["rr_email_address"];
                dict["@us_password"] = (string)dr_registration["rr_password"];
                dict["@us_is_active"] = is_active;
                dict["@us_is_report_only"] = is_report_only;
                // insert user
                bd_db.exec_scalar(sql, dict);

                //delete request
                sql = "delete from registration_requests where rr_guid = @rr_guid ";
                dict["@rr_guid"] = guid;
                bd_db.exec_scalar(sql, dict);

                bd_util.set_flash_msg(HttpContext, "Welcome. Registration was successful. You can now log in.");

                Response.Redirect("/Login");
            }

        }
    }
}
