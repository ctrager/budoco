using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public int id;
        public string username;

        public void OnGet(int id)
        {
            this.id = id; //bd_util.get_int_or_zero_from_string(Request.Query["id"]);

            if (id != 0)
            {
                string sql = "select us_id, us_username from users where us_id = " + id.ToString();

                DataTable dt = db_util.get_datatable(sql);

                this.username = (string)dt.Rows[0]["us_username"];
            }
        }

        public void OnPost(int id, string username)
        {
            string sql;
            var dict = new Dictionary<string, dynamic>();

            if (id == 0)
            {
                sql = "insert into users (us_username) values(@us_username) returning us_id";
                dict["@us_username"] = username;
                this.id = (int)db_util.exec_scalar(sql, dict);
            }
            else
            {
                sql = "update users set us_username = @us_username where us_id = @us_id;";
                dict["@us_id"] = id;
                dict["@us_username"] = username;
                db_util.exec(sql, dict);
                this.id = id;
            }

            this.username = username;
        }
    }
}
