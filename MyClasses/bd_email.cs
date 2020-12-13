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
    public static class bd_email
    {
        static int filename_suffix_count = 0;

        static List<string[]> filter_lines = new List<string[]>();

        public static void queue_email(string type, string to, string subject, string body, int post_id = 0)
        {
            string sql = @"insert into outgoing_email_queue
                (oq_email_type, oq_email_to, oq_email_subject, oq_email_body, oq_post_id)
                values(@oq_email_type, @oq_email_to, @oq_email_subject, @oq_email_body, @oq_post_id)";

            var dict = new Dictionary<string, dynamic>();
            dict["@oq_email_type"] = type;
            dict["@oq_email_to"] = to;
            dict["@oq_email_subject"] = subject;
            dict["@oq_email_body"] = body;
            dict["@oq_post_id"] = post_id;

            bd_db.exec(sql, dict);

            spawn_email_sending_thread();

        }

        public static void spawn_email_sending_thread()
        {
            // Spawn sending thread
            // The thread doesn't loop. It sends and then dies.
            // But we spawn it every time we put an email into the outgoing queue.
            Thread thread = new Thread(threadproc_send_emails);
            thread.Start();
        }

        public static void spawn_email_receiving_thread()
        {
            string path = bd_config.get(bd_config.DebugPathToEmailFile);
            if (!string.IsNullOrWhiteSpace(path))
            {
                debug_read_message_from_file(path);
                return;
            }

            // This thread loops
            Thread thread = new Thread(threadproc_fetch_incoming_messages);
            thread.Start();
        }

        public static void spawn_registration_request_expiration_thread()
        {

            // Spawn sending thread
            // loops, sleeps
            Thread thread = new Thread(threadproc_expire_registration_requests);
            thread.Start();
        }


        // read the table outgoing_email_queue, try to send an email for each row
        // and if good, delete the row
        static void threadproc_send_emails()
        {
            var sql = @"select * from outgoing_email_queue 
                where oq_sending_attempt_count < @max_retries
                order by oq_id desc";

            var dict = new Dictionary<string, dynamic>();
            dict["@max_retries"] = bd_config.get(bd_config.MaxNumberOfSendingRetries);

            DataTable dt = bd_db.get_datatable(sql, dict);

            foreach (DataRow dr in dt.Rows)
            {
                int oq_id = (int)dr[0];
                bd_util.log("trying to send " + oq_id.ToString());

                try
                {
                    send_email(
                        (string)dr["oq_email_to"],
                        (string)dr["oq_email_subject"],
                        (string)dr["oq_email_body"],
                        (int)dr["oq_post_id"]);

                    // Done, good, so delete from queue
                    sql = "delete from outgoing_email_queue where oq_id = " + oq_id.ToString();
                    bd_db.exec(sql);
                }
                catch (Exception exception)
                {
                    bd_util.log(exception.Message, Serilog.Events.LogEventLevel.Error);
                    bd_util.log(exception.StackTrace, Serilog.Events.LogEventLevel.Error);
                    increment_retry_count(oq_id, exception.Message);
                }
            }
        }

        static void increment_retry_count(int oq_id, string exception_message)
        {
            var sql = @"update outgoing_email_queue 
                        set oq_sending_attempt_count = oq_sending_attempt_count + 1, 
                        oq_last_sending_attempt_date = CURRENT_TIMESTAMP,
                        oq_last_exception = @exception
                        where oq_id = @oq_id";
            var dict = new Dictionary<string, dynamic>();
            dict["@exception"] = exception_message;
            dict["@oq_id"] = oq_id;

            bd_db.exec(sql, dict);
        }


        static void send_email(string to, string subject, string body_text, int post_id = 0)
        {

            var message = new MimeMessage();

            string[] addresses = to.Split(",");
            for (int i = 0; i < addresses.Length; i++)
            {
                var parsed_address = new System.Net.Mail.MailAddress(addresses[i]);
                message.To.Add(new MailboxAddress(
                    parsed_address.DisplayName,
                    parsed_address.Address));
            }
            message.Subject = subject;
            bd_util.log("send_email to: " + to);
            bd_util.log("email subject: " + subject);

            var multipart = new Multipart("mixed");

            // plain text body
            var body = new TextPart("plain")
            {
                Text = body_text
            };
            multipart.Add(body);

            if (post_id != 0)
            {
                add_file_attachments(post_id, multipart);
            }

            message.Body = multipart;
            smtp_send(message);
        }


        static async void add_file_attachments(int post_id, Multipart multipart)
        {
            string sql = @"/*add_file_attachments p*/ select pa_id, pa_file_content_type, pa_file_name from post_attachments 
                        where pa_post = " + post_id.ToString() + " order by pa_id";

            DataTable dt = bd_db.get_datatable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                // Here and elsewhere, can I combine the two queries?
                int pa_id = (int)dr["pa_id"];
                sql = "/*add_file_attachments pa*/ select pa_content from post_attachments where pa_id = " + pa_id.ToString();
                byte[] bytea = await bd_db.get_bytea_async(sql);

                // for example "image", "jpeg"
                string[] content_type_pair = ((string)dr["pa_file_content_type"]).Split("/");

                MemoryStream memory_stream = new MemoryStream(bytea);
                var attachment = new MimePart(content_type_pair[0], content_type_pair[1])
                {
                    Content = new MimeContent(memory_stream, ContentEncoding.Default),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = (string)dr["pa_file_name"]
                };
                multipart.Add(attachment);
            };
        }


        static void smtp_send(MimeMessage message)
        {

            // all our outgoing emails get sent from what's
            // configured in config file
            var from_address = new MailboxAddress(
                bd_config.get(bd_config.OutgoingEmailDisplayName),
                bd_config.get(bd_config.SmtpUser));

            message.From.Add(from_address);

            if (bd_config.get(bd_config.DebugSkipSendingEmails) == 0)
            {
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect(
                        bd_config.get(bd_config.SmtpHost),
                        bd_config.get(bd_config.SmtpPort),
                        MailKit.Security.SecureSocketOptions.Auto);

                    string smtp_user = bd_config.get(bd_config.SmtpUser);
                    string smtp_password = bd_config.get(bd_config.SmtpPassword);
                    client.Authenticate(smtp_user, smtp_password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }

        }

        static void threadproc_fetch_incoming_messages()
        {
            while (true)
            {
                if (bd_config.get(bd_config.EnableIncomingEmail) == 1)
                {
                    load_filter_lines();
                    fetch_incoming_messages();
                }

                // config in seconds, func expects milliseconds
                int milliseconds = 1000 * bd_config.get(bd_config.SecondsToSleepAfterCheckingIncomingEmail);
                System.Threading.Thread.Sleep(milliseconds);
            }
        }

        static void load_filter_lines()
        {
            // get the lines for filtering by subject and from

            filter_lines.Clear();
            if (!File.Exists("incoming_email_filter_active.txt"))
                return;


            var lines = File.ReadLines("incoming_email_filter_active.txt");

            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }
                string[] pair = line.Split(":");
                if (pair.Length != 2)
                {
                    continue;
                }
                pair[0] = pair[0].Trim();

                if (pair[0] == AllowIfFromContains
                || pair[0] == AllowIfSubjectContains
                || pair[0] == DenyIfFromContains
                || pair[0] == DenyIfSubjectContains)
                {
                    pair[1] = pair[1].Trim().ToLower();
                    bd_util.log("filter_line:" + pair[0] + ":" + pair[1]);
                    filter_lines.Add(pair);
                }
                else
                {
                    if (pair[0] == "Allow" || pair[0] == "Deny")
                    {
                        pair[1] = null;
                        bd_util.log("filter_line:" + pair[0]);
                        filter_lines.Add(pair);
                    }
                }

            } // end foreach
        }

        static void fetch_incoming_messages()
        {

            using (var client = new ImapClient())
            {
                try
                {
                    bd_util.log("fetch_incoming_messages Connecting to Imap");
                    client.Connect(
                        bd_config.get(bd_config.ImapHost),
                        bd_config.get(bd_config.ImapPort),
                        true); // ssl

                    client.Authenticate(
                         bd_config.get(bd_config.ImapUser),
                         bd_config.get(bd_config.ImapPassword)
                    );

                    // The Inbox folder is always available on all IMAP servers...
                    IMailFolder inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadWrite);

                    bd_util.log("Total messages: " + inbox.Count.ToString());
                    bd_util.log("Recent messages: " + inbox.Recent.ToString());

                    for (int index = 0; index < inbox.Count; index++)
                    {
                        var message = inbox.GetMessage(index);
                        bool processed = process_incoming_message(message);
                        if (processed)
                        {
                            // delete it
                            if (bd_config.get(bd_config.DebugSkipDeleteOfIncomingEmails) == 0)
                            {
                                bd_util.log("Actually deleting: " + message.Subject);
                                inbox.AddFlags(index, MessageFlags.Deleted, true);
                                client.Inbox.Expunge();
                            }
                        }
                    }

                    client.Disconnect(true);
                    bd_util.log("Disconnecting from Imap");
                }
                catch (Exception exception)
                {
                    bd_util.log(exception.Message);
                    bd_util.log(exception.StackTrace);

                }
            }
        }

        static void debug_read_message_from_file(string path)
        {
            bd_util.log("fake email input:" + path, Serilog.Events.LogEventLevel.Warning);
            FileStream stream = File.OpenRead(path);
            var parser = new MimeParser(stream);
            MimeMessage message = parser.ParseMessage();
            process_incoming_message(message);
        }

        const string DenyIfSubjectContains = "DenyIfSubjectContains";
        const string DenyIfFromContains = "DenyIfFromContains";
        const string AllowIfSubjectContains = "AllowIfSubjectContains";
        const string AllowIfFromContains = "AllowIfFromContains";

        static bool process_incoming_message(MimeMessage message)
        {

            bd_util.log("email subject:" + message.Subject);
            string debug_folder = bd_config.get(bd_config.DebugFolderToSaveEmails);

            // save the incoming email as a text file, so we can study it
            if (debug_folder != "")
            {
                filename_suffix_count++;
                var stream = File.Create(debug_folder + "/budoco_email_" + filename_suffix_count.ToString() + ".txt");
                message.WriteTo(stream);
                stream.Close();
            }

            int issue_id = bd_issue.get_incoming_issue_id_from_subject(message.Subject);

            // no magic number in the subject, so this isn't a reply 
            if (issue_id == 0)
            {
                // Create a new issue?
                if (bd_config.get(bd_config.EnableIncomingEmailIssueCreation) == 0)
                {
                    return false; // we don't recognize this email as one of ours
                }
            }

            string from = message.From[0].ToString();

            bd_util.log("email from: " + from + ", Subject: " + message.Subject);

            string text_body = message.TextBody;
            string html_body = message.HtmlBody;


            if (text_body is null)
            {
                text_body = "";
            }
            else
            {
                text_body = text_body.Trim();
            }

            if (html_body is null)
            {
                html_body = "";
            }
            else
            {
                html_body = html_body.Trim();
            }

            if (issue_id == 0)
            {

                if (!is_email_allowed(from, message.Subject))
                    return false;

                issue_id = bd_issue.create_issue(message.Subject, text_body, bd_util.SYSTEM_USER_ID);
            }

            int post_id = bd_issue.create_post_from_email(bd_issue.POST_TYPE_REPLY_IN, issue_id, from.ToString(), text_body, html_body);

            insert_attachments(post_id, issue_id, message);

            return true; // delete this message
        }

        static bool is_email_allowed(string from, string subject)
        {
            string lower_from = from.ToLower();
            string lower_subject = subject.ToLower();

            foreach (string[] pair in filter_lines)
            {
                if (pair[0] == DenyIfFromContains && lower_from.Contains(pair[1]))
                {
                    bd_util.log(pair[0] + ":" + lower_from);
                    return false; // denied
                }

                if (pair[0] == DenyIfSubjectContains && lower_subject.Contains(pair[1]))
                {
                    bd_util.log(pair[0] + ":" + lower_subject);
                    return false;
                }

                if (pair[0] == AllowIfFromContains && lower_from.Contains(pair[1]))
                {
                    bd_util.log(pair[0] + ":" + lower_from);
                    return true; // allow
                }

                if (pair[0] == AllowIfSubjectContains && lower_subject.Contains(pair[1]))
                {
                    bd_util.log(pair[0] + ":" + lower_subject);
                    return true;
                }

                if (pair[0] == "Deny")
                {
                    bd_util.log("Deny " + lower_from + " " + lower_subject);
                    return false;
                }
            }

            return true; // allowed
        }


        static void insert_attachments(int post_id, int issue_id, MimeMessage message)
        {

            using (var iter = new MimeIterator(message))
            {
                // collect our list of attachments and their parent multiparts
                while (iter.MoveNext())
                {
                    var multipart = iter.Parent as Multipart;
                    var part = iter.Current as MimePart;
                    if (part is not null)
                    {
                        if (part.FileName is not null)
                        {
                            bd_issue.insert_post_attachment_from_email_attachment(post_id, issue_id, part);
                        }

                    }
                }
            }
        }

        public static bool validate_email_address(string address)
        {
            //if (!EmailValidation.EmailValidator.Validate(address))
            try
            {
                System.Net.Mail.MailAddress ma = new System.Net.Mail.MailAddress(address);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        static void threadproc_expire_registration_requests()
        {
            while (true)
            {

                string sql;
                int hours;
                DateTime time_in_past;
                var dict = new Dictionary<string, dynamic>();

                // Regisgtrations
                hours = bd_config.get(bd_config.RegistrationRequestExpirationInHours);
                time_in_past = DateTime.Now.AddHours(-1 * hours);
                dict["@date"] = time_in_past;

                sql = @"delete from registration_requests 
                where rr_is_invitation = false and rr_created_date < @date";

                bd_db.exec(sql, dict);

                // Invitations
                hours = bd_config.get(bd_config.InviteUserExpirationInHours);
                time_in_past = DateTime.Now.AddHours(-1 * hours);
                dict["@date"] = time_in_past;

                sql = @"delete from registration_requests 
                where rr_is_invitation = true and rr_created_date < @date";

                bd_db.exec(sql, dict);

                // sleep for an hour
                int milliseconds = 1000 * 60 * 60;
                System.Threading.Thread.Sleep(milliseconds);
            }
        }
    }
}
