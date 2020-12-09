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
        public static void threadproc_send_emails()
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
                    bd_util.log(exception.Message);
                    bd_util.log(exception.StackTrace);
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


        public static void send_email(string to, string subject, string body_text, int post_id = 0)
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
                Console.WriteLine("bytea.Length");
                Console.WriteLine(bytea.Length);


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

        public static void threadproc_fetch_incoming_messages()
        {
            while (true)
            {
                if (bd_config.get(bd_config.EnableIncomingEmail) == 1)
                {
                    fetch_incoming_messages();
                }

                // config in seconds, func expects milliseconds
                int milliseconds = 1000 * bd_config.get(bd_config.SecondsToSleepAfterCheckingIncomingEmail);
                System.Threading.Thread.Sleep(milliseconds);
            }
        }

        public static void fetch_incoming_messages()
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
            bd_util.log("fake email input:" + path);
            FileStream stream = File.OpenRead(path);
            var parser = new MimeParser(stream);
            MimeMessage message = parser.ParseMessage();
            process_incoming_message(message);
        }

        static bool process_incoming_message(MimeMessage message)
        {

            bd_util.log("email subject:" + message.Subject);
            string debug_folder = bd_config.get(bd_config.DebugFolderToSaveEmails);

            if (debug_folder != "")
            {
                filename_suffix_count++;
                var stream = File.Create(debug_folder + "/budoco_email_" + filename_suffix_count.ToString() + ".txt");
                message.WriteTo(stream);
                stream.Close();
            }

            int issue_id = get_incoming_issue_id_from_subject(message.Subject);

            if (issue_id == 0)
            {
                return false; // we don't recognize this email as one of ours
            }

            InternetAddress from = message.From[0];
            bd_util.log("email from:" + from.ToString());
            bd_util.log(message.TextBody);
            string text_body = message.TextBody.Trim();
            string html_body = message.HtmlBody.Trim();

            if (text_body is null)
            {
                text_body = "";
            }
            if (html_body is null)
            {
                html_body = "";
            }

            int post_id = create_post_from_email(issue_id, from.ToString(), text_body, html_body);

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
                            insert_attachment_as_post(post_id, issue_id, part);
                        }

                    }
                }
            }
            return true; // delete this message
        }

        static int create_post_from_email(int issue_id, string from, string text_body, string html_body)
        {
            var sql = @"insert into posts
                (p_issue, p_post_type, p_text, p_created_by_user, p_email_from, p_email_from_html_part)
                values(@p_issue, @p_post_type, @p_text, @p_created_by_user, @p_email_from, @p_email_from_html_part)
                returning p_id";

            var dict = new Dictionary<string, dynamic>();
            dict["@p_issue"] = issue_id;
            dict["@p_post_type"] = "reply";
            dict["@p_text"] = text_body;
            dict["@p_email_from_html_part"] = html_body;
            dict["@p_created_by_user"] = 1; // system
            dict["@p_email_from"] = from;

            int post_id = (int)bd_db.exec_scalar(sql, dict);
            return post_id;
        }

        static void insert_attachment_as_post(int post_id, int issue_id, MimePart part)
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

        static int get_incoming_issue_id_from_subject(string subject)
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


        public static void threadproc_expire_registration_requests()
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
                bd_util.log(time_in_past);
                dict["@date"] = time_in_past;

                sql = @"delete from registration_requests 
                where rr_is_invitation = false and rr_created_date < @date";

                bd_db.exec(sql, dict);

                // Invitations
                hours = bd_config.get(bd_config.InviteUserExpirationInHours);
                time_in_past = DateTime.Now.AddHours(-1 * hours);
                bd_util.log(time_in_past);
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
