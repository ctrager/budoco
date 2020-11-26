using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using RazorPartialToString.Services;
using System.Threading.Tasks;
using System.IO;

namespace budoco.Pages
{
    public class IssueModel : PageModel
    {

        private readonly IRazorPartialToStringRenderer _renderer;

        public IssueModel(IRazorPartialToStringRenderer renderer)
        {
            _renderer = renderer;
        }

        // bindings start 
        [FromQuery]
        public int id { get; set; }

        [BindProperty]
        public string force_create { get; set; }

        [BindProperty]
        public string description { get; set; }

        [BindProperty]
        public string details { get; set; }


        // dropdown 
        [BindProperty]
        public IEnumerable<SelectListItem> categories { get; set; }
        [BindProperty]
        public int category_id { get; set; }
        [BindProperty]
        public int category_selected_id { get; set; }
        [BindProperty]
        public string category_text { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> assigned_to_users { get; set; }
        [BindProperty]
        public int assigned_to_user_id { get; set; }
        [BindProperty]
        public int assigned_to_user_selected_id { get; set; }
        [BindProperty]
        public string assigned_to_user_text { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> projects { get; set; }
        [BindProperty]
        public int project_id { get; set; }
        [BindProperty]
        public int project_selected_id { get; set; }
        [BindProperty]
        public string project_text { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> priorities { get; set; }
        [BindProperty]
        public int priority_id { get; set; }
        [BindProperty]
        public int priority_selected_id { get; set; }
        [BindProperty]
        public string priority_text { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> organizations { get; set; }
        [BindProperty]
        public int organization_id { get; set; }
        [BindProperty]
        public int organization_selected_id { get; set; }
        [BindProperty]
        public string organization_text { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> statuses { get; set; }
        [BindProperty]
        public int status_id { get; set; }
        [BindProperty]
        public int status_selected_id { get; set; }
        [BindProperty]
        public string status_text { get; set; }

        [BindProperty]
        public IFormFile uploaded_file { get; set; }

        [BindProperty]
        public string post_text { get; set; }


        // bindings end        

        //https://stackoverflow.com/questions/56172036/razor-view-disabled-html-attribute-based-on-viewmodel-property
        public string null_or_disabled = null;
        public DataTable dt_posts;
        public string created_by_username;
        public string created_date;
        public string last_updated_username;
        public string last_updated_date;
        public int prev_issue_id_in_list;
        public int next_issue_id_in_list;

        public void OnGet()
        {
            if (!bd_util.check_user_permissions(HttpContext))
                return;

            GetIssue();

        }

        public void OnPost()
        {
            if (!bd_util.check_user_permissions(HttpContext))
                return;

            OnIssueFormPost();
            GetIssue();
        }

        void GetIssue()
        {
            PrepareDropdowns();

            if (id != 0)
            {
                string sql = @"select issues.*, 
                coalesce(created_by.us_username, '') as ""created_by_username"",
                coalesce(last_updated.us_username, '') as ""last_updated_username"",
                coalesce(assigned_to.us_username, '') as ""assigned_to_username"",
                coalesce(ca_name, '') as ""category_name"",
                coalesce(pj_name, '') as ""project_name"",
                coalesce(og_name, '') as ""organization_name"",
                coalesce(pr_name, '') as ""priority_name"",
                coalesce(st_name, '') as ""status_name"",
                created_by.us_username as ""created_by_username""
  
              from issues 
                inner join users created_by on created_by.us_id = i_created_by_user
                left outer join users last_updated on last_updated.us_id = i_last_updated_user
                left outer join users assigned_to on assigned_to.us_id = i_assigned_to_user
                left outer join categories on ca_id = i_category
                left outer join projects on pj_id = i_project
                left outer join organizations on og_id = i_organization
                left outer join priorities on pr_id = i_priority
                left outer join statuses on st_id = i_status
                
                where i_id = " + id.ToString();

                DataRow dr = bd_db.get_datarow(sql);

                if (dr == null)
                {
                    bd_util.set_flash_err(HttpContext, "Issue " + id.ToString() + " not found.");
                    id = 0;
                    Response.Redirect("/Issues");
                    return;
                }

                created_by_username = (string)dr["created_by_username"];
                created_date = (string)dr["i_created_date"].ToString();

                last_updated_username = (string)dr["last_updated_username"];
                last_updated_date = dr["i_last_updated_date"].ToString();

                description = (string)dr["i_description"];
                details = (string)dr["i_details"];

                category_id = (int)dr["i_category"];
                category_selected_id = category_id;
                category_text = (string)dr["category_name"];

                project_id = (int)dr["i_project"];
                project_selected_id = project_id;
                project_text = (string)dr["project_name"];


                organization_id = (int)dr["i_organization"];
                organization_selected_id = organization_id;
                organization_text = (string)dr["organization_name"];

                priority_id = (int)dr["i_priority"];
                priority_selected_id = priority_id;
                priority_text = (string)dr["project_name"];

                status_id = (int)dr["i_status"];
                status_selected_id = status_id;
                status_text = (string)dr["status_name"];

                assigned_to_user_id = (int)dr["i_assigned_to_user"];
                assigned_to_user_selected_id = assigned_to_user_id;
                assigned_to_user_text = (string)dr["assigned_to_username"];


                if (HttpContext.Session.GetInt32("us_is_report_only") == 1)
                {
                    null_or_disabled = "disabled";
                }
            }
            else
            {

                //Defaults values

                DataRow dr = bd_db.get_datarow("select * from statuses where st_is_default is true order by st_name limit 1");
                if (dr is not null)
                {
                    status_id = status_selected_id = (int)dr[0];
                    status_text = (string)dr[1];
                }
                dr = bd_db.get_datarow("select * from projects where pj_is_default is true order by pj_name limit 1");
                if (dr is not null)
                {
                    project_id = project_selected_id = (int)dr[0];
                    project_text = (string)dr[1];
                }
                dr = bd_db.get_datarow("select * from organizations where og_is_default is true order by og_name limit 1");
                if (dr is not null)
                {
                    organization_id = organization_selected_id = (int)dr[0];
                    organization_text = (string)dr[1];
                }
                dr = bd_db.get_datarow("select * from priorities where pr_is_default is true order by pr_name limit 1");
                if (dr is not null)
                {
                    priority_id = priority_selected_id = (int)dr[0];
                    priority_text = (string)dr[1];
                }
                dr = bd_db.get_datarow("select * from categories where ca_is_default is true order by ca_name limit 1");
                if (dr is not null)
                {
                    category_id = category_selected_id = (int)dr[0];
                    category_text = (string)dr[1];
                }

            }
            Console.WriteLine("qq list");
            // for prev, next issue
            List<int> issue_list = bd_session.Get("issue_list");
            if (issue_list is not null)
            {
                int current_position_in_list = issue_list.IndexOf(id);
                if (current_position_in_list != -1)
                {
                    // next
                    if (current_position_in_list < (issue_list.Count - 1))
                    {
                        next_issue_id_in_list = issue_list[current_position_in_list + 1];
                    }

                    // prev
                    if (current_position_in_list > 0)
                    {
                        prev_issue_id_in_list = issue_list[current_position_in_list - 1];
                    }
                }
            }
        }

        void OnIssueFormPost()
        {
            if (!IsValid())
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(details))
                details = "";

            string sql;

            if (id == 0 || force_create == "force_create")
            {

                sql = @"insert into issues 
                (i_description, i_details, i_created_by_user, 
                i_category, i_project, i_organization, i_priority, i_status, i_assigned_to_user) 
                values(@i_description, @i_details, @i_created_by_user, 
                @i_category, @i_project, @i_organization, @i_priority, @i_status, @i_assigned_to_user) 
                returning i_id";

                this.id = (int)bd_db.exec_scalar(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, bd_util.CREATE_WAS_SUCCESSFUL);
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
                i_organization = @i_organization,
                i_priority = @i_priority,
                i_status = @i_status,
                i_assigned_to_user = @i_assigned_to_user,
                i_last_updated_date = CURRENT_TIMESTAMP
                where i_id = @i_id;";

                bd_db.exec(sql, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);
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
            dict["@i_organization"] = organization_id;
            dict["@i_priority"] = priority_id;
            dict["@i_status"] = status_id;

            return dict;
        }

        void PrepareDropdowns()
        {
            assigned_to_users = bd_db.prepare_select_list("select us_id, us_username from users where us_is_active = true order by us_username");
            categories = bd_db.prepare_select_list("select ca_id, ca_name from categories where ca_is_active = true order by ca_name");
            projects = bd_db.prepare_select_list("select pj_id, pj_name from projects where pj_is_active = true order by pj_name");
            organizations = bd_db.prepare_select_list("select og_id, og_name from organizations where og_is_active = true order by og_name");
            priorities = bd_db.prepare_select_list("select pr_id, pr_name from priorities where pr_is_active = true order by pr_name");
            statuses = bd_db.prepare_select_list("select st_id, st_name from statuses where st_is_active = true order by st_name");
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
                bd_util.set_flash_errs(HttpContext, errs);
                return false;
            }
        }


        public Task<ContentResult> OnPostAddPostAsync()
        {
            var sql = @"insert into posts
                (p_issue, p_text, p_created_by_user,
                p_file_name, p_file_length, p_file_content_type)
                values(@p_issue, @p_text, @p_created_by_user,
                @p_file_name, @p_file_length, @p_file_content_type)
                returning p_id";

            var dict = new Dictionary<string, dynamic>();

            dict["@p_issue"] = id;
            dict["@p_text"] = post_text;
            dict["@p_created_by_user"] = HttpContext.Session.GetInt32("us_id");

            if (uploaded_file is not null)
            {
                dict["@p_file_name"] = uploaded_file.FileName;
                dict["@p_file_length"] = uploaded_file.Length;
                dict["@p_file_content_type"] = uploaded_file.ContentType;
            }
            else
            {
                dict["@p_file_name"] = "";
                dict["@p_file_length"] = 0;
                dict["@p_file_content_type"] = "";
            }

            int post_id = (int)bd_db.exec_scalar(sql, dict);

            // insert the blob
            if (uploaded_file is not null)
            {
                MemoryStream memory_stream = new MemoryStream();
                uploaded_file.CopyTo(memory_stream);
                dict["@pa_post"] = post_id;
                dict["@pa_content"] = memory_stream.ToArray();
                sql = @"insert into post_attachments (pa_post, pa_content) 
                    values(@pa_post, @pa_content)";
                bd_db.exec(sql, dict);
            }

            return OnGetPostsAsync();

        }
        public async Task<ContentResult> OnGetPostsAsync()
        {

            var sql = @"select posts.*, us_username
                    from posts 
                    inner join users on us_id = p_created_by_user
                    where p_issue = " + id.ToString()
                    + " order by p_id asc";

            dt_posts = bd_db.get_datatable(sql);

            String html = await _renderer.RenderPartialToStringAsync("_IssuePostsPartial", this);
            return Content(html);
        }

    }
}
