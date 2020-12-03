using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace budoco.Pages
{
    public class UsersModel : PageModel
    {

        public DataTable dt;
        public string singular_table_name = "User";


        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            //DataSet ds = new DataSet();
            string sql = @"select 
            us_id as ""ID"",
            us_username as ""Username"",
            us_email_address as ""Email"",
            us_is_admin as ""Admin"",
            us_is_active as ""Active"",
            us_is_report_only as ""Report Only"",
            coalesce(og_name, '') as ""Organization"",
            us_created_date as ""Created""
            from users
            left outer join organizations on og_id = us_organization";
            dt = bd_db.get_datatable(sql);
        }
    }
}
