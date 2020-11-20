using System;
using System.Data;
using Npgsql;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace budoco
{
    public static class db_util
    {

        public static string get_connection_string()
        {
            return Startup.cnfg.DbConnectionString;
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

        public static DataTable get_datatable(string sql, Dictionary<string, dynamic> sql_parameters)
        {

            Console.WriteLine(sql);

            DataSet ds = new DataSet();

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();

                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);

                foreach (KeyValuePair<string, dynamic> pair in sql_parameters)
                {

                    cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                }

                var da = new NpgsqlDataAdapter(cmd);
                da.Fill(ds);

                return ds.Tables[0];
            }
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

        public static DataRow get_datarow(string sql, Dictionary<string, dynamic> sql_parameters)
        {
            DataTable dt = get_datatable(sql, sql_parameters);

            if (dt.Rows.Count == 1)
            {
                return dt.Rows[0];
            }
            else
            {
                return null;
            }
        }

        public static string exec(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {
            Console.WriteLine(sql);
            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                if (sql_parameters is not null)
                {
                    foreach (KeyValuePair<string, dynamic> pair in sql_parameters)
                    {

                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                }

                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static object exec_scalar(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {

            Console.WriteLine(sql);
            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                conn.Open();
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                if (sql_parameters is not null)
                {
                    foreach (KeyValuePair<string, dynamic> pair in sql_parameters)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                }
                var result = cmd.ExecuteScalar();
                return result;
            }
        }

        public static SelectList prepare_select_list(string sql)
        {
            var list = new List<Dictionary<string, dynamic>>();

            DataTable dt = db_util.get_datatable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                var d = new Dictionary<string, dynamic>();
                d["val"] = dr[0];
                d["nam"] = dr[1];
                list.Add(d);
            }
            return new SelectList(list, "val", "nam");
        }


    }
}