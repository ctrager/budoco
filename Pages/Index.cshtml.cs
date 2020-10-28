using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;


namespace net_razor.Pages
{
    public class IndexModel : PageModel
    {
        public string foo;
        
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Console.WriteLine("corey was here");
            foo = "bar nice 23 xxx 4";
        }
        public string GetSomeHtml()
        {
            Console.WriteLine("this is GetSomeHtml");
            return "<h1>corey was here</h1>";    
        }
    }
}
