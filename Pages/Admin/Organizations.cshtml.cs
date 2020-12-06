using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;

namespace budoco.Pages
{
    public class OrganizationsModel : PageModel
    {

        public DataTable dt;
        public string singular_table_name = "Organization";

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;


            string sql = @"select 
            og_id as ""ID"",
            og_name as ""Name"",
            og_is_active as ""Active"",
            og_is_default as ""Default"",
            count as ""Number of Users""
            from organizations 
            left outer join (select us_organization, count(*) as count from users 
            group by us_organization) count_subquery on og_id = us_organization
            order by og_name";

            dt = bd_db.get_datatable(sql);
        }
    }
}
