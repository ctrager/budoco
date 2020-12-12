using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace budoco.Pages
{
    public class QueriesModel : PageModel
    {

        public DataTable dt;
        public string singular_table_name = "Query";

        public void OnGet()
        {
            string sql = @"select qu_id as ""ID"", qu_name as ""Name"", qu_description as ""Description"", qu_sql as ""SQL"", 
            qu_is_active as ""Active"", qu_is_default as ""Default"" from queries order by qu_name";
            dt = bd_db.get_datatable(sql);
        }

        public void OnPost(int delete_id)
        {
            string sql = "delete from queries where qu_id = " + delete_id.ToString();
            bd_db.exec(sql);
            bd_util.set_flash_msg(HttpContext, "Delete was successful");

            OnGet();
        }
    }
}
