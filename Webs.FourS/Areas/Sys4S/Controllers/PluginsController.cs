using System.Web.Mvc;
using Frame.Core;
using Newtonsoft.Json;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    [AllowAnonymous]
    [MvcMenuFilter(false)]
    public class PluginsController : BaseController
    {
        // GET: Plugins
        public ActionResult Lookup()
        {
            return View();
        }

        public ActionResult GetLookupData()
        {
            var type = Request.QueryString["_lookupType"];
            if (string.IsNullOrEmpty(type)) return new ContentResult();

            var xmlPath = "~/Areas/sys4s/Xml/plugin.xml";
            var request = RequestWrapper.InstanceFromRequest().LoadXmlByUrl(xmlPath, type);

            var query = request.ToParamQuery();
            if (!string.IsNullOrEmpty(request["_valueFeild"]))
            {
                var value = request[request["_valueFeild"]];
                var field = query.GetData().Where[0].Data.Column;
                query.ClearWhere().AndWhere(field, value, Cp.Equal);
            }

            var data = request.GetService().GetDynamicListWithPaging(query);
            var json = JsonConvert.SerializeObject(data);
            return Content(json, "application/json");
        }
    }
}