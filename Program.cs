using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


namespace budoco
{
    public class Program
    {

        public static int Main(string[] args)
        {
            Console.WriteLine("Main"); // here on purpose

            bd_config.load_config();

            string log_file_location = bd_config.get(bd_config.LogFileFolder) + "/budoco_log_.txt";
            Log.Logger = new LoggerConfiguration()

                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("budoco", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(log_file_location,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // Write config to log, even though budoco can pick up most changes without
            // being restarted and we don't log the changed values.
            bd_config.log_config();

            try
            {
                Log.Information("Starting host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
            webBuilder.UseKestrel(options =>
            {
                // Because we will control it with nginx,
                // which gives good specific 413 status rather than vague 400 status
                options.Limits.MaxRequestBodySize = null;
            });
        });

        // public static IHostBuilder CreateHostBuilder(string[] args) =>
        //     Host.CreateDefaultBuilder(args)
        //         // dotnet install package Serilog.AspNetCore
        //         .UseSerilog()

        //         .ConfigureWebHostDefaults(webBuilder =>
        //         {
        //             webBuilder.UseStartup<Startup>();
        //         });
    }
}
