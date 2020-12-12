using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;

namespace budoco.Pages
{
    public class CreateIssueModel : PageModel
    {

        [BindProperty]
        public string username { get; set; }

        [BindProperty]
        public string password { get; set; }

        [BindProperty]
        public string description { get; set; }

        [BindProperty]
        public string image_data { get; set; }

        public ActionResult OnPost()
        {
            Console.WriteLine("QQQQQQQQQQQQQQQQQQQ");

            int issue_id = bd_issue.create_issue(description, "posted by Budoco screenshot", 1);

            return null;
        }
    }
}
