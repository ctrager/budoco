using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data;

namespace budoco.Pages
{
    public sealed class ReportsAdminModel : PageModel
    {
        public DataTable dt;
        public string singular_table_name = "Report";

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

        public void OnPost(int delete_id)
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string sql = "delete from reports where rp_id = " + delete_id.ToString();
            bd_db.exec(sql);
            bd_util.set_flash_msg(HttpContext, "Delete was successful");

            OnGet();
        }
    }
}
