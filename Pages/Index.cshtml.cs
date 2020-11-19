using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Data;
using Npgsql;

namespace budoco.Pages
{
    public class IndexModel : PageModel
    {
        public string foo;
        public string line1;
        public string line2;
        public string line3;

        public DataTable dt;

        private readonly ILogger<IndexModel> _logger;

        //private Microsoft.AspNetCore.Http.Ses;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Console.WriteLine("corey was here");

            foo = "hello corey 222";

            //var sess = Session;
            //HttpContext context = HttpContext.Current;
            IEnumerable<string> keys = HttpContext.Session.Keys;
            string keys_string = keys.ToString();

            int my_count = 0;

            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("my_count")))
            {
                my_count = (int)HttpContext.Session.GetInt32("my_count");
            }

            my_count++;

            HttpContext.Session.SetInt32("my_count", my_count);
            HttpContext.Session.SetString("foo", "bar");

            foo = my_count.ToString();

            foreach (string k in keys)
            {
                foo += ", ";
                foo += k;
            }

            line1 = "line1 - 133 66";
            line2 = Startup.cnfg["Btnet:DbConnectionString"];
            DataSet ds = new DataSet();
            string sql = "select * from corey_table";
            using (var conn = new NpgsqlConnection(Startup.cnfg["Btnet:DbConnectionString"]))
            {
                conn.Open();
                var da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
            }
            dt = ds.Tables[0];

            /*             foreach (DataRow dr in ds1.Tables[0].Rows)
                        {
                            Console.WriteLine(dr[1].ToString());
                        } */

        }
        public string GetSomeHtml()
        {
            Console.WriteLine("this is GetSomeHtml  "); return "<h1>corey was here</h1>";
        }
    }
}
