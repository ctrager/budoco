using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace budoco.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel()
        {
            Log.Information("IndexModel ctor");
        }

        public void OnGet()
        {
            HttpContext.Session.SetString("dummy", "dummy"); // to solve problem of session changing?

            string sql = "select * from sessions where se_id = '" + HttpContext.Session.Id + "'";

            DataRow dr = db_util.get_datarow(sql);
            if (dr == null)
            {
                Response.Redirect("/Login");
            }
            else
            {
                Response.Redirect("/Admin/Users");
            }

        }
    }
}
