using System.Web.Mvc;

namespace Web.FourS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            filters.Add(new MvcHandleErrorAttribute());
            //filters.Add(new AuthorizeAttribute());
            filters.Add(new MvcDisposeFilter());
            filters.Add(new MvcMenuFilter());
        }
    }
}