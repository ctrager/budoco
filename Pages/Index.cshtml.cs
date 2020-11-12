using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;



namespace net_razor.Pages
{
    public class IndexModel : PageModel
    {
        public string foo;      
        private readonly ILogger<IndexModel> _logger;

        //private Microsoft.AspNetCore.Http.Ses;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Console.WriteLine("corey was here");
            
            foo = "hello";

             //var sess = Session;
            //HttpContext context = HttpContext.Current;
            IEnumerable<string> keys = HttpContext.Session.Keys;
            string keys_string = keys.ToString();
            
            int my_count = 0;

            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("my_count")))
            {
                my_count = (int)HttpContext.Session.GetInt32("my_count");
            }

            my_count++;

            HttpContext.Session.SetInt32("my_count", my_count);
            
            foo = my_count.ToString();
            
            foreach (string k in keys) 
            {
                foo += ", ";
                foo += k;    
            }
             
        }
        public string GetSomeHtml()
        {
            Console.WriteLine("this is GetSomeHtml");
            return "<h1>corey was here</h1>";    
        }
    }
}
