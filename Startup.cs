using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace budoco
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Console.WriteLine("qq Startup");
            Log.Warning("qq calling Log.Warning in Startup");
            Configuration = configuration;

            Configuration.GetSection("Budoco").Bind(cnfg);

            // test cache
            object o = new object();
            int i = 0;
            o = i;
            MyCache.Set("my_int", o);

        }

        public IConfiguration Configuration { get; }

        public static MyConfig cnfg = new MyConfig();


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("qq ConfigureServices");

            services.AddDistributedMemoryCache();

            services.Configure<CookiePolicyOptions>(options =>
                    {
                        // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                        options.CheckConsentNeeded = context => false; // Default is true, make it false
                        //options.CheckConsentNeeded = false;
                    });

            services.AddHttpContextAccessor();
            //TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                //options.Cookie.HttpOnly = false;
                options.Cookie.IsEssential = true;
                //options.Cookie.
            });

            services.AddRazorPages();
            Console.WriteLine("qq DbConnectionString follows: ");
            Console.WriteLine(Configuration["Budoco:DbConnectionString"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Console.WriteLine("qq Configure");

            if (env.IsDevelopment())
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
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            // corey added a step in the pipeline
            app.Use(async (context, next) =>
            {
                int my_int = (int)MyCache.Get("my_int");
                object o = new object();
                o = ++my_int;
                MyCache.Set("my_int", o);
                Console.WriteLine(context.Request.Path + "," + my_int.ToString());

                await next.Invoke();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

        }

    }

}
