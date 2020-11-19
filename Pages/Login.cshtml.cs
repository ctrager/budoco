using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Serilog;

namespace budoco.Pages
{
    public class LoginModel : PageModel
    {


        // bindings start 
        [BindProperty]
        public int id { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        // bindings end  

        public List<string> errs = new List<string>();

        public void OnPost(string action)
        {
            if (action is not null) {
                Signout();
                return;
            }

            // check user and password
            string sql = @"select * from users where 
            (us_username = @us_username or us_email = @us_email) 
            and us_password = @us_password";

            var dict = new Dictionary<string, dynamic>();

            dict["@us_username"] = username;
            dict["@us_email"] = username; // on purpose, user can login typing either
            dict["@us_password"] = password;

            DataRow user_dr = db_util.get_datarow(sql, dict);

            if (user_dr is null)
            {
                errs.Add("User or Password incorrect");
            }
            else
            {
                // login 

                dict.Clear();

                sql = @"insert into sessions (se_id, se_user)
                values (@se_id, @se_user)";

                dict["@se_id"] = HttpContext.Session.Id;
                dict["@se_user"] = user_dr[0];
                db_util.exec(sql, dict);
                Response.Redirect("Issues");
            }

        }

        public void Signout() 
        {
            string sql = "delete from sessions where se_id = '" + HttpContext.Session.Id + "'";
            db_util.exec(sql);
            Response.Redirect("/");
        }
    }
}
