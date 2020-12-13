using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

namespace budoco.Pages
{
    public class GitLogModel : PageModel
    {
        public ActionResult OnGet()
        {
            byte[] bytea = System.IO.File.ReadAllBytes("git_log.txt");

            return File(bytea, "text/plain");
        }
    }
}
