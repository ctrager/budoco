namespace budoco
{
    public class MyConfig
    {
        public string AppName { get; set; }

        public string DbServer { get; set; }
        public string DbDatabase { get; set; }
        public string DbUser { get; set; }
        public string DbPasswordFile { get; set; }

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string SmtpPasswordFile { get; set; }

        public int RowsPerPage { get; set; }
        public bool AutoConfirmRegistration { get; set; }
    }
}