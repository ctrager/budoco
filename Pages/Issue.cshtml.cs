using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
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
        public IEnumerable<SelectListItem> assigned_to_user_list { get; set; }
        [BindProperty]
        public int assigned_to_user_id { get; set; }

        // dropdown
        [BindProperty]
        public IEnumerable<SelectListItem> organization_list { get; set; }
        [BindProperty]
        public int organization_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_1_list { get; set; }
        [BindProperty]
        public int custom_1_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_2_list { get; set; }
        [BindProperty]
        public int custom_2_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_3_list { get; set; }
        [BindProperty]
        public int custom_3_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_4_list { get; set; }
        [BindProperty]
        public int custom_4_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_5_list { get; set; }
        [BindProperty]
        public int custom_5_id { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> custom_6_list { get; set; }
        [BindProperty]
        public int custom_6_id { get; set; }

        [BindProperty]
        public IFormFile uploaded_file1 { get; set; }
        [BindProperty]
        public IFormFile uploaded_file2 { get; set; }
        [BindProperty]
        public IFormFile uploaded_file3 { get; set; }

        [BindProperty]
        public string post_type { get; set; }
        [BindProperty]
        public string post_email_to { get; set; }
        [BindProperty]
        public string post_text { get; set; }

        // bindings end        

        public string custom_1_label = bd_config.get(bd_config.CustomFieldLabelSingular1);
        public string custom_2_label = bd_config.get(bd_config.CustomFieldLabelSingular2);
        public string custom_3_label = bd_config.get(bd_config.CustomFieldLabelSingular3);
        public string custom_4_label = bd_config.get(bd_config.CustomFieldLabelSingular4);
        public string custom_5_label = bd_config.get(bd_config.CustomFieldLabelSingular5);
        public string custom_6_label = bd_config.get(bd_config.CustomFieldLabelSingular6);

        public string dropdown_partial_prefix;
        public string dropdown_partial_label;

        //https://stackoverflow.com/questions/56172036/razor-view-disabled-html-attribute-based-on-viewmodel-property
        public string null_or_disabled = null;
        public DataTable dt_posts;
        public DataTable dt_post_email_to;
        public string created_by_username;
        public string created_date;
        public string last_updated_username;
        public string last_updated_date;
        public int prev_issue_id_in_list;
        public int next_issue_id_in_list;
        public string post_error = "";

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
                coalesce(assigned_to.us_is_active, true) as ""assigned_to_is_active"",
                
                coalesce(og_is_active, true) as ""og_is_active"",
                coalesce(c1_is_active, true) as ""c1_is_active"",
                coalesce(c2_is_active, true) as ""c2_is_active"",
                coalesce(c3_is_active, true) as ""c3_is_active"",
                coalesce(c4_is_active, true) as ""c4_is_active"",
                coalesce(c5_is_active, true) as ""c5_is_active"",
                coalesce(c6_is_active, true) as ""c6_is_active"",
         
                created_by.us_username as ""created_by_username""
  
              from issues 
                inner join users created_by on created_by.us_id = i_created_by_user
                left outer join users last_updated on last_updated.us_id = i_last_updated_user
                left outer join users assigned_to on assigned_to.us_id = i_assigned_to_user
                 left outer join organizations on og_id = i_organization
                left outer join custom_1 on c1_id = i_custom_1
                left outer join custom_2 on c2_id = i_custom_2
                left outer join custom_3 on c3_id = i_custom_3
                left outer join custom_4 on c4_id = i_custom_4
                left outer join custom_5 on c5_id = i_custom_5
                left outer join custom_6 on c6_id = i_custom_6
                
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
                organization_id = (int)dr["i_organization"];
                custom_1_id = (int)dr["i_custom_1"];
                custom_2_id = (int)dr["i_custom_2"];
                custom_3_id = (int)dr["i_custom_3"];
                custom_4_id = (int)dr["i_custom_4"];
                custom_5_id = (int)dr["i_custom_5"];
                custom_6_id = (int)dr["i_custom_6"];

                assigned_to_user_id = (int)dr["i_assigned_to_user"];

                if (HttpContext.Session.GetInt32("us_is_report_only") == 1)
                {
                    null_or_disabled = "disabled";
                }

                // TODO:
                // if this issue uses an option where is_active == false,
                // we still want to show it

                // only user without email is "system"
                dt_post_email_to = bd_db.get_datatable(
                    @"select p_email_to from posts where p_email_to != '' and p_issue = " + id.ToString()
                    + " union select p_email_from from posts where p_email_from != '' and p_issue = " + id.ToString()
                    + " union select us_email_address from users where us_username != 'system' and us_is_active = true order by 1");

            }
            else
            {
                SelectDefaultDropdownOptions();
            }

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


        void SelectDefaultDropdownOptions()
        {
            //Defaults values for dropdowns

            DataRow dr;

            dr = bd_db.get_datarow("select * from organizations where og_is_default is true order by og_name limit 1");
            if (dr is not null)
            {
                organization_id = (int)dr[0];
            }

            if (bd_config.get(bd_config.CustomFieldEnabled1) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_1 where c1_is_default is true order by c1_name limit 1");
                if (dr is not null)
                {
                    custom_1_id = (int)dr[0];
                }
            }

            if (bd_config.get(bd_config.CustomFieldEnabled2) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_2 where c2_is_default is true order by c2_name limit 1");
                if (dr is not null)
                {
                    custom_2_id = (int)dr[0];
                }
            }

            if (bd_config.get(bd_config.CustomFieldEnabled3) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_3 where c3_is_default is true order by c3_name limit 1");
                if (dr is not null)
                {
                    custom_3_id = (int)dr[0];
                }
            }
            if (bd_config.get(bd_config.CustomFieldEnabled4) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_4 where c4_is_default is true order by c4_name limit 1");
                if (dr is not null)
                {
                    custom_4_id = (int)dr[0];
                }
            }

            if (bd_config.get(bd_config.CustomFieldEnabled5) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_5 where c5_is_default is true order by c5_name limit 1");
                if (dr is not null)
                {
                    custom_5_id = (int)dr[0];
                }
            }
            if (bd_config.get(bd_config.CustomFieldEnabled6) == 1)
            {
                dr = bd_db.get_datarow("select * from custom_6 where c6_is_default is true order by c6_name limit 1");
                if (dr is not null)
                {
                    custom_6_id = (int)dr[0];
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
                i_organization,
                i_custom_1, i_custom_2,i_custom_3,i_custom_4,i_custom_5,i_custom_6,
                i_assigned_to_user) 
                values(@i_description, @i_details, @i_created_by_user, 
                @i_organization, 
                @i_custom_1, @i_custom_2, @i_custom_3, @i_custom_4, @i_custom_5, @i_custom_6,
                @i_assigned_to_user) 
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
             
                i_custom_1 = @i_custom_1,
                i_custom_2 = @i_custom_2,
                i_custom_3 = @i_custom_3,
                i_custom_4 = @i_custom_4,
                i_custom_5 = @i_custom_5,
                i_custom_6 = @i_custom_6,
                
                i_organization = @i_organization,
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
            dict["@i_custom_1"] = custom_1_id;
            dict["@i_custom_2"] = custom_2_id;
            dict["@i_custom_3"] = custom_3_id;
            dict["@i_custom_4"] = custom_4_id;
            dict["@i_custom_5"] = custom_5_id;
            dict["@i_custom_6"] = custom_6_id;

            dict["@i_assigned_to_user"] = assigned_to_user_id;
            dict["@i_organization"] = organization_id;

            return dict;
        }

        void PrepareDropdowns()
        {
            assigned_to_user_list = bd_db.prepare_select_list("select us_id, us_username from users where us_is_active = true order by us_username");
            organization_list = bd_db.prepare_select_list("select og_id, og_name from organizations where og_is_active = true order by og_name");

            if (bd_config.get(bd_config.CustomFieldEnabled1) == 1)
            {
                custom_1_list = bd_db.prepare_select_list("select c1_id, c1_name from custom_1 where c1_is_active = true order by c1_name");
            }

            if (bd_config.get(bd_config.CustomFieldEnabled2) == 1)
            {
                custom_2_list = bd_db.prepare_select_list("select c2_id, c2_name from custom_2 where c2_is_active = true order by c2_name");
            }

            if (bd_config.get(bd_config.CustomFieldEnabled3) == 1)
            {
                custom_3_list = bd_db.prepare_select_list("select c3_id, c3_name from custom_3 where c3_is_active = true order by c3_name");
            }

            if (bd_config.get(bd_config.CustomFieldEnabled4) == 1)
            {
                custom_4_list = bd_db.prepare_select_list("select c4_id, c4_name from custom_4 where c4_is_active = true order by c4_name");
            }

            if (bd_config.get(bd_config.CustomFieldEnabled5) == 1)
            {
                custom_5_list = bd_db.prepare_select_list("select c5_id, c5_name from custom_5 where c5_is_active = true order by c5_name");
            }

            if (bd_config.get(bd_config.CustomFieldEnabled6) == 1)
            {
                custom_6_list = bd_db.prepare_select_list("select c6_id, c6_name from custom_6 where c6_is_active = true order by c6_name");
            }

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
            bd_util.check_user_permissions(HttpContext);

            if (post_email_to is not null)
            {
                // trim leading/trailing spaces
                string[] addresses = post_email_to.Split(",");
                for (int i = 0; i < addresses.Length; i++)
                {
                    string address = addresses[i].Trim();
                }
                post_email_to = string.Join(",", addresses);
            }

            if (!IsValidPost())
            {
                return OnGetPostsAsync();
            }

            var sql = @"insert into posts
                (p_issue, p_post_type, p_text, p_created_by_user, p_email_to)
                values(@p_issue, @p_post_type, @p_text, @p_created_by_user, @p_email_to)
                returning p_id";

            var dict = new Dictionary<string, dynamic>();

            if (string.IsNullOrWhiteSpace(post_text))
            {
                post_text = "";
            }
            if (string.IsNullOrWhiteSpace(post_email_to))
            {
                post_email_to = "";
            }
            dict["@p_issue"] = id;
            dict["@p_post_type"] = post_type;
            dict["@p_email_to"] = post_email_to;
            dict["@p_text"] = post_text;
            dict["@p_created_by_user"] = HttpContext.Session.GetInt32("us_id");

            int post_id = (int)bd_db.exec_scalar(sql, dict);

            if (uploaded_file1 is not null)
                InsertPostAttachment(post_id, id, uploaded_file1);
            if (uploaded_file2 is not null)
                InsertPostAttachment(post_id, id, uploaded_file2);
            if (uploaded_file3 is not null)
                InsertPostAttachment(post_id, id, uploaded_file3);

            // update timestamp on parent issue table
            sql = @"update issues set 
            i_last_post_user = @i_last_post_user,
            i_last_post_date = CURRENT_TIMESTAMP
            where i_id = " + id.ToString();
            dict["@i_last_post_user"] = HttpContext.Session.GetInt32("us_id");

            bd_db.exec(sql, dict);

            if (post_type == "email")
            {
                //SendIssueEmail();
                QueueEmail(post_id);
            }

            return OnGetPostsAsync();
        }

        bool IsValidPost()
        {
            if (post_type == "email")
            {
                if (string.IsNullOrWhiteSpace(post_email_to))
                {
                    // post_error instead of set_flash_err because user is at the bottom
                    // of the page, can't see the error at the top
                    post_error = @"Email ""To"" is required.";
                    return false;
                }

                string[] addresses = post_email_to.Split(",");
                for (int i = 0; i < addresses.Length; i++)
                {
                    if (!bd_email.validate_email_address(addresses[i]))
                    {
                        post_error = "Email address is invalid: " + addresses[i];
                        return false;
                    }
                }
            }
            return true;
        }

        void QueueEmail(int post_id)
        {

            // Build a magical email subject line
            // get fields from issue
            var sql = @"select 
                cast(i_id as text) as ""id"",
                to_char(i_created_date, 'US') as ""microseconds"", 
                i_description from issues where i_id = " + id.ToString();
            DataRow dr_issue = bd_db.get_datarow(sql);

            string identifier = "[#"
                + (string)dr_issue["id"]
                + "-" + (string)dr_issue["microseconds"]
                + "] ";

            // We use the id and very precise create date as like a GUID to tie the
            // incoming emails back to this issue
            // [#123-76767] My computer won't turn on
            string subject = identifier + (string)dr_issue["i_description"];

            bd_email.queue_email("post",
                post_email_to,
                subject,
                post_text,
                post_id);

        }

        void InsertPostAttachment(int post_id, int issue_id, IFormFile uploaded_file)
        {

            var sql = @"insert into post_attachments
                (pa_post, pa_issue, pa_file_name, pa_file_length, pa_file_content_type, pa_content)
                values(@pa_post, @pa_issue, @pa_file_name, @pa_file_length, @pa_file_content_type, @pa_content)";

            var dict = new Dictionary<string, dynamic>();

            dict["@pa_post"] = post_id;
            dict["@pa_issue"] = issue_id;
            dict["@pa_file_name"] = uploaded_file.FileName;
            dict["@pa_file_length"] = uploaded_file.Length;
            dict["@pa_file_content_type"] = uploaded_file.ContentType;

            MemoryStream memory_stream = new MemoryStream();
            uploaded_file.CopyTo(memory_stream);

            dict["@pa_content"] = memory_stream.ToArray();

            bd_db.exec(sql, dict);
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

            if (post_error != "")
            {
                return Content("<!--error-->" + html);
            }
            else
            {
                return Content(html);
            }
        }

        public DataTable GetPostAttachments(int p_id)
        {
            var sql = @"select pa_id, pa_file_name, pa_file_length, pa_file_content_type
                    from post_attachments
                    where pa_post = " + p_id.ToString()
                    + " order by pa_id asc";

            return bd_db.get_datatable(sql);

        }

    }
}
