using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CAD_webapp.Startup))]
namespace CAD_webapp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
