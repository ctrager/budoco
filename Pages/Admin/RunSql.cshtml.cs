using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RazorPartialToString.Services;

namespace budoco.Pages
{
    public class RunSqlModel : PageModel
    {
        private readonly IRazorPartialToStringRenderer _renderer;

        public RunSqlModel(IRazorPartialToStringRenderer renderer)
        {
            _renderer = renderer;
        }

        [BindProperty]
        public string sql { get; set; }

        public DataTable dt;

        public string error;
        public string success;

        public void OnGet()
        {
            if (bd_config.get(bd_config.DebugEnableRunSql) != 1)
                return;

        }

        public async Task<ContentResult> OnPostRunAsync()
        {
            if (bd_config.get(bd_config.DebugEnableRunSql) == 0)
            {
                return Content("<div>DebugEnableRunSql: 0</div>");
            }
            dt = null;
            error = null;

            try
            {
                dt = bd_db.get_datatable(sql);
            }
            catch (Exception e)
            {

                if (e.Message == "Cannot find table 0.")
                {
                    // suppress this - this is just what happens when we run a query that does't SELECT 
                    // like an update   
                    success = "Success";
                }
                else
                {
                    error = e.Message;
                }
            }

            String html = await _renderer.RenderPartialToStringAsync("_PlainDataTablePartial", this);
            return Content(html);
        }
    }
}
