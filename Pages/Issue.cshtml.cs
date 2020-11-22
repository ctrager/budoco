using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace budoco.Pages
{
    public class IssueModel : PageModel
    {

        // bindings start 
        [BindProperty]
        public int id { get; set; }

        [BindProperty]
        public string description { get; set; }

        [BindProperty]
        public string details { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> assigned_to_users { get; set; }
        [BindProperty]
        public int assigned_to_user_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> categories { get; set; }
        [BindProperty]
        public int category_id { get; set; }


        [BindProperty]
        public IEnumerable<SelectListItem> priorities { get; set; }
        [BindProperty]
        public int priority_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> projects { get; set; }
        [BindProperty]
        public int project_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> statuses { get; set; }
        [BindProperty]
        public int status_id { get; set; }

        // bindings end        

        public void OnGet(int id)
        {
            bd_util.redirect_if_not_logged_in(HttpContext);

            PrepareDropdowns();

            this.id = id;

            if (id != 0)
            {
                string sql = "select * from issues where i_id = " + id.ToString();

                DataRow dr = bd_db.get_datarow(sql);

                description = (string)dr["i_description"];
                details = (string)dr["i_details"];
                category_id = (int)dr["i_category"];
                project_id = (int)dr["i_project"];
                priority_id = (int)dr["i_priority"];
                status_id = (int)dr["i_status"];
                assigned_to_user_id = (int)dr["i_assigned_to_user"];

            }
        }

        public void OnPost()
        {
            PrepareDropdowns();

            if (!IsValid())
            {
                return;
            }

            string sql;

            if (id == 0)
            {

                sql = @"insert into issues 
                (i_description, i_details, i_created_by_user, i_category, i_project, i_priority, i_status, i_assigned_to_user) 
                values(@i_description, @i_details, @i_created_by_user, @i_category, @i_project, @i_priority, @i_status, @i_assigned_to_user) 
                returning i_id";

                this.id = (int)bd_db.exec_scalar(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Create was successful");
                Response.Redirect("Issue?id=" + this.id.ToString());
            }
            else
            {
                sql = @"update issues set 
                i_description = @i_description, 
                i_details = @i_details,
                i_last_updated_user = @i_last_updated_user,
                i_category = @i_category,
                i_project = @i_project,
                i_priority = @i_priority,
                i_status = @i_status,
                i_assigned_to_user = @i_assigned_to_user,
                i_last_updated_date = CURRENT_TIMESTAMP
                where i_id = @i_id;";

                bd_db.exec(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, "Update was successful");
            }
        }

        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            dict["@i_id"] = id;
            dict["@i_description"] = description;
            dict["@i_details"] = details;
            dict["@i_created_by_user"] = HttpContext.Session.GetInt32("us_id");
            dict["@i_last_updated_user"] = HttpContext.Session.GetInt32("us_id");
            dict["@i_category"] = category_id;
            dict["@i_assigned_to_user"] = assigned_to_user_id;
            dict["@i_project"] = project_id;
            dict["@i_priority"] = priority_id;
            dict["@i_status"] = status_id;

            return dict;
        }

        void PrepareDropdowns()
        {
            assigned_to_users = bd_db.prepare_select_list("select us_id, us_username from users");
            categories = bd_db.prepare_select_list("select ca_id, ca_name from categories");
            projects = bd_db.prepare_select_list("select pj_id, pj_name from projects");
            priorities = bd_db.prepare_select_list("select pr_id, pr_name from priorities");
            statuses = bd_db.prepare_select_list("select st_id, st_name from statuses");
        }

        bool IsValid()
        {
            var errs = new List<string>();

            if (string.IsNullOrWhiteSpace(description))
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
