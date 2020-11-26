using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Serilog;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace budoco.Pages
{
    public class IssuesModel : PageModel
    {

        public DataTable dt;

        [FromQuery]
        public int page { get; set; }

        [FromQuery]
        public int sort { get; set; }

        [FromQuery]
        public string dir { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> queries { get; set; }
        [FromQuery]
        public int query_id { get; set; }

        public void OnGet()
        {

            if (!bd_util.check_user_permissions(HttpContext))
                return;

            if (page == 0)
            {
                page = 1;
            }

            if (dir is null)
            {
                dir = "";
            }



            // which query?
            DataRow query_row;
            const int ID = 0;
            const int SQL = 1;

            if (query_id == 0)
            {
                // get from session?
                // save so that when we navigte back, we are in same place
                object obj = HttpContext.Session.GetInt32("issues_query_id");
                if (obj is not null)
                {
                    query_id = (int)obj;
                    obj = (int)HttpContext.Session.GetInt32("issues_sort");
                    sort = (int)obj;
                    obj = (int)HttpContext.Session.GetInt32("issues_page");
                    page = (int)obj;
                    dir = (string)HttpContext.Session.GetString("issues_dir");
                }
            }

            // first query or previously selected query
            queries = bd_db.prepare_select_list("select qu_id, qu_name from queries order by qu_sort_seq, qu_name");
            if (query_id == 0)
            {
                // first query
                query_row = bd_db.get_datarow(
                    "select qu_id, qu_sql from queries order by qu_sort_seq, qu_name limit 1");
                query_id = (int)query_row[ID];
                sort = 0;
                page = 1;
                dir = "";

            }
            else
            {
                // previously selected query
                query_row = bd_db.get_datarow(
                    "select qu_id, qu_sql from queries where qu_id = " + query_id.ToString());

                if (query_row is null) // bad query id from session or url?
                {
                    // first query
                    query_row = bd_db.get_datarow(
                        "select qu_id, qu_sql from queries order by qu_sort_seq, qu_name limit 1");
                    query_id = (int)query_row[ID];
                    sort = 0;
                    page = 1;
                    dir = "";
                }
            }

            // save so that when we navigte back, we are in same place
            HttpContext.Session.SetInt32("issues_query_id", query_id);
            HttpContext.Session.SetInt32("issues_page", page);
            HttpContext.Session.SetInt32("issues_sort", sort);
            HttpContext.Session.SetString("issues_dir", dir);

            string sql = (string)query_row[SQL];

            // trim order by clause and replace with ours  
            if (sort > 0)
            {
                if (dir != "asc" && dir != "desc")
                {
                    dir = "asc";
                }
                int start_of_order_by = sql.IndexOf("order by");
                sql = sql.Substring(0, start_of_order_by);
                // sort is 0 based everywhere but the sql
                sql += "order by " + (sort + 1).ToString() + " " + dir;
            }

            sql = sql.Replace("$ME", HttpContext.Session.GetInt32("us_id").ToString());

            dt = bd_db.get_datatable(sql);
        }
    }
}
