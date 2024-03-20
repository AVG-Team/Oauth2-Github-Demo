using System.Web;
using System.Web.Mvc;

namespace Test_Thuyet_Trinh_Oauth2
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
