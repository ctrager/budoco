using budoco;
using Npgsql.Logging;
using System;
using System.Linq;

namespace budoco
{
    class bd_pg_log_provider : INpgsqlLoggingProvider
    {
        public NpgsqlLogger CreateLogger(string name)
        {
            return new bd_pg_logger(name);
        }
    }

    class bd_pg_logger : NpgsqlLogger
    {
        int highest_ms = 0;
        internal bd_pg_logger(string name)
        {
        }

        public override bool IsEnabled(NpgsqlLogLevel level)
        {
            return true;
        }

        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception exception = null)
        {
            if (msg.StartsWith("Query duration"))
            {
                bd_util.log(msg);

                string digits = new String(msg.Where(Char.IsDigit).ToArray());
                int ms = Convert.ToInt32(digits);
                if (ms > highest_ms)
                {
                    highest_ms = ms;
                    bd_util.log("Query duration high water mark is now " + digits);
                }

            }
            else if (level == NpgsqlLogLevel.Warn)
            {
                Serilog.Log.Warning(msg);
                Console.WriteLine("PG Warning: " + msg);
            }
            else if (level == NpgsqlLogLevel.Error)
            {
                Serilog.Log.Error(msg);
                Console.WriteLine("PG Error: " + msg);
            }
            else if (level == NpgsqlLogLevel.Fatal)
            {
                Serilog.Log.Fatal(msg);
                Console.WriteLine("PG Fatal: " + msg);
            }

        }
    }
}
