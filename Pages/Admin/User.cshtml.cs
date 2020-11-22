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
        [BindProperty]
        public int id { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string email { get; set; }

        [BindProperty]
        public bool is_admin { get; set; }
        // bindings end        

        public void OnGet(int id)
        {

            bd_util.redirect_if_not_logged_in(HttpContext);

            this.id = id;

            if (id != 0)
            {
                string sql = "select * from users where us_id = " + id.ToString();

                DataRow dr = bd_db.get_datarow(sql);

                username = (string)dr["us_username"];
                email = (string)dr["us_email"];
                is_admin = (bool)dr["us_is_admin"];
            }
        }

        public void OnPost()
        {
            if (!IsValid())
            {
                return;
            }

            string sql;


            if (id == 0)
            {
                sql = @"insert into users (us_username, us_email, us_is_admin) 
                values(@us_username, @us_email, @us_is_admin) 
                returning us_id";

                this.id = (int)bd_db.exec_scalar(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Create was successful");
                Response.Redirect("User?id=" + this.id.ToString());

            }
            else
            {
                sql = @"update users set 
                us_username = @us_username,
                us_email = @us_email,
                us_is_admin = @us_is_admin
                where us_id = @us_id;";

                bd_db.exec(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Update was successful");
            }
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@us_id"] = id;
            dict["@us_username"] = username;
            dict["@us_email"] = email;
            dict["@us_is_admin"] = is_admin;

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
                bd_util.set_flash_err(HttpContext, errs);
                return false;
            }
        }
    }
}
