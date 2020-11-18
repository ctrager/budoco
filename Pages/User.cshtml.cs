using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
//using System.Data.SqlClient;
using System.Data.Common;
//using Npgsql.Sq
using Npgsql;
using NpgsqlTypes;

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

        public void OnGet()
        {
            id = bd_util.get_int_or_zero_from_string(Request.Query["id"]);

            string sql = "select us_id, us_username from users where us_id = " + id.ToString();

            DataTable dt = db_util.get_datatable(sql);
            username = (string)dt.Rows[0]["us_username"];

        }

        public void OnPost(int id, string username)
        {
            string sql = "update users set us_username = '" + username + "' where us_id = " + id;
            db_util.exec(sql);
            this.id = id;
            this.username = username;
        }

    }
}
