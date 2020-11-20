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

namespace budoco.Pages
{
    public class IssuesModel : PageModel
    {

        public DataTable dt;

        [FromQuery]
        public int page { get; set; }

        public void OnGet()
        {
            if (page == 0)
            {
                page = 1;
            }

            //bd_util.redirect_if_not_logged_in(HttpContext);

            DataSet ds = new DataSet();

            string sql = @"select i_id as ""Issue Id"", i_desc as ""desc"" 
            from issues order by i_id asc";

            dt = db_util.get_datatable(sql);
        }
    }
}
