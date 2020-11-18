using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace budoco
{
    public static class db_util
    {

        public static string get_connection_string()
        {
            return Startup.cnfg["Btnet:DbConnectionString"];
        }

        public static DataTable get_datatable(string sql)
        {

            Console.WriteLine(sql);

            DataSet ds = new DataSet();

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                var da = new NpgsqlDataAdapter(sql, conn);
                da.Fill(ds);
            }
            return ds.Tables[0];
        }

        public static DataRow get_datarow(string sql)
        {
            DataTable dt = get_datatable(sql);

            if (dt.Rows.Count == 1)
            {
                return dt.Rows[0];

            }
            else
            {
                return null;

            }
        }

        public static string exec(string sql)
        {

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            return null;
        }

    }
}