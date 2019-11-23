using System.Web.Mvc;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using System.Web;

namespace Web.FourS.Controllers
{
    [AllowAnonymous]
    [MvcMenuFilter(false)]
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult DoLogin(JObject request)
        {
            var message = new sys_userService().Login(request);
            SetTicket(message.ticket);
            return Json(message, JsonRequestBehavior.DenyGet);
        }

        private void SetTicket(string ticket)
        {
            try
            {
                var cookie = new HttpCookie("hbticket", ticket);
                var context = System.Web.HttpContext.Current;
                context.Response.Cookies.Remove(cookie.Name);
                context.Response.Cookies.Add(cookie);
            }
            catch
            {
            }

        }

        public ActionResult Logout()
        {
            FormsAuth.SingOut();
            return Redirect("~/Login");
        }
    }
}
