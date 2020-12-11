using System;
using System.Collections.Generic;
using System.Data;
using MimeKit;
using System.IO;
using MailKit.Net.Imap;
using MailKit;
using System.Threading;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Composition.Convention;

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
            int user_org = 0;

            if (user_id != bd_util.SYSTEM_USER_ID_ZERO)
            {
                user_org = (int)bd_db.exec_scalar("select us_organization from users where us_id = " + user_id.ToString());
            }

            // get defaults

            //int i_organization = user_org == 0 ? get_default : user_org;

            var dict = new Dictionary<string, dynamic>();

            dict["@i_description"] = description;
            dict["@i_details"] = details;
            dict["@i_created_by_user"] = user_id;
            dict["@i_last_updated_user"] = user_id;
            dict["@i_custom_1"] = 0; // custom_1_id;
            dict["@i_custom_2"] = 0; // custom_2_id;
            dict["@i_custom_3"] = 0; // custom_3_id;
            dict["@i_custom_4"] = 0; // custom_4_id;
            dict["@i_custom_5"] = 0; // custom_5_id;
            dict["@i_custom_6"] = 0; // custom_6_id;

            dict["@i_assigned_to_user"] = 0; // assigned_to_user_id;
            dict["@i_organization"] = user_org;


            int issue_id = (int)bd_db.exec_scalar(bd_issue.INSERT_ISSUE_SQL, dict);

            return issue_id;

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

        public static void insert_attachment_as_post(int post_id, int issue_id, MimePart part)
        {
            bd_util.log(part.ContentType + "," + part.FileName);

            var sql = @"insert into post_attachments
                (pa_post, pa_issue, pa_file_name, pa_file_length, pa_file_content_type, pa_content)
                values(@pa_post, @pa_issue, @pa_file_name, @pa_file_length, @pa_file_content_type, @pa_content)";

            var dict = new Dictionary<string, dynamic>();

            dict["@pa_post"] = post_id;
            dict["@pa_issue"] = issue_id;
            dict["@pa_file_name"] = part.FileName;
            dict["@pa_file_content_type"]
                = part.ContentType.MediaType + "/" + part.ContentType.MediaSubtype;

            MemoryStream memory_stream = new MemoryStream();
            part.Content.DecodeTo(memory_stream);
            dict["@pa_file_length"] = memory_stream.Length;
            dict["@pa_content"] = memory_stream.ToArray();

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

    }
}
