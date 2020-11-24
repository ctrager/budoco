using System;
using System.Data;
using Npgsql;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace budoco
{
    /*

        Budoco doesn't use an ORM. Just straight SQL, parameterized queries.
        Old school, fast, easy.

    */

    public static class bd_db
    {

        public static string get_connection_string()
        {
            //"server=127.0.0.1;database=budoco;user id='postgres';password='password';"

            string connection_string = "server=" + bd_config.get("DbServer");
            connection_string += ";database=" + bd_config.get("DbDatabase");
            connection_string += ";user id=" + bd_config.get("DbUser");
            connection_string += ";password='" + bd_config.get("DbPassword");
            connection_string += "'";
            return connection_string;
        }

        public static DataTable get_datatable(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {
            log_sql(sql, sql_parameters);

            DataSet ds = new DataSet();

            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                var cmd = create_command(conn, sql, sql_parameters);
                var da = new NpgsqlDataAdapter(cmd);
                da.Fill(ds);

                return ds.Tables[0];
            }
        }

        public static DataRow get_datarow(string sql, Dictionary<string, dynamic> sql_parameters = null)
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
            log_sql(sql, sql_parameters);
            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                var cmd = create_command(conn, sql, sql_parameters);
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static object exec_scalar(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {
            log_sql(sql, sql_parameters);
            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                var cmd = create_command(conn, sql, sql_parameters);
                var result = cmd.ExecuteScalar();
                return result;
            }
        }

        public static SelectList prepare_select_list(string sql)
        {
            var list = new List<Dictionary<string, dynamic>>();

            DataTable dt = bd_db.get_datatable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                var d = new Dictionary<string, dynamic>();
                d["val"] = dr[0];
                d["nam"] = dr[1];
                list.Add(d);
            }
            return new SelectList(list, "val", "nam");
        }

        public static bool exists(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {
            object obj = exec_scalar(sql, sql_parameters);
            if (obj is null)
            {
                // looks like postgres returns null when "select 1 where thing = 1" returns no rows 
                return false;
            }
            else
            {
                if ((int)obj > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        static NpgsqlCommand create_command(
            NpgsqlConnection conn,
            string sql,
            Dictionary<string, dynamic> sql_parameters = null)
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
            return cmd;

        }

        static void log_sql(string sql, Dictionary<string, dynamic> sql_parameters = null)
        {
            bd_util.console_write_line(sql);
            if (sql_parameters is not null)
            {
                foreach (string key in sql_parameters.Keys)
                {
                    bd_util.console_write_line(
                        key + "=" + sql_parameters[key].ToString());
                }
            }
        }

        public static byte[] get_bytea(string sql)
        {
            log_sql(sql, null);
            byte[] bytea;
            using (var conn = new NpgsqlConnection(get_connection_string()))
            {
                var cmd = create_command(conn, sql, null);
                var reader = cmd.ExecuteReader();
                reader.Read();
                bytea = (byte[])reader[0];
                reader.Close();
                return bytea;
            }
        }
    }
}