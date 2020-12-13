using System;
using System.Collections.Generic;
using MimeKit;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace budoco
{
    public static class bd_issue
    {

        public const string POST_TYPE_COMMENT = "comment";
        public const string POST_TYPE_HISTORY = "history";
        public const string POST_TYPE_EMAIL_OUT = "email_out";

        // response to one of our outgoing emails
        public const string POST_TYPE_REPLY_IN = "reply_in";

        // an email that creates a new issue
        public const string POST_TYPE_EMAIL_IN = "email_in";

        public const string INSERT_ISSUE_SQL = @"insert into issues 
                (i_description, i_details, i_created_by_user, 
                i_organization,
                i_custom_1, i_custom_2,i_custom_3,i_custom_4,i_custom_5,i_custom_6,
                i_assigned_to_user) 
                values(@i_description, @i_details, @i_created_by_user, 
                @i_organization, 
                @i_custom_1, @i_custom_2, @i_custom_3, @i_custom_4, @i_custom_5, @i_custom_6,
                @i_assigned_to_user) 
                returning i_id";

        public static int create_issue(string description, string details, int user_id)
        {
            int i_organization = 0;

            if (user_id != bd_util.SYSTEM_USER_ID)
            {
                i_organization = (int)bd_db.exec_scalar("select us_organization from users where us_id = " + user_id.ToString());

            }
            else
            {
                object obj = bd_db.exec_scalar("select og_id from organizations where og_is_default is true order by og_name limit 1");
                if (obj is not null)
                {
                    i_organization = (int)obj;
                }
            }

            var dict = new Dictionary<string, dynamic>();
            int custom_1_id = bd_issue.get_default_for_custom_field("1");
            int custom_2_id = bd_issue.get_default_for_custom_field("2");
            int custom_3_id = bd_issue.get_default_for_custom_field("3");
            int custom_4_id = bd_issue.get_default_for_custom_field("4");
            int custom_5_id = bd_issue.get_default_for_custom_field("5");
            int custom_6_id = bd_issue.get_default_for_custom_field("6");

            // description is too long, but we don't want to loose it
            // so let's prepend it to details;
            if (description.Length > 200)
            {
                details = description + "\n\n" + details;
            }

            dict["@i_description"] = description;
            dict["@i_details"] = details;
            dict["@i_created_by_user"] = user_id;
            dict["@i_last_updated_user"] = user_id;
            dict["@i_custom_1"] = custom_1_id;
            dict["@i_custom_2"] = custom_2_id;
            dict["@i_custom_3"] = custom_3_id;
            dict["@i_custom_4"] = custom_4_id;
            dict["@i_custom_5"] = custom_5_id;
            dict["@i_custom_6"] = custom_6_id;

            dict["@i_assigned_to_user"] = 0; // assigned_to_user_id;
            dict["@i_organization"] = i_organization;


            int issue_id = (int)bd_db.exec_scalar(bd_issue.INSERT_ISSUE_SQL, dict);

            return issue_id;

        }

        // COREY todo make the following server methods  DRYer

        public static int create_comment_post_with_image(int issue_id, int user_id, string image_data)
        {
            var sql = @"insert into posts
                (p_issue, p_post_type, p_text, p_created_by_user)
                values(@p_issue, @p_post_type, @p_text, @p_created_by_user)
                returning p_id";

            var dict = new Dictionary<string, dynamic>();
            dict["@p_issue"] = issue_id;
            dict["@p_post_type"] = bd_issue.POST_TYPE_COMMENT;
            dict["@p_text"] = "";
            dict["@p_created_by_user"] = user_id;

            int post_id = (int)bd_db.exec_scalar(sql, dict);

            if (!string.IsNullOrEmpty(image_data))
            {
                insert_post_attachment_from_dataurl(post_id, issue_id, image_data);
            }

            return post_id;
        }

        public static int create_post_from_email(string post_type, int issue_id, string from, string text_body, string html_body)
        {
            var sql = @"insert into posts
                (p_issue, p_post_type, p_text, p_created_by_user, p_email_from, p_email_from_html_part)
                values(@p_issue, @p_post_type, @p_text, @p_created_by_user, @p_email_from, @p_email_from_html_part)
                returning p_id";

            var dict = new Dictionary<string, dynamic>();
            dict["@p_issue"] = issue_id;
            dict["@p_post_type"] = post_type;
            dict["@p_text"] = text_body;
            dict["@p_email_from_html_part"] = html_body;
            dict["@p_created_by_user"] = 1; // system
            dict["@p_email_from"] = from;

            int post_id = (int)bd_db.exec_scalar(sql, dict);
            return post_id;
        }

        // Post Attachment
        public static void insert_post_attachment_from_dataurl(int post_id, int issue_id, string image_data)
        {

            // turn UrlData format into bin
            string base_64_string = Regex.Match(image_data, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
            byte[] bytea = Convert.FromBase64String(base_64_string);

            insert_post_attachment(post_id, issue_id,
                "screenshot.png", "image/png", bytea);
        }

        // Post Attachment
        public static void insert_post_attachment_from_email_attachment(int post_id, int issue_id, MimePart part)
        {
            string content_type = part.ContentType.MediaType + "/" + part.ContentType.MediaSubtype;
            MemoryStream memory_stream = new MemoryStream();
            part.Content.DecodeTo(memory_stream);
            byte[] bytea = memory_stream.ToArray();

            insert_post_attachment(post_id, issue_id,
                part.FileName, content_type, bytea);

        }

        // Post Attachment
        public static void insert_post_attachment_from_uploaded_file(int post_id, int issue_id, IFormFile uploaded_file)
        {

            MemoryStream memory_stream = new MemoryStream();
            uploaded_file.CopyTo(memory_stream);
            byte[] bytea = memory_stream.ToArray();

            insert_post_attachment(post_id, issue_id,
                uploaded_file.FileName, uploaded_file.ContentType, bytea);
        }

        // Post Attachment
        static void insert_post_attachment(int post_id, int issue_id,
            string file_name, string content_type, byte[] content)
        {
            var sql = @"insert into post_attachments
                (pa_post, pa_issue, pa_file_name, pa_file_length, pa_file_content_type, pa_content)
                values(@pa_post, @pa_issue, @pa_file_name, @pa_file_length, @pa_file_content_type, @pa_content)";

            var dict = new Dictionary<string, dynamic>();

            dict["@pa_post"] = post_id;
            dict["@pa_issue"] = issue_id;
            dict["@pa_file_name"] = file_name;
            dict["@pa_file_length"] = content.Length;
            dict["@pa_file_content_type"] = content_type;

            dict["@pa_content"] = content;

            bd_db.exec(sql, dict);

        }

        public static int get_incoming_issue_id_from_subject(string subject)
        {
            int issue_id = 0;
            int microseconds = 0;

            // parse the "[#123-456] Computer won't turn on"
            // the 123 is the issue_id
            // the 456 is the microseconds part, "US" format specifier, of the i_created_date
            int start_of_id = subject.IndexOf("[#") + 2;

            if (start_of_id > 0)
            {
                int end_of_id = subject.IndexOf("]");
                if (end_of_id > start_of_id)
                {
                    int length = end_of_id - start_of_id;
                    string issue_and_microseconds = subject.Substring(start_of_id, length);
                    string[] pair = issue_and_microseconds.Split("-");
                    if (pair.Length == 2)
                    {
                        bool is_int = int.TryParse(pair[0], out issue_id);
                        if (is_int)
                        {
                            int.TryParse(pair[1], out microseconds);
                        }
                    }
                }
            }

            if (issue_id == 0)
                return 0;

            // Fetch the issue. 
            // Don't just use the id, because we don't want people to just guess issues.
            // Also use the microseconds part of the creation timestamp.
            var sql = @"select i_id from issues where i_id = @i_id 
                and to_char(i_created_date, 'US') = @microseconds";

            var dict = new Dictionary<string, dynamic>();
            dict["@i_id"] = issue_id;
            dict["@microseconds"] = microseconds.ToString();

            object obj = bd_db.exec_scalar(sql, dict);

            if (obj is not null)
                return issue_id;
            else
                return 0;

        }

        public static int get_default_for_custom_field(string number)
        {
            if (bd_config.get("CustomFieldEnabled" + number) == 0)
                return 0;

            string sql = "select c$_id from custom_$ where c$_is_default is true order by c$_name limit 1";
            sql = sql.Replace("$", number);

            object obj = bd_db.exec_scalar(sql);

            if (obj is null)
                return 0;
            else
                return (int)obj;

        }


    }
}
