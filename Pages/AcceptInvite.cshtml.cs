using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;
using Serilog;
using System;

namespace budoco.Pages
{
    public class AcceptInviteModel : PageModel
    {

        // bindings start 
        [FromQuery]
        public string guid { get; set; }

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        [BindProperty]
        public string retyped_password { get; set; }

        [BindProperty]
        public string email { get; set; }

        public void OnGet()
        {

        }
    }
}
