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

        [BindProperty]
        public IEnumerable<SelectListItem> queries { get; set; }
        [FromQuery]
        public int query_id { get; set; }


        public void OnGet()
        {

            bd_util.redirect_if_not_logged_in(HttpContext);

            if (page == 0)
            {
                page = 1;
            }

            // which query?
            DataRow query_row;
            const int ID = 0;
            const int SQL = 1;

            queries = db_util.prepare_select_list("select qu_id, qu_name from queries order by qu_sort_seq, qu_name");
            if (query_id == 0)
            {
                query_row = db_util.get_datarow(
                    "select qu_id, qu_sql from queries order by qu_sort_seq, qu_name limit 1");
                query_id = (int)query_row[ID];
            }
            else
            {
                query_row = db_util.get_datarow(
                    "select qu_id, qu_sql from queries where qu_id = " + query_id.ToString());

            }

            string sql = (string)query_row[SQL];

            dt = db_util.get_datatable(sql);
        }
    }
}
