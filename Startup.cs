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
using Npgsql.Logging;
using Microsoft.AspNetCore.Mvc;

namespace budoco
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            bd_util.log("Startup");
            Configuration = configuration;

            bd_util.log(Configuration["Budoco:DebugWhatEnvIsThis"]);

            //NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug, false, false);
            NpgsqlLogManager.Provider = new bd_pg_log_provider();

            bd_db.update_db_schema(env.ContentRootPath);

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

            // for screenshots
            services.AddCors();


            // services.Configure<ApiBehaviorOptions>(options =>
            // {
            //     options.SuppressModelStateInvalidFilter = true;

            // });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery)
        {
            bd_util.log("Configure");

            app.UseCors();

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


                bd_util.log("Startup.cs URL: " + context.Request.GetDisplayUrl());

                antiforgery.GetTokens(context);

                bool allowed = bd_util.check_user_permissions(context);

                if (allowed)
                {
                    bd_db.query_count = 0; // just to see how many queries we do on busy pages. Not threadsafe, but doesn't matter.
                    await next.Invoke();
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

        }
    }
}
