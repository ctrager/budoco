namespace budoco
{
    public class MyConfig
    {
        public string DbConnectionString { get; set; }
        public string AppName { get; set; }

        public int RowsPerPage { get; set; }

        public bool AutoConfirmRegistration { get; set; }
    }
}