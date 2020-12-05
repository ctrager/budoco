using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace budoco.Pages
{
    public class UserSettingsModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string email_address { get; set; }

        [BindProperty]
        public string organization { get; set; }

        int user_id;

        // bindings end        

        public void OnGet()
        {
            bd_util.check_user_permissions(HttpContext);

            user_id = bd_util.get_user_id_from_session(HttpContext);

            if (user_id == 0)
            {
                bd_util.set_flash_err(HttpContext, "You need to sign in.");
                Response.Redirect("Login");
                return;
            }

            string sql = @"select 
                us_username, us_email_address, coalesce(og_name,'') as organization
                from users 
                left outer join organizations on us_organization = og_id
                where us_id = " + user_id.ToString();

            DataRow dr = bd_db.get_datarow(sql);

            if (dr is not null)
            {
                username = (string)dr["us_username"];
                email_address = (string)dr["us_email_address"];
                organization = (string)dr["organization"];
            }
        }

        public void OnPost()
        {

            bd_util.check_user_permissions(HttpContext);

            user_id = bd_util.get_user_id_from_session(HttpContext);

            if (user_id == 0)
            {
                bd_util.set_flash_err(HttpContext, "You need to sign in.");
                Response.Redirect("Login");
                return;
            }

            if (!IsValid())
            {
                return;
            }

            string sql = @"update users set 
                us_username = @us_username,
                us_email_address = @us_email_address
                where us_id = @us_id";

            var dict = new Dictionary<string, dynamic>();

            dict["@us_id"] = user_id;
            dict["@us_username"] = username;
            dict["@us_email_address"] = email_address;

            bd_db.exec(sql, dict);
            bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);
        }

        bool IsValid()
        {
            // TODO
            // DRY up this code 
            // Identica; in User and UserSettings
            // and similar in Register
            var errs = new List<string>();

            string sql;
            var dict = new Dictionary<string, dynamic>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username required.");
            }
            else
            {
                sql = "select 1 from users where us_username = @us_username and us_id != @us_id";

                dict["@us_username"] = username;
                dict["@us_id"] = user_id;

                if (bd_db.exists(sql, dict))
                {
                    errs.Add("Username is used by somebody else.");
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
                sql = "select 1 from users where us_email_address = @us_email_address and us_id != @us_id";

                dict["@us_email_address"] = email_address;
                dict["@us_id"] = user_id;

                if (bd_db.exists(sql, dict))
                {
                    errs.Add("Email is used by somebody else.");
                }
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
