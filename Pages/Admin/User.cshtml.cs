using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;

namespace budoco.Pages
{
    public class UserModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string email { get; set; }

        [BindProperty]
        public bool is_admin { get; set; }

        [BindProperty]
        public bool is_active { get; set; }

        [BindProperty]
        public bool is_report_only { get; set; }

        // bindings end        

        public void OnGet()
        {

            bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN);

            string sql = "select * from users where us_id = " + id.ToString();

            DataRow dr = bd_db.get_datarow(sql);

            username = (string)dr["us_username"];
            email = (string)dr["us_email"];
            is_admin = (bool)dr["us_is_admin"];
            is_active = (bool)dr["us_is_active"];
            is_report_only = (bool)dr["us_is_report_only"];
        }

        public void OnPost()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            if (!IsValid())
            {
                return;
            }

            string sql;


            sql = @"update users set 
                us_username = @us_username,
                us_email = @us_email,
                us_is_admin = @us_is_admin,
                us_is_active = @us_is_active,
                us_is_report_only = @us_is_report_only
                where us_id = @us_id;";

            bd_db.exec(sql, GetValuesDict());
            bd_util.set_flash_msg(HttpContext, "Update was successful");
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@us_id"] = id;
            dict["@us_username"] = username;
            dict["@us_email"] = email;
            dict["@us_is_admin"] = is_admin;
            dict["@us_is_active"] = is_active;
            dict["@us_is_report_only"] = is_report_only;

            return dict;
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username is required.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                errs.Add("Email is required");
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
