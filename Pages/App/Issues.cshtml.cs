using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        string qu_sql;

        public void OnGet()
        {

            if (page == 0)
            {
                page = 1;
            }

            if (dir is null)
            {
                dir = "";
            }

            // which query?

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

            // prepare dropdown
            queries = bd_db.prepare_select_list(
                @"select qu_id, qu_name from queries 
                where qu_is_active
                order by qu_is_default desc, qu_name asc");

            if (query_id == 0)
            {
                use_default_query();
            }
            else
            {
                // previously selected query
                DataRow query_row = bd_db.get_datarow(
                    "select qu_id, qu_sql from queries where qu_is_active and qu_id = " + query_id.ToString());

                if (query_row is null) // bad query id from session or url?
                {
                    use_default_query();
                }
                else
                {
                    qu_sql = (string)query_row["qu_sql"];
                }
            }

            // save so that when we navigte back, we are in same place
            HttpContext.Session.SetInt32("issues_query_id", query_id);
            HttpContext.Session.SetInt32("issues_page", page);
            HttpContext.Session.SetInt32("issues_sort", sort);
            HttpContext.Session.SetString("issues_dir", dir);

            // qu_sql is original from db
            // sql - we are going to alter it
            var sql = qu_sql;

            // trim order by clause and replace with ours  
            if (sort > -1)
            {
                if (dir != "asc" && dir != "desc")
                {
                    dir = "asc";
                }
                int start_of_order_by = sql.IndexOf("order by");
                if (start_of_order_by > 0)
                {
                    sql = sql.Substring(0, start_of_order_by);
                    // sort is 0 based everywhere but the sql
                    sql += "order by " + (sort + 1).ToString() + " " + dir;
                }
                else
                {
                    sql += " order by " + (sort + 1).ToString() + " " + dir;
                }
            }

            sql = bd_util.enhance_sql_per_user(HttpContext, sql);

            dt = bd_db.get_datatable(sql);

            // cache the list for the prev/next links in issue.cshtml
            var issue_list = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                issue_list.Add((int)row[0]);
            }
            bd_session.Set(HttpContext, "issue_list", issue_list);

        }

        void use_default_query()
        {
            // first query
            DataRow query_row = bd_db.get_datarow(
                @"select qu_id, qu_sql from queries 
                where qu_is_active
                order by qu_is_default desc, qu_name asc limit 1");
            query_id = (int)query_row["qu_id"];
            sort = -1;
            page = 1;
            dir = "";
            qu_sql = (string)query_row["qu_sql"];

        }
    }
}
