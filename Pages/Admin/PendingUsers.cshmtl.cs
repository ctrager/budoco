using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;
using System.Collections.Generic;

namespace budoco.Pages
{
    public class PendingUsersModel : PageModel
    {

        [BindProperty]
        public string action { get; set; }

        [BindProperty]
        public int delete_id { get; set; }

        public string singular_table_name = "Entry";
        public DataTable dt;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string sql = @"select 
            rr_id as ""ID"",
            rr_created_date as ""Date Created"",
            rr_username as ""Username"",
            rr_email_address as ""Email"",
            case when rr_is_invitation = true then 'Invitation' else 'Registration' end as ""Type"",
            rr_organization as ""Organization""
            from registration_requests order by rr_created_date desc";

            dt = bd_db.get_datatable(sql);
        }

        public void OnPost()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string sql = "delete from registration_requests where rr_id = @rr_id";
            var dict = new Dictionary<string, dynamic>();
            dict["@rr_id"] = delete_id;
            bd_db.exec(sql, dict);
            bd_util.set_flash_msg(HttpContext, "Delete was successful");

            OnGet();
        }

    }
}
