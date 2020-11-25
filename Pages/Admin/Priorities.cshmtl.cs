using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace budoco.Pages
{
    public class PrioritiesModel : PageModel
    {

        public DataTable dt;
        public string singular_table_name = "Priority";

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string sql = @"select * from priorities order by pr_name";
            dt = bd_db.get_datatable(sql);
        }
    }
}
