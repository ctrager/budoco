using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Extensions;
using Serilog;
using RazorPartialToString.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Http.Features;

namespace budoco
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            bd_util.log("Startup");
            Configuration = configuration;

            bd_util.log(Configuration["Budoco:DebugWhatEnvIsThis"]);

            // If there are pending outgoing emails try to send them.
            bd_email.spawn_email_sending_thread();

            bd_email.spawn_email_receiving_thread();

            bd_email.spawn_registration_request_expiration_thread();


        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            bd_util.log("ConfigureServices");

            // services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                // options.CheckConsentNeeded = context => false; // Default is true, make it false
                //options.CheckConsentNeeded = false;
                // So that if we click a budoco link in email, it works
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddHttpContextAccessor();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
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
            bd_util.log("Configure");

            // https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0
            // for nginx
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //if (env.IsDevelopment())
            //{
            if (bd_config.get(bd_config.UseDeveloperExceptionPage) == 1)
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

            // for redirecting https to http - commented out because nginx is doing this for us and it's annoying in dev
            // app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseSerilogRequestLogging();

            // corey added a step in the pipeline
            app.Use(async (context, next) =>
            {
                // The default behavior expires the session id every app restart which
                // meant I had to re-log in every time I changed code    
                string budoco_session_id = null;
                if (context.Request.Cookies.ContainsKey(bd_util.BUDOCO_SESSION_ID))
                {
                    budoco_session_id = context.Request.Cookies[bd_util.BUDOCO_SESSION_ID];
                    context.Session.SetString(bd_util.BUDOCO_SESSION_ID, budoco_session_id);
                }

                bd_util.log(
                    DateTime.Now.ToString("hh:mm:ss")
                    + " "
                    + context.Request.GetDisplayUrl()
                    + ", "
                    + budoco_session_id);

                // let's do it every url, so that we don't have to restart
                bd_config.load_config();

                // For counting how many queries per page, but only
                // trustworthy when there's one client, because it's
                // global, not per session
                bd_db.query_count = 0;

                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
