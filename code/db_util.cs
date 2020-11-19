using System;
using System.Data;
using Npgsql;
using System.Collections.Generic;

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

        public static string exec(string sql, Dictionary<string, dynamic> sql_parameters)
        {

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                foreach (KeyValuePair<string, dynamic> pair in sql_parameters)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }
                Console.WriteLine(cmd.CommandText);
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static object exec_scalar(string sql, Dictionary<string, dynamic> sql_parameters)
        {

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                foreach (KeyValuePair<string, dynamic> pair in sql_parameters)
                {
                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }
                Console.WriteLine(cmd.CommandText);
                var result = cmd.ExecuteScalar();
                Console.WriteLine("scalar follows");
                Console.WriteLine(result.ToString());
                return result;
            }
        }

    }
}