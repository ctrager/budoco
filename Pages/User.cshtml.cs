using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Data;

namespace budoco.Pages
{
    public class UserModel : PageModel
    {

        private readonly ILogger<UserModel> _logger;

        public UserModel(ILogger<UserModel> logger)
        {
            _logger = logger;
        }
        [BindProperty]
        public int id { get; set; }
        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string email { get; set; }

        public string flash;

        public void OnGet(int id)
        {
            this.id = id; //bd_util.get_int_or_zero_from_string(Request.Query["id"]);

            if (id != 0)
            {
                string sql = "select * from users where us_id = " + id.ToString();

                DataTable dt = db_util.get_datatable(sql);

                this.username = (string)dt.Rows[0]["us_username"];
                this.email = (string)dt.Rows[0]["us_email"];
            }
        }

        public void OnPost()
        {
            if (!IsValid(username, email))
            {
                return;
            }

            string sql;
            var dict = new Dictionary<string, dynamic>();

            if (id == 0)
            {
                sql = "insert into users (us_username) values(@us_username) returning us_id";
                dict["@us_username"] = username;
                this.id = (int)db_util.exec_scalar(sql, dict);
                bd_util.set_flash_msg(HttpContext, "Create was successful");
                Response.Redirect("User?id=" + this.id.ToString());

            }
            else
            {
                sql = "update users set us_username = @us_username where us_id = @us_id;";
                dict["@us_id"] = id;
                dict["@us_username"] = username;
                db_util.exec(sql, dict);
                bd_util.set_flash_msg(HttpContext, "Update was successful");
            }

        }

        bool IsValid(string username, string email)
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
