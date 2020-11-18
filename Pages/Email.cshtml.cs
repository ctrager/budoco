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

        public void OnPost(string to, string from, string subject, string body)
        {
            var message = new MimeMessage ();
        
			message.To.Add (new MailboxAddress ("corey gmail", "ctrager@gmail.com"));
			message.From.Add (new MailboxAddress ("corey yahoo", "ctrager@yahoo.com"));
			message.Subject = subject;

			message.Body = new TextPart ("plain") {
				Text = body
			};

/*

	
smtp.gmail.com

Requires SSL: Yes

Requires TLS: Yes (if available)

Requires Authentication: Yes

Port for SSL: 465

Port for TLS/STARTTLS: 587

Server - smtp.mail.yahoo.com
Port - 465 or 587
Requires SSL - Yes
Requires TLS - Yes (if available)
Requires authentication - Yes
Your login info

Email address - Your full email address (name@domain.com.)
Password - Your account's password.
Requires authentication - Yes
*/

			using (var client = new MailKit.Net.Smtp.SmtpClient()) {
				client.Connect(
                    //"smtp.mail.yahoo.com", 465, true);
                    //"smtp.mail.yahoo.com", 587, true);
                    "smtp.mail.yahoo.com", 587, MailKit.Security.SecureSocketOptions.Auto);
                    //"smtp.mail.yahoo.com", 587, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    
                    //"smtp.mail.yahoo.com", 587, true);
                    //465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                    //567, MailKit.Security.SecureSocketOptions.SslOnConnect);
 
				// Note: only needed if the SMTP server requires authentication
				client.Authenticate ("ctrager@yahoo.com", "");

				client.Send (message);
				client.Disconnect (true);
			}
        }
    }
}
