using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace budoco.Pages
{
    public class UsersModel : PageModel
    {
        private readonly ILogger<UsersModel> _logger;

        public UsersModel(ILogger<UsersModel> logger)
        {
            _logger = logger;
        }

        public DataTable dt;

        public void OnGet()
        {
            bd_util.redirect_if_not_logged_in(HttpContext);
            GetUsers();
        }


        public void OnPost(int delete_id)
        {
            bd_util.console_write_line("deleting");
            string sql = "delete from users where us_id = @us_id;";
            var dict = new Dictionary<string, dynamic>();
            dict["@us_id"] = delete_id;
            bd_db.exec(sql, dict);
            bd_util.set_flash_msg(HttpContext, "Delete was successful");
            GetUsers();
        }

        void GetUsers()
        {
            DataSet ds = new DataSet();
            string sql = "select * from users";
            dt = bd_db.get_datatable(sql);
        }
    }
}
