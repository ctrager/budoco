using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;

namespace budoco.Pages
{
    public sealed class ReportsModel : PageModel
    {
        public DataTable dt;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext))
                return;

            string sql = @"
                select
                    rp_id as ""ID"",
                    rp_name as ""Name"",
                    rp_chart_type as ""Chart Type""
                from
                    reports
                order
                    by rp_name";

            dt = bd_db.get_datatable(sql);
        }

    }
}
