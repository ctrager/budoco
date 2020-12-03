using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using MailKit;
using MimeKit;
using Serilog;
using System.IO;
using Microsoft.AspNetCore;
using System.Web;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Diagnostics.CodeAnalysis;

namespace budoco
{
    public static class bd_email
    {
        public static void queue_email(string type, string to, string subject, string body)
        {
            string sql = @"insert into outgoing_email_queue
                (oq_email_type, oq_email_to, oq_email_subject, oq_email_body)
                values(@oq_email_type, @oq_email_to, @oq_email_subject, @oq_email_body)";

            var dict = new Dictionary<string, dynamic>();
            dict["@oq_email_type"] = type;
            dict["@oq_email_to"] = to;
            dict["@oq_email_subject"] = subject;
            dict["@oq_email_body"] = body;

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
                    (string)dr["oq_email_to"], (string)dr["oq_email_subject"], (string)dr["oq_email_body"]);

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


        public static string send_email(string to, string subject, string body)
        {

            var message = new MimeMessage();

            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            bd_util.console_write_line("email to: " + to);
            bd_util.console_write_line("email body: " + body);
            message.Body = new TextPart("plain")
            {
                Text = body
            };

            bd_util.console_write_line("sending email to: " + to + ", subject: " + subject);
            bd_util.console_write_line("body : " + body);
            string result = send_email(message);
            return result;
        }

        public static string send_email(MimeMessage message)
        {
            string result = "";

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
