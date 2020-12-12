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
            string sql = @"select 
            og_id as ""ID"",
            og_name as ""Name"",
            og_is_active as ""Active"",
            og_is_default as ""Default"",
            user_count as ""Number of Users"",
            issue_count as ""Number of Issues""
            from organizations 
            
            left outer join (select us_organization, count(*) as user_count from users 
            group by us_organization) user_count_subquery on og_id = us_organization
            
            left outer join (select i_organization, count(*) as issue_count from issues 
            group by i_organization) issue_count_subquery on og_id = i_organization
            
            order by og_name";

            dt = bd_db.get_datatable(sql);
        }
    }
}
