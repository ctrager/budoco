using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;

namespace budoco.Pages.Report
{
    public sealed class IndexModel : PageModel
    {
        public DataTable dt;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN) /*|| security.user.can_use_reports || security.user.can_edit_reports*/)
            {
                return;
            }

            string sql = @"
                select
                    rp_id,
                    rp_desc,
                    rp_chart_type
                from
                    reports
                order
                    by rp_desc";

            dt = bd_db.get_datatable(sql);
        }

        public IEnumerable<string> Columns()
        {
            yield return "Id";
            yield return "Name";
            yield return "View Chart";
            yield return "View Data";

            if (bd_util.is_user_admin(HttpContext) /*|| security.user.can_edit_reports*/)
            {
                yield return "Action";
            }
        }
    }
}
