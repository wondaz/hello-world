using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Web.FourS;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class ConfigController : Controller
    {
        // GET: /Sys/Config/
        [MvcMenuFilter(false)]
        public ActionResult Index()
        {
            var themes = new List<dynamic>
            {
                new {text = "默认", value = "default"},
                new {text = "灰色", value = "gray"},
                new {text = "黑色", value = "black"},
                new {text = "material", value = "material"},
                new {text = "metro", value = "metro"},
                new {text = "bootstrap", value = "bootstrap"},
            };

            var navigations = new List<dynamic>
            {
                new {text = "横向菜单", value = "menubutton"},
                new {text = "手风琴", value = "accordion"},
                new {text = "树形结构", value = "tree"}
            };
            var popupWins = new List<dynamic>()
            {
                 new {text = "否", value = "0"},
                new {text = "是", value = "1"},
            };

            var model = new
            {
                dataSource = new
                {
                    themes,
                    navigations,
                    popupWins
                },
                form = new sys_userService().GetCurrentUserSettings()
            };

            return View(model);
        }
    }

    public class ConfigApiController : ApiController
    {
        // POST api/sys/config
        public void Post([FromBody]JObject value)
        {
            if (value == null)
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.NotAcceptable) { Content = new StringContent("配置不可为空") });

            var service = new sys_userService();
            service.SaveCurrentUserSettings(value);
        }
    }
}
