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
using System.Security.Policy;

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
        public string disable_dropdowns_when_not_null = null;
        public DataTable dt_posts;
        public DataTable dt_post_email_to;
        public string created_by_username;
        public string created_date;
        public string last_updated_username;
        public string last_updated_date;
        public int prev_issue_id_in_list;
        public int next_issue_id_in_list;
        public string post_error = "";
        public int user_org;

        DateTime time1;
        DateTime time2;

        public void OnGet()
        {
            time1 = DateTime.Now;

            GetIssue();

            time2 = DateTime.Now;
            TimeSpan timespan = time2 - time1;
            bd_util.log("Issue OnGet milliseconds:" + timespan.TotalMilliseconds.ToString());

        }

        public void OnPost()
        {

            OnIssueFormPost();
            GetIssue();
        }

        void GetIssue()
        {
            // There are 8 use cases
            // 1 regular user create
            // 2 regular user get existing
            // 3 report only user create
            // 4 report only user get existing
            // 5 org user create
            // 6 org user get existing
            // 7 org user report only create
            // 8 org user report only existing


            // get existing to update
            if (id != 0)
            {
                string sql = @"select issues.*, 
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

                // not found
                if (dr == null)
                {
                    bd_util.set_flash_err(HttpContext, "Issue " + id.ToString() + " not found.");
                    id = 0;
                    Response.Redirect("/App/Issues");
                    return;
                }

                organization_id = (int)dr["i_organization"];

                // no permission
                if (!GetAndCheckUserOrg(organization_id))
                {
                    return;
                }

                created_by_username = (string)dr["created_by_username"];
                created_date = (string)dr["i_created_date"].ToString();

                last_updated_username = (string)dr["last_updated_username"];
                last_updated_date = dr["i_last_updated_date"].ToString();
                description = (string)dr["i_description"];
                details = (string)dr["i_details"];

                custom_1_id = (int)dr["i_custom_1"];
                custom_2_id = (int)dr["i_custom_2"];
                custom_3_id = (int)dr["i_custom_3"];
                custom_4_id = (int)dr["i_custom_4"];
                custom_5_id = (int)dr["i_custom_5"];
                custom_6_id = (int)dr["i_custom_6"];

                assigned_to_user_id = (int)dr["i_assigned_to_user"];

                // This has to be here in the sequence because it uses the values 
                // we just fetched from the db
                PrepareDropdowns();

                // don't let reporter change anything
                if (bd_util.is_user_report_only(HttpContext))
                {
                    disable_dropdowns_when_not_null = "disabled";
                }

                // for the dropdown of email addresses in the post form
                // only user without email is "system"
                dt_post_email_to = bd_db.get_datatable(
                    @"select p_email_to from posts where p_email_to != '' and p_issue = " + id.ToString()
                    + " union select p_email_from from posts where p_email_from != '' and p_issue = " + id.ToString()
                    + " union select us_email_address from users where us_username != 'system' and us_is_active = true order by 1");

            }
            else
            {
                user_org = bd_util.get_user_organization_from_session(HttpContext);

                PrepareDropdowns();

                // prep for create 
                SelectDefaultDropdownOptions();
            }

            // for prev, next issue
            List<int> issue_list = bd_session.Get(HttpContext, "issue_list");
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
            if (user_org != 0)
            {
                // We ony put one option in the select, so it's selected
            }
            else
            {
                // get the default org
                object obj = bd_db.exec_scalar("select og_id from organizations where og_is_default is true order by og_name limit 1");
                if (obj is not null)
                {
                    organization_id = (int)obj;
                }
            }

            custom_1_id = bd_issue.get_default_for_custom_field("1");
            custom_2_id = bd_issue.get_default_for_custom_field("2");
            custom_3_id = bd_issue.get_default_for_custom_field("3");
            custom_4_id = bd_issue.get_default_for_custom_field("4");
            custom_5_id = bd_issue.get_default_for_custom_field("5");
            custom_6_id = bd_issue.get_default_for_custom_field("6");

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

                // create a new issue
                this.id = (int)bd_db.exec_scalar(bd_issue.INSERT_ISSUE_SQL, GetValuesDict());
                bd_util.set_flash_msg(HttpContext, bd_util.CREATE_WAS_SUCCESSFUL);
                Response.Redirect("/App/Issue?id=" + this.id.ToString());
            }
            else
            {
                // update an existing issue

                if (user_org != 0)
                {
                    if (organization_id != user_org)
                    {
                        // This should never happen, but really, but let's really stop it, even if ugly
                        throw new Exception("User asigned to org tried to change the org of an issue");
                    }
                }


                DataRow dr_before = bd_db.get_datarow(
                    "select * from issues where i_id = " + id.ToString());

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
                where i_id = @i_id
                returning i_last_updated_date";

                DateTime last_updated_date = (DateTime)bd_db.exec_scalar(sql, GetValuesDict());

                WriteHistoryPosts(dr_before, last_updated_date);

                bd_util.set_flash_msg(HttpContext, bd_util.UPDATE_WAS_SUCCESSFUL);
            }
        }

        void WriteHistoryPosts(DataRow dr_before, DateTime last_updated_date)
        {

            string sql_for_insert = @"insert into posts 
                (p_issue, p_post_type, p_changed_field, p_before_val, p_after_val, p_created_by_user, p_created_date)
                values(@p_issue, @p_post_type, @p_changed_field, @p_before_val, @p_after_val, @p_created_by_user, @p_created_date)";

            var dict = new Dictionary<string, dynamic>();
            dict["@p_issue"] = id;
            dict["@p_post_type"] = bd_issue.POST_TYPE_HISTORY;
            dict["@p_created_by_user"] = bd_util.get_user_id_from_session(HttpContext);
            dict["@p_created_date"] = last_updated_date; // same date as issue, sync the history

            if ((string)dr_before["i_description"] != description)
            {
                dict["@p_changed_field"] = "Description";
                dict["@p_before_val"] = (string)dr_before["i_description"];
                dict["@p_after_val"] = description;
                bd_db.exec(sql_for_insert, dict);
            }

            string before_details = (string)dr_before["i_details"];
            if (before_details != details)
            {
                dict["@p_changed_field"] = "Details";
                dict["@p_before_val"] = string.IsNullOrEmpty(before_details) ? "[None]" : before_details;
                dict["@p_after_val"] = string.IsNullOrEmpty(details) ? "[None]" : details;
                bd_db.exec(sql_for_insert, dict);
            }

            // Write posts for all the dropdowns
            string sql_for_get_text = null;
            string changed_field_name = null;

            // org
            changed_field_name = "Organization";
            sql_for_get_text = "select og_name from organizations where og_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_organization"], organization_id, sql_for_get_text, sql_for_insert, dict);

            // assigned
            changed_field_name = "Assigned";
            sql_for_get_text = "select us_username from users where us_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_assigned_to_user"], assigned_to_user_id, sql_for_get_text, sql_for_insert, dict);

            // 1
            changed_field_name = bd_config.get("CustomFieldLabelSingular1");
            sql_for_get_text = "select c1_name from custom_1 where c1_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_1"], custom_1_id, sql_for_get_text, sql_for_insert, dict);
            // 2
            changed_field_name = bd_config.get("CustomFieldLabelSingular2");
            sql_for_get_text = "select c2_name from custom_2 where c2_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_2"], custom_2_id, sql_for_get_text, sql_for_insert, dict);
            // 3
            changed_field_name = bd_config.get("CustomFieldLabelSingular3");
            sql_for_get_text = "select c3_name from custom_3 where c3_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_3"], custom_3_id, sql_for_get_text, sql_for_insert, dict);
            // 4
            changed_field_name = bd_config.get("CustomFieldLabelSingular4");
            sql_for_get_text = "select c4_name from custom_4 where c4_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_4"], custom_4_id, sql_for_get_text, sql_for_insert, dict);
            // 5
            changed_field_name = bd_config.get("CustomFieldLabelSingular5");
            sql_for_get_text = "select c5_name from custom_5 where c5_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_5"], custom_5_id, sql_for_get_text, sql_for_insert, dict);
            // 6
            changed_field_name = bd_config.get("CustomFieldLabelSingular6");
            sql_for_get_text = "select c6_name from custom_6 where c6_id = ";
            WriteHistoryPost(changed_field_name, (int)dr_before["i_custom_6"], custom_6_id, sql_for_get_text, sql_for_insert, dict);
        }

        void WriteHistoryPost(string changed_field_name, int before_val, int after_val, string sql_for_get_text, string sql_for_insert, Dictionary<string, dynamic> dict)
        {
            if (after_val == before_val)
                return;

            if (before_val == 0)
            {
                dict["@p_before_val"] = "[None]";
            }
            else
            {
                // get the text for this id
                dict["@p_before_val"] = (string)bd_db.exec_scalar(sql_for_get_text + before_val.ToString());
            }

            if (after_val == 0)
            {
                dict["@p_after_val"] = "[None]";
            }
            else
            {
                // get the text for this id
                dict["@p_after_val"] = (string)bd_db.exec_scalar(sql_for_get_text + after_val.ToString());
            }

            // insert into posts
            dict["@p_changed_field"] = changed_field_name;
            bd_db.exec(sql_for_insert, dict);

        }


        Dictionary<string, dynamic> GetValuesDict()
        {
            var dict = new Dictionary<string, dynamic>();

            int us_id = bd_util.get_user_id_from_session(HttpContext);

            dict["@i_id"] = id;
            dict["@i_description"] = description;
            dict["@i_details"] = details;
            dict["@i_created_by_user"] = us_id;
            dict["@i_last_updated_user"] = us_id;
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

            string sql;

            // org dropdown - users in orgs can only see their own
            if (user_org != 0)
            {
                // user belongs to org
                // just one
                sql = "select og_id, og_name from organizations where og_id = " + user_org.ToString();
            }
            else
            {
                // normal user
                if (id == 0)
                {
                    // create, get only active
                    sql = @"select og_id, og_name from organizations where og_is_active = true"
                    + " union select 0, '[None]' order by og_name";

                }
                else
                {
                    // update, so also includes current val even if inactive
                    sql = @"select og_id, og_name from organizations where og_is_active = true
                      union select 0, '[None]'
                      union select og_id, og_name from organizations where og_id = " + organization_id.ToString()
                        + " order by og_name";
                }
            }
            organization_list = bd_db.prepare_select_list(sql);

            // assigned to user dropdown - users in orgs can only see their own and the users not in ogs
            if (user_org != 0)
            {
                // user belongs to org
                if (id == 0)
                {
                    // only from own org and unassigned to org
                    sql = @"select us_id, us_username from users 
                    where us_is_active = true 
                    and (us_organization = 0 or us_organization = " + user_org.ToString()
                    + ") order by us_username";
                }
                else
                {
                    // same as above, but also including current val
                    sql = @"select us_id, us_username from users 
                    where us_is_active = true 
                    and (us_organization = 0 or us_organization = " + user_org.ToString()
                    + ") union select us_id, us_username from users where us_id = " + assigned_to_user_id.ToString()
                    + " order by us_username";

                }
            }
            else
            {
                // normal user
                if (id == 0)
                {
                    // all active
                    sql = "select us_id, us_username from users where us_is_active = true order by us_username";

                }
                else
                {
                    // all active plus current val
                    sql = @"select us_id, us_username from users where us_is_active = true  
                      union select us_id, us_username from users where us_id = " + assigned_to_user_id.ToString()
                      + " order by us_username";


                }
            }
            assigned_to_user_list = bd_db.prepare_select_list(sql);

            // custom 1-6
            custom_1_list = PrepareCustomDropdown("1", custom_1_id);
            custom_2_list = PrepareCustomDropdown("2", custom_2_id);
            custom_3_list = PrepareCustomDropdown("3", custom_3_id);
            custom_4_list = PrepareCustomDropdown("4", custom_4_id);
            custom_5_list = PrepareCustomDropdown("5", custom_5_id);
            custom_6_list = PrepareCustomDropdown("6", custom_6_id);

        }

        SelectList PrepareCustomDropdown(string number, int current_val)
        {
            if (bd_config.get("CustomFieldEnabled" + number) == 0)
                return null;

            string sql;
            if (id == 0)
            {
                // all active
                sql = "select c$_id, c$_name from custom_$ where c$_is_active = true order by c$_name";
            }
            else
            {
                // all active and current val    
                sql = @"select c$_id, c$_name from custom_$ where c$_is_active = true 
                  union select c$_id, c$_name from custom_$ where c$_id = " + current_val.ToString()
                  + " order by c$_name";
            }
            sql = sql.Replace("$", number);

            return bd_db.prepare_select_list(sql);
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
            dict["@p_created_by_user"] = bd_util.get_user_id_from_session(HttpContext);

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
            dict["@i_last_post_user"] = bd_util.get_user_id_from_session(HttpContext);
            bd_db.exec(sql, dict);

            if (post_type == bd_issue.POST_TYPE_EMAIL_OUT)
            {
                //SendIssueEmail();
                QueueEmail(post_id);
            }

            return OnGetPostsAsync();
        }

        bool IsValidPost()
        {
            if (post_type == bd_issue.POST_TYPE_EMAIL_OUT)
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

            string issue_email_preamble = bd_config.get(bd_config.IssueEmailPreamble);
            issue_email_preamble = issue_email_preamble.Replace("$URL",
                bd_config.get(bd_config.WebsiteUrlRootWithoutSlash) + "/App/Issue?id=" + id.ToString());
            string email_body = issue_email_preamble + "\n\n" + post_text;

            bd_email.queue_email("post",
                post_email_to,
                subject,
                email_body,
                post_id);

        }

        void InsertPostAttachment(int post_id, int issue_id, IFormFile uploaded_file)
        {
            bd_issue.insert_post_attachment_from_uploaded_file(post_id, issue_id, uploaded_file);
        }

        public async Task<ContentResult> OnGetPostsAsync()
        {
            time1 = DateTime.Now;

            // client fetches posts using ajax

            //check permission
            organization_id = (int)bd_db.exec_scalar(
               "select i_organization from issues where i_id = " + id.ToString());

            // A user wouldn't normally get this far, to be able to post
            // unless his permission was changed from under him or he was hacking
            if (!GetAndCheckUserOrg(organization_id))
            {
                return Content("<!--error--><!--org permission-->");
            }

            // get posts
            var sql = @"select posts.*, us_username
                    from posts 
                    inner join users on us_id = p_created_by_user
                    where p_issue = " + id.ToString()
                    + " order by p_id asc";

            dt_posts = bd_db.get_datatable(sql);

            // get the html as a string and return it to ajax client
            String html = await _renderer.RenderPartialToStringAsync("_IssuePostsPartial", this);

            time2 = DateTime.Now;
            TimeSpan timespan = time2 - time1;
            bd_util.log("OnGetPostsAsync milliseconds:" + timespan.TotalMilliseconds.ToString());


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

        bool GetAndCheckUserOrg(int i_organization)
        {
            user_org = bd_util.get_user_organization_from_session(HttpContext);
            if (user_org != 0)
            {
                if (user_org != i_organization)
                {
                    bd_util.set_flash_err(HttpContext,
                        "You don't have permission to view this issue because it does not belong to your organization.");
                    Response.Redirect("/Stop");
                    return false;
                }
            }
            return true; ;
        }

    }
}
