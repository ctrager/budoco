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
            Console.WriteLine("qq admin ctor");
            Response.Body.Write(UnicodeEncoding.UTF8.GetBytes("this is response.body.write "));
            Response.CompleteAsync();
     
        }

        public void OnGet()
        {
            Console.WriteLine("qq admin onget  ");
        }
    }
}
