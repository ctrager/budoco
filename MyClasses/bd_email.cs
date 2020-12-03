using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Data;
using MimeKit;
using System.IO;

namespace budoco
{
    public static class bd_email
    {
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

            spawn_sending_thread();


        }

        public static void spawn_sending_thread()
        {
            // Spawn sending thread
            // The thread doesn't loop. It sends and then dies.
            // But we spawn it every time we send an email and at startup.
            System.Threading.Thread thread = new System.Threading.Thread(threadproc_send_emails);
            thread.Start();
        }

        public static void threadproc_send_emails()
        {
            var sql = "select * from outgoing_email_queue order by oq_id desc";

            DataTable dt = bd_db.get_datatable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                int oq_id = (int)dr[0];
                bd_util.console_write_line("trying to send " + oq_id.ToString());

                string result = send_email(
                    (string)dr["oq_email_to"],
                    (string)dr["oq_email_subject"],
                    (string)dr["oq_email_body"],
                    (int)dr["oq_post_id"]);

                if (result != "")
                {
                    increment_retry_count(oq_id, result);
                }
                else
                {
                    // Done, good, so delete from queue
                    sql = "delete from outgoing_email_queue where oq_id = " + oq_id.ToString();
                    bd_db.exec(sql);
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


        public static string send_email(string to, string subject, string body_text, int post_id = 0)
        {

            var message = new MimeMessage();

            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            bd_util.console_write_line("send_email to: " + to);
            bd_util.console_write_line("email subject: " + subject);

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
            string result = smtp_send(message);
            return result;
        }


        static void add_file_attachments(int post_id, Multipart multipart)
        {
            string sql = @"/*add_file_attachments p*/ select pa_id, pa_file_content_type, pa_file_name from post_attachments 
                        where pa_post = " + post_id.ToString() + " order by pa_id";

            DataTable dt = bd_db.get_datatable(sql);

            foreach (DataRow dr in dt.Rows)
            {
                // Here and elsewhere, can I combine the two queries?
                int pa_id = (int)dr["pa_id"];
                sql = "/*add_file_attachments pa*/ select pa_content from post_attachments where pa_id = " + pa_id.ToString();
                byte[] bytea = bd_db.get_bytea(sql);
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


        static string smtp_send(MimeMessage message)
        {
            string result = "";

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
                    try
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
                    catch (Exception e)
                    {
                        result = e.Message;
                        bd_util.console_write_line(e.Message);
                        bd_util.console_write_line(e.StackTrace);
                    }
                }
            }
            return result;
        }
    }
}

/*
         void SendIssueEmail()
         {

             // get fields from issue
             var sql = @"select 
                 cast(i_id as text) as ""id"",
                 cast(i_created_date as text) as ""date"", 
                 i_description from issues where i_id = " + id.ToString();
             DataRow dr_issue = bd_db.get_datarow(sql);

             //post_error = "Email isn't being sent yet. This feature is under construction";

             var message = new MimeMessage();
             message.To.Add(new MailboxAddress("", "ctrager@yahoo.com"));

             string identifier1 = "[#" + (string)dr_issue["id"] + "] ";
             string identifier2 = "          [" + (string)dr_issue["date"] + "]";

             // We use the id and very precise create date as like a GUID to tie the
             // incoming emails back to this issue
             // [Issue:123] My computer won't turn on [2020-01-01 11:59:59.233242]
             message.Subject = identifier1 + (string)dr_issue["i_description"] + identifier2;

             // create our message text, just like before (except don't set it as the message.Body)
             var body = new TextPart("plain")
             {
                 Text = post_text
             };

             var multipart = new Multipart("mixed");
             multipart.Add(body);

             if (uploaded_file1 != null)
             {
                 MemoryStream memory_stream = new MemoryStream();
                 uploaded_file1.CopyTo(memory_stream);


                 // create an image attachment for the file located at path
                 //var pair = uploaded_file1.ContentType().Split("image","jpeg");
                 var attachment = new MimePart("image", "jpeg")
                 {
                     Content = new MimeContent(memory_stream, ContentEncoding.Default),
                     ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                     ContentTransferEncoding = ContentEncoding.Base64,
                     FileName = uploaded_file1.FileName
                 };
                 multipart.Add(attachment);
             }



             // now create the multipart/mixed container to hold the message text and the
             // image attachment
             //var multipart = new Multipart("mixed");
             //multipart.Add(body);
             //multipart.Add(attachment);

             // now set the multipart/mixed as the message body
             message.Body = multipart;
             bd_email.send_email(message);

         }
 */