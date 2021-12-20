using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using DX.Data.Xpo;
using DX.Data.Xpo.Identity;
using DX.Test.Web.Core.Data;
using DX.Test.Web.Core.Models;
using DX.Test.Web.Core.Services;

namespace DX.Test.Web.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string connStrName = "DefaultConnection";
            //Initialize XPODataLayer / Database	
            services.AddXpoDatabase((o) => {
                o.Name = connStrName;
                o.ConnectionString = Configuration.GetConnectionString(connStrName);
            });

            //Initialize identity to use XPO
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options => {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 3;					
                })	
                .AddXpoIdentityStores<ApplicationUser, XpoApplicationUser, ApplicationRole, XpoApplicationRole>(connStrName,
					new ApplicationUserMapper(), 
					new ApplicationRoleMapper(),
					new XPUserStoreValidator<string, ApplicationUser, XpoApplicationUser>(),
					new XPRoleStoreValidator<string, ApplicationRole, XpoApplicationRole>())				
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            //services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();                
                //app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
