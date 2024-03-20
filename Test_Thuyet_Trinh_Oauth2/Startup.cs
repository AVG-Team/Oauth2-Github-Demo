using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Test_Thuyet_Trinh_Oauth2.Startup))]
namespace Test_Thuyet_Trinh_Oauth2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
