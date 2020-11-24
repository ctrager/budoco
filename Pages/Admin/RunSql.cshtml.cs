using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.AspNetCore.Mvc;

namespace budoco.Pages
{
    public class RunSqlModel : PageModel
    {

        [BindProperty]
        public string sql { get; set; }

        public DataTable dt;

        public Exception exception;

        public void OnGet()
        {

        }

        public void OnPost()
        {
            // if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
            //     return;

            dt = null;
            exception = null;

            try
            {
                dt = bd_db.get_datatable(sql);
            }
            catch (Exception e)
            {
                exception = e;
            }
        }
    }
}
