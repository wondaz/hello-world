using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json.Linq;

namespace Web.FourS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapPageRoute("Report", "report", "~/Content/page/report.aspx");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "Web.FourS.Controllers" }
            );
 
            ModelBinders.Binders.Add(typeof(JObject), new JObjectModelBinder());
        }
    }
}