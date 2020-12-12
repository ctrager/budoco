using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;

namespace budoco.Pages
{
    public class EmailQueueModel : PageModel
    {

        [BindProperty]
        public string action { get; set; }

        [BindProperty]
        public string delete_id { get; set; }

        public string singular_table_name = "Entry";
        public DataTable dt;

        public void OnGet()
        {
            string sql = @"select 
            oq_id as ""ID"",
            oq_date_created as ""Date Created"",
            oq_email_type as ""Email Type"",
            oq_post_id as ""Post ID"",
            oq_sending_attempt_count as ""Number of Send Attempts"",
            oq_last_sending_attempt_date as ""Most Recent Send Attempt"",
            oq_last_exception as ""Last Error"",
            oq_email_to as ""Email To"",
            oq_email_subject as ""Email Subject""
            from outgoing_email_queue order by oq_date_created desc";

            dt = bd_db.get_datatable(sql);
        }

        public void OnPost()
        {
            if (action == "reset_counts")
            {
                string sql = "update outgoing_email_queue set oq_sending_attempt_count = 0";
                bd_db.exec(sql);
                bd_util.set_flash_msg(HttpContext, "Reset was successful.");
            }
            else
            if (action == "retry")
            {
                bd_email.spawn_email_sending_thread();
                bd_util.set_flash_msg(HttpContext, "Trying to resend now...");
            }
            else
            if (action == "delete")
            {
                string sql = "delete from outgoing_email_queue where oq_id = " + delete_id.ToString();
                bd_db.exec(sql);
                bd_util.set_flash_msg(HttpContext, "Delete was successful");
            }

            OnGet();
        }

    }
}
