using Frame.Core;
using System.Web.Http;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S
{
    public class FourSAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Biz4S";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            APP.DB_DEFAULT_CONN_NAME = "DMS_4S";
            context.MapRoute(
                AreaName + "default",
                AreaName + "/{controller}/{action}/{id}",
                new { area = AreaName, controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { string.Format("Web.FourS.Areas.{0}.Controllers", this.AreaName) }
            );

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
               AreaName + "ApiWeb",
               "api/" + AreaName + "/{controller}/{action}/{id}",
               new
               {
                   area = AreaName,
                   action = RouteParameter.Optional,
                   id = RouteParameter.Optional,
                   namespaceName = new[] { string.Format("Web.FourS.Areas.{0}.Controllers", AreaName) }
               },
               new { action = new StartWithConstraint() }
           );
        }
    }
}