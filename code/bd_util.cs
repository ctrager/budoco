using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

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

        public static string get_flash(HttpContext context)
        {
            string flash = context.Session.GetString("flash");
            context.Session.SetString("flash", "");
            return flash;
        }

    }
}