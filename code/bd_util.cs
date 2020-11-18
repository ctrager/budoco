using System;
using System.Data;
using Npgsql;
using NpgsqlTypes;

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

    }
}