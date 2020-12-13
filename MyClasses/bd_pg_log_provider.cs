using budoco;
using Microsoft.Extensions.Logging;
using Npgsql.Logging;
using System;
using System.Linq;
using Serilog;

public class bd_pg { }; // for logging context

namespace budoco_pg
{

    class bd_pg_log_provider : INpgsqlLoggingProvider
    {
        public NpgsqlLogger CreateLogger(string name)
        {
            return new bd_pg_logger();
        }
    }

    class bd_pg_logger : NpgsqlLogger
    {

        Serilog.ILogger _logger;

        public bd_pg_logger()
        {
            _logger = Serilog.Log.ForContext<bd_pg>();

        }

        public override bool IsEnabled(NpgsqlLogLevel level)
        {
            return true;
        }

        int highest_ms = 0;

        public override void Log(NpgsqlLogLevel level, int connectorId, string msg, Exception exception = null)
        {

            _logger.Write((Serilog.Events.LogEventLevel)level, msg);

            if (msg.StartsWith("Query duration"))
            {

                string digits = new String(msg.Where(Char.IsDigit).ToArray());
                int ms = Convert.ToInt32(digits);
                if (ms > highest_ms)
                {
                    highest_ms = ms;
                    _logger.Information("Query duration high water mark is now " + digits);
                }

            }
        }
    }
}
