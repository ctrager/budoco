using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace budoco.Pages.Report
{
    public sealed class DeleteModel : PageModel
    {
        public DataRow dr;

        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string name { get; set; }

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN) /*|| security.user.can_edit_reports*/)
            {
                return;
            }

            var sql = $"select rp_id, rp_desc from reports where rp_id=@id";
            var dict = new Dictionary<string, object>
            {
                ["@id"] = id
            };

            dr = bd_db.get_datarow(sql, dict);

            name = (string)dr["rp_desc"];
        }

        public void OnPost()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN) /*|| security.user.can_edit_reports*/)
            {
                return;
            }

            // do delete here
            var sql = $"delete from reports where rp_id=@id;";
            var dict = new Dictionary<string, object>
            {
                ["@id"] = id
            };

            bd_db.exec(sql, dict);

            Response.Redirect("/Report/Index");
        }
    }
}
