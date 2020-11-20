using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace budoco.Pages
{
    public class IssuesModel : PageModel
    {

        public void OnGet()
        {
            bd_util.redirect_if_not_logged_in(HttpContext);

        }
    }
}
