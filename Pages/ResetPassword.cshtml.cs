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
    public class ResetPasswordModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public string guid { get; set; }

        [BindProperty]
        public string password { get; set; }

        [BindProperty]
        public string retyped_password { get; set; }

        [BindProperty]
        public string email { get; set; }

        List<string> errs = new List<string>();
        Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();

        public string username;
        int user_id;

        public void OnGet()
        {
            DataRow dr = GetUserUsingGuid();
            if (dr is not null)
            {
                user_id = (int)dr["us_id"];
                username = (string)dr["us_username"];
                email = (string)dr["us_email_address"];
            }
        }

        public void OnPost(string password)
        {
            // don't allow user to update ANY user via this page, 
            // just the one associated with the guid we sent in the email   
            DataRow dr = GetUserUsingGuid();
            if (dr is not null)
            {
                user_id = (int)dr["us_id"];
                username = (string)dr["us_username"];
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errs.Add("Password is required.");
                bd_util.set_flash_errs(HttpContext, errs);
                return;
            }

            if (string.IsNullOrWhiteSpace(retyped_password))
            {
                errs.Add("Please enter your new password twice.");
                bd_util.set_flash_errs(HttpContext, errs);
                return;
            }

            if (password != retyped_password)
            {
                errs.Add("The passwords don't match. Please enter your new password twice.");
                bd_util.set_flash_errs(HttpContext, errs);
                return;
            }

            string sql;

            if (username == "admin")
                sql = "update users set us_password = @us_password, us_email_address = @us_email_address where us_id = @us_id";
            else
                sql = "update users set us_password = @us_password where us_id = @us_id";

            dict["@us_password"] = bd_util.compute_password_hash(password);
            dict["@us_id"] = user_id;
            if (email is not null)
            {
                dict["@us_email_address"] = email;
            }

            bd_db.exec(sql, dict);

            bd_util.set_flash_msg(HttpContext, "Password has been reset successfully.");

            Response.Redirect("/Index");
        }

        DataRow GetUserUsingGuid()
        {
            string sql = @"select us_id, us_username, us_email_address from reset_password_requests 
            inner join users on us_id = rp_user_id
            where rp_guid = @rp_guid";

            dict["@rp_guid"] = guid;
            DataRow dr_reset = bd_db.get_datarow(sql, dict);

            if (dr_reset is null)
            {
                errs.Add("Password reset failed. Please try again.");
                bd_util.set_flash_errs(HttpContext, errs);
                Response.Redirect("/ResetPasswordRequest");

            }

            return dr_reset;

        }
    }
}
