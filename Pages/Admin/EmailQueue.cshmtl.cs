using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;

namespace budoco.Pages
{
    public class EmailQueueModel : PageModel
    {

        [BindProperty]
        public string queue_action { get; set; }

        public string singular_table_name = "Entry";
        public DataTable dt;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            string sql = @"select oq_id, oq_date_created, oq_email_type, 
            oq_post_id, oq_sending_attempt_count, oq_last_sending_attempt_date, oq_last_exception,
            oq_email_to, oq_email_subject
            from outgoing_email_queue order by oq_date_created desc";

            dt = bd_db.get_datatable(sql);
        }

        public void OnPost()
        {
            if (!bd_util.check_user_permissions(HttpContext, bd_util.MUST_BE_ADMIN))
                return;

            Console.WriteLine("queue action: " + queue_action);

            if (queue_action == "reset_counts")
            {
                string sql = "update outgoing_email_queue set oq_sending_attempt_count = 0";
                bd_db.exec(sql);
                bd_util.set_flash_msg(HttpContext, "Reset was successful.");
            }
            else
            if (queue_action == "delete")
            {
                string sql = "delete from outgoing_email_queue";
                bd_db.exec(sql);
                bd_util.set_flash_msg(HttpContext, "Delete was successful.");
            }
            else
            if (queue_action == "retry")
            {
                bd_email.spawn_email_sending_thread();
            }

            OnGet();
        }
    }
}
