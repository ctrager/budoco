using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Collections.Generic;
using System.Data;

namespace budoco
{
    public static class bd_util
    {
        public static int get_int_or_zero_from_string(string s)
        {
            if (s is null)
            {
                return 0;
            }

            return Convert.ToInt32(s);
        }

        public static string get_flash_msg(HttpContext context)
        {
            string flash = context.Session.GetString("flash_msg");
            context.Session.SetString("flash_msg", "");
            return flash;
        }

        public static void set_flash_msg(HttpContext context, string msg)
        {
            context.Session.SetString("flash_msg", msg);
        }

        public static List<string> get_flash_errs(HttpContext context)
        {
            string s = context.Session.GetString("flash_err");
            context.Session.SetString("flash_err", "");

            if (s is null)
            {
                s = "";
            }

            string[] err_array = s.Split('|');

            return new List<string>(err_array);

        }

        public static void set_flash_err(HttpContext context, List<string> errs)
        {
            string s = string.Join('|', errs);
            context.Session.SetString("flash_err", s);
        }

        public static void redirect_if_not_logged_in(HttpContext context)
        {
            // Session was changing every page. Stackoverflow person
            // said session needs at least one thing. This seems to work.
            context.Session.SetString("dummy", "dummy");

            string sql = @"select * from sessions 
                inner join users on se_user = us_id
                where se_id = '" + context.Session.Id + "'";

            DataRow dr = db_util.get_datarow(sql);

            if (dr is null)
            {
                context.Response.Redirect("/Login");
                return;
            }

            context.Session.SetInt32("us_id", (int)dr["us_id"]);
            context.Session.SetString("us_username", (string)dr["us_username"]);
            context.Session.SetString("us_email", (string)dr["us_email"]);
            context.Session.SetInt32("us_is_admin", Convert.ToInt32((bool)dr["us_is_admin"]));

        }

    }

}