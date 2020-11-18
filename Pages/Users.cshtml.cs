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
            DataSet ds = new DataSet();
            string sql = "select * from users";
            dt = db_util.get_datatable(sql);

        }

    }
}
