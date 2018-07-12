using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DX.Test.Web.MVC.Startup))]
namespace DX.Test.Web.MVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
