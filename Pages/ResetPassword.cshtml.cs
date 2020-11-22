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

        List<string> errs = new List<string>();
        Dictionary<string, dynamic> dict = new Dictionary<string, dynamic>();

        public void OnGet()
        {
            GetUserIdUsingGuid();
        }

        public void OnPost(string password)
        {
            int user_id = GetUserIdUsingGuid();
            if (user_id == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errs.Add("Password is required.");
                bd_util.set_flash_err(HttpContext, errs);
                return;
            }

            if (string.IsNullOrWhiteSpace(retyped_password))
            {
                errs.Add("Please enter your new password twice.");
                bd_util.set_flash_err(HttpContext, errs);
                return;
            }

            if (password != retyped_password)
            {
                errs.Add("The passwords don't match. Please enter your new password twice.");
                bd_util.set_flash_err(HttpContext, errs);
                return;
            }

            string sql = "update users set us_password = @us_password where us_id = @us_id";

            dict["@us_password"] = bd_util.compute_password_hash(password);
            dict["@us_id"] = user_id;

            bd_db.exec(sql, dict);

            bd_util.set_flash_msg(HttpContext, "Password has been reset successfully.");
        }

        int GetUserIdUsingGuid()
        {
            string sql = "select * from emailed_links where el_guid = @el_guid";

            dict["@el_guid"] = guid;
            DataRow dr_reset = bd_db.get_datarow(sql, dict);

            if (dr_reset is null)
            {
                errs.Add("Password reset failed. Please try again.");
                bd_util.set_flash_err(HttpContext, errs);
                Response.Redirect("ResetPasswordRequest");
                return 0;
            }
            else
            {
                return (int)dr_reset["el_user_id"];
            }

        }
    }
}
