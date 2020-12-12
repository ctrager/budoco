using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace budoco.Pages
{
    public class HtmlPartModel : PageModel
    {
        [FromQuery]
        public int p_id { get; set; }

        public ContentResult OnGet()
        {
            bd_util.check_user_permissions(HttpContext);

            string html = (string)bd_db.exec_scalar("select p_email_from_html_part from posts where p_id = "
                + p_id.ToString());

            if (html is not null)
            {
                var content = Content((string)html);
                content.ContentType = "text/html";
                return content;
            }
            else
            {
                return null;
            }
        }
    }
}
