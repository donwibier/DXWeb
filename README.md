# DXWeb
Repository for several DevExpress releated NuGet packages:

#### DX.Utils
This package contains several helper classes for working with:
* Attributes
* Bit operations
* Collections
* Conversion helpers
* Logging helpers
* Url manipulations
* DataStore<TKey, TModel> base class to work with DataSets ->
  DTO's  including Mapper and Validator classes to support LINQ and CRUD operations.

#### DX.Data.Xpo
This package contains the XpoDatabase and XpoDataStore for easy config and use of DTO pattern incl. XPMapper and XPValidator classes

#### DX.Data.Xpo.Identity
This package contains an XPO based storage mechanism for use with MS Identity to support a small dozen different DB engines.

Quick config on .NET Core:
```cs
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

            // Add other services ...
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

```


#### DX.Data.Xpo.Mvc (For ASP.NET Framework ASPxGridView MVC Extension only)

**Please note:** _You will need at least an **active** [DevExpress ASP.NET **License**](https://www.devexpress.com/products/net/controls/asp/) for this package_

Some helper classes to have the ASPxGridView extensions support server-side filtering and sorting based on the XPDataStore implementation.

More documentation to follow...
