using System.Web.Mvc;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class PartsController : BaseController
    {
        // GET: Sys/Parts
        public ActionResult LookupParts()
        {
            return View();
        }
    }
}