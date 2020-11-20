using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;

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
            bd_util.redirect_if_not_logged_in(HttpContext);

            // logged in so, redirect to main issues list
            Response.Redirect("/Issues");

        }
    }
}
