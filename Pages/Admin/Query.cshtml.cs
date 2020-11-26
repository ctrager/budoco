using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RazorPartialToString.Services;
using System.Data.Common;

namespace budoco.Pages
{
    public class QueryModel : PageModel
    {

        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string sql { get; set; }

        public DataTable dt;

        public string error;

        public void OnGet()
        {
            DataRow dr = bd_db.get_datarow("select * from queries where qu_id = " + id.ToString());
            sql = (string)dr["qu_sql"];

        }

    }
}
