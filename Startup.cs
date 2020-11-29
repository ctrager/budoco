using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;
using RazorPartialToString.Services;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace budoco
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            bd_util.console_write_line("Startup");
            Configuration = configuration;

            bd_util.console_write_line(Configuration["Budoco:DebugWhatEnvIsThis"]);

            // test cache
            // object o = new object();
            // int i = 0;
            // o = i;
            // MyCache.Set("my_int", o);

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            bd_util.console_write_line("ConfigureServices");

            // services.AddDistributedMemoryCache();

            // services.Configure<CookiePolicyOptions>(options =>
            //         {
            //             // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            //             // options.CheckConsentNeeded = context => false; // Default is true, make it false
            //             //options.CheckConsentNeeded = false;
            //             options.
            //         });

            services.AddHttpContextAccessor();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.IsEssential = true;
            });

            services.AddRazorPages();

            services.AddTransient<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();

            // if nginx is not on same machine
            // services.Configure<ForwardedHeadersOptions>(options =>
            // {
            //     options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            // });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            bd_util.console_write_line("Configure");



            // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0
            // for nginx
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //if (env.IsDevelopment())
            //{
            if (bd_config.get("UseDeveloperExceptionPage") == 1)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();

            // for redirecting https to http
            // app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseSerilogRequestLogging();

            // corey added a step in the pipeline
            app.Use(async (context, next) =>
            {
                // int my_int = (int)MyCache.Get("my_int");
                // object o = new object();
                // o = ++my_int;
                // MyCache.Set("my_int", o);

                // Corey says: use our own guid for authentication so that when we 
                // restart we don't have to log in again.
                // Framework generates a new guid each restart. We don't.
                // 
                string budoco_session_id = null;
                if (context.Request.Cookies.ContainsKey(bd_util.BUDOCO_SESSION_ID))
                {
                    budoco_session_id = context.Request.Cookies[bd_util.BUDOCO_SESSION_ID];
                    context.Session.SetString(bd_util.BUDOCO_SESSION_ID, budoco_session_id);
                }

                bd_util.console_write_line(
                    DateTime.Now.ToString("hh:mm:ss")
                    + " "
                    + context.Request.GetDisplayUrl()
                    + ", "
                    + budoco_session_id);

                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
