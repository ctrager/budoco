using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text;
using MailKit;
using MimeKit;

namespace budoco.Pages
{
    public class EmailModel : PageModel
    {
        public string to;
        public string from;
        public string subject;
        public string body;

        private readonly ILogger<AdminModel> _logger;

        public EmailModel(ILogger<AdminModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public string email_result;
        public void OnPost(string to, string from, string subject, string body)
        {
            email_result = bd_util.send_email(to, from, subject, body);
        }
    }
}
