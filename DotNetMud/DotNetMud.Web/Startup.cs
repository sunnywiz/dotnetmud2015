using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DotNetMud.Web.Startup))]
namespace DotNetMud.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR(); 
        }
    }
}
