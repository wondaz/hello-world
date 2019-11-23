using System.Web.Http;
using System.Web.Mvc;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    [MvcMenuFilter(false)]
    public class HomeController : Controller
    {
        // GET: /Dashboard/Home/
        [System.Web.Mvc.AllowAnonymous]
        public ActionResult DashboardView()
        {
            return View();
        }

        //public ActionResult LookupMaterial()
        //{
        //    return View();
        //}
    }

}
