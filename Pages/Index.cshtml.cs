using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace budoco.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public string msg;

        public void OnGet()
        {
            DataTable dt = (DataTable)MyCache.Get(HttpContext.Session.Id + ":dt");

            if (dt != null)
            {
                msg = dt.Rows.Count.ToString();
            }
            else
            {
                msg = "dt is null";
            }
        }
    }
}
