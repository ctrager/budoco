using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text;

namespace net_razor.Pages
{
    public class AdminModel : PageModel
    {
        private readonly ILogger<AdminModel> _logger;

        public AdminModel(ILogger<AdminModel> logger)
        {
            _logger = logger;
            logger.LogInformation("corey qq is logging info");
            logger.LogWarning("corey qq is logging warning withtout");
            System.Diagnostics.Debug.WriteLine("corey qq writeline without add logging");
        }

        public void OnGet()
        {
            
            //Response.StatusCode = 403;
            //Response.Body.WriteAsync(UnicodeEncoding.UTF8.GetBytes("this is response.body.write 403"));
            //Response.CompleteAsync();
            ///Console.WriteLine("qq admin onget  ");
            //Response
            //Response.
        }
    }
}
