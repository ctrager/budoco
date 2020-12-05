
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace budoco.Pages
{
    public class ResetPasswordRequestModel : PageModel
    {
        [BindProperty]
        public string email { get; set; }

        public void OnPost()
        {
            var errs = new List<string>();
            if (String.IsNullOrWhiteSpace(email))
            {

                errs.Add("Email address is required.");

            }
            else
            {

                // is email in our db?
                var sql = "select * from users where us_email_address = @us_email_address";
                var dict = new Dictionary<string, dynamic>();
                dict["@us_email_address"] = email;
                DataRow dr_user = bd_db.get_datarow(sql, dict);

                if (dr_user is not null)
                {

                    string guid = bd_util.insert_change_password_request_link(email, (int)dr_user["us_id"]);

                    string body = "Follow or browse to this link to reset password:\n"
                        + bd_config.get(bd_config.WebsiteUrlRootWithoutSlash)
                        + "/ResetPassword?guid="
                        + guid;

                    bd_email.queue_email("reset",
                        email, // to
                        bd_config.get(bd_config.AppName) + ": Reset Password", // subject
                        body);

                    bd_util.set_flash_msg(HttpContext, "An email with password reset instructions was sent to " + email + ".");

                }
                else
                {
                    errs.Add("The email address is not registered.");
                }
            }

            if (errs.Count > 0)
            {
                bd_util.set_flash_errs(HttpContext, errs);
            }
        }
    }
}
