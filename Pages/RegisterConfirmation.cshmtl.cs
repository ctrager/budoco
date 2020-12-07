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

        public bool is_success = false;

        public void OnGet()
        {
            var errs = new List<string>();
            string sql = "select * from registration_requests where rr_guid = @rr_guid";
            var dict = new Dictionary<string, dynamic>();
            dict["@rr_guid"] = guid;
            DataRow dr_registration = bd_db.get_datarow(sql, dict);

            if (dr_registration is null)
            {
                errs.Add("Registration did not succeed. Please register again.");
                bd_util.set_flash_errs(HttpContext, errs);
                Response.Redirect("Register");
            }
            else
            {
                dict["@us_username"] = (string)dr_registration["rr_username"];
                dict["@us_email_address"] = (string)dr_registration["rr_email_address"];
                dict["@us_password"] = (string)dr_registration["rr_password"];

                sql = "select 1 from users where us_username = @us_username";

                if (bd_util.is_username_already_taken((string)dr_registration["rr_username"]))
                {
                    errs.Add("Already registered. Did you click twice?");
                }
                else
                {

                    bool is_active = true;
                    bool is_report_only = false;

                    if (bd_config.get(bd_config.NewUserStartsInactive) == 1)
                        is_active = false;

                    if (bd_config.get(bd_config.NewUserStartsReportOnly) == 1)
                        is_report_only = true;

                    dict["@us_username"] = (string)dr_registration["rr_username"];
                    dict["@us_email_address"] = (string)dr_registration["rr_email_address"];
                    dict["@us_password"] = (string)dr_registration["rr_password"];
                    dict["@us_is_active"] = is_active;
                    dict["@us_is_report_only"] = is_report_only;

                    // create user
                    sql = @"insert into users (us_username, us_email_address, us_password, us_is_active, us_is_report_only) 
                    values(@us_username, @us_email_address, @us_password, @us_is_active, @us_is_report_only) 
                    returning us_id";

                    bd_db.exec_scalar(sql, dict);

                    bd_util.set_flash_msg(HttpContext, "Registration was succesful.");

                    is_success = true;
                }

            }
        }
    }
}
