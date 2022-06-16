using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.EntityFrameworkCore;
using DX.Data.Xpo;
using DX.Data.Xpo.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DX.Test.Web.Blazor.Areas.Identity;
using DX.Test.Web.Blazor.Data;
using Microsoft.AspNetCore.Components.Authorization;
using DX.Blazor.Identity.Server.Services;
using DX.Blazor.Identity.Models;

namespace DX.Test.Web.Blazor
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			string connStrName = "DefaultConnection";
			//Initialize XPODataLayer / Database	
			services.AddXpoDatabase((o) => {
				o.Name = connStrName;
				o.ConnectionString = Configuration.GetConnectionString(connStrName);
				o.UpdateSchema = DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema;
			});			

			services
				.AddIdentity<ApplicationUser, ApplicationRole>(options => {
					options.Lockout.AllowedForNewUsers = true;
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
					options.Lockout.MaxFailedAccessAttempts = 3;
				})
				.AddDefaultTokenProviders() // <===== Add this for blazor login pages
				.AddXpoIdentityStores(connStrName,
					new ApplicationUserMapper(),
					new ApplicationRoleMapper(),
					new XPUserStoreValidator<string, ApplicationUser, XpoApplicationUser>(),
					new XPRoleStoreValidator<string, ApplicationRole, XpoApplicationRole>())
				.AddDefaultTokenProviders();

			services.AddRazorPages();
			services.AddServerSideBlazor();
			services.AddHttpClient(); // <===== Add this for blazor login pages
			
			services.AddSingleton<WeatherForecastService>();
			
			//services.AddTransient<RegisterUser>();
			//services.AddTransient<RegisterUser>((s) => new RegisterUser());

			// DX.Blazor.Identity.Server configuration
			services.AddScoped<DX.Blazor.Identity.IAuthService<RegisterUserModel, AuthenticationModel>, Services.AuthService>();
			services.AddScoped<DX.Blazor.Identity.Server.TokenProvider>();
			services.AddScoped<AuthenticationStateProvider, DX.Blazor.Identity.Server.AuthStateProvider<ApplicationUser>>();
			// ====
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
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
