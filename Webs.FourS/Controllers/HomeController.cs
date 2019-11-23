using System.Web.Mvc;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Controllers
{
    [MvcMenuFilter(false)]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //!User.Identity.IsAuthenticated ||
            if ( FormsAuth.UserData == null || FormsAuth.UserData.UserName == null)
            {
                return Redirect("~/Login/Index");
            }

            var loginer = FormsAuth.UserData;
            ViewBag.Title = "汽车4S店管理系统";
            ViewBag.UserName = loginer.UserName;
            ViewBag.UserCode = loginer.UserCode;
            ViewBag.Settings = new sys_userService().GetCurrentUserSettings();

            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        //public void Download()
        //{
        //    Exporter.Instance().Download();
        //}
    }
}
