using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace budoco.Pages
{
    public class IssueModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public int id { get; set; }

        [BindProperty]
        public string desc { get; set; }

        // bindings end        

        public void OnGet(int id)
        {

            bd_util.redirect_if_not_logged_in(HttpContext);

            this.id = id;

            if (id != 0)
            {
                string sql = "select * from issues where i_id = " + id.ToString();

                DataRow dr = db_util.get_datarow(sql);

                desc = (string)dr["i_desc"];

            }
        }

        public void OnPost()
        {
            if (!IsValid())
            {
                return;
            }

            string sql;

            if (id == 0)
            {

                sql = @"insert into issues 
                (i_desc, i_created_by_user) 
                values(@i_desc, @i_created_by_user) 
                returning i_id";

                this.id = (int)db_util.exec_scalar(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Create was successful");
                Response.Redirect("Issue?id=" + this.id.ToString());
            }
            else
            {
                sql = @"update issues set 
                i_desc = @i_desc, 
                i_last_updated_user = @i_last_updated_user
                where i_id = @i_id;";

                db_util.exec(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Update was successful");
            }
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@i_id"] = id;
            dict["@i_desc"] = desc;
            dict["@i_created_by_user"] = HttpContext.Session.GetInt32("us_id");
            dict["@i_last_updated_user"] = HttpContext.Session.GetInt32("us_id");
            return dict;
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(desc))
            {
                errs.Add("Description is required.");
            }

            if (errs.Count == 0)
            {
                return true;
            }
            else
            {
                bd_util.set_flash_err(HttpContext, errs);
                return false;
            }
        }
    }
}
