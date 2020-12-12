using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public string email_address { get; set; }

        [BindProperty]
        public bool is_admin { get; set; }

        [BindProperty]
        public bool is_active { get; set; }

        [BindProperty]
        public bool is_report_only { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> organizations { get; set; }
        [BindProperty]
        public int organization_id { get; set; }

        // bindings end        

        public void OnGet()
        {
            string sql = "select * from users where us_id = " + id.ToString();

            DataRow dr = bd_db.get_datarow(sql);

            username = (string)dr["us_username"];
            email_address = (string)dr["us_email_address"];
            is_admin = (bool)dr["us_is_admin"];
            is_active = (bool)dr["us_is_active"];
            is_report_only = (bool)dr["us_is_report_only"];
            organization_id = (int)dr["us_organization"];

            organizations = bd_db.prepare_select_list("select og_id, og_name from organizations order by og_name");

        }

        public void OnPost()
        {

            if (!IsValid())
            {
                return;
            }

            string sql;


            sql = @"update users set 
                us_username = @us_username,
                us_email_address = @us_email_address,
                us_is_admin = @us_is_admin,
                us_is_active = @us_is_active,
                us_is_report_only = @us_is_report_only,
                us_organization = @us_organization
                where us_id = @us_id;";

            bd_db.exec(sql, GetValuesDict());
            bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@us_id"] = id;
            dict["@us_username"] = username;
            dict["@us_email_address"] = email_address;
            dict["@us_is_admin"] = is_admin;
            dict["@us_is_active"] = is_active;
            dict["@us_is_report_only"] = is_report_only;
            dict["@us_organization"] = organization_id;

            return dict;
        }

        bool IsValid()
        {
            int user_id = id; // generica admin datatable partial likes just "id"

            // DRY up this code 
            // Identica; in User and UserSettings
            // and similar in Register
            var errs = new List<string>();

            var dict = new Dictionary<string, dynamic>();

            if (string.IsNullOrWhiteSpace(username))
            {
                errs.Add("Username required.");
            }
            else
            {
                if (bd_util.is_username_already_taken_not_by_me(username, id))
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
                if (bd_util.is_email_already_taken_not_by_me(email_address, id))
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
