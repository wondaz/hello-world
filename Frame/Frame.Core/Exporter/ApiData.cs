using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using Frame.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frame.Core
{
    public class ApiData : IDataGetter
    {
        public object GetData(HttpContext context)
        {
            var url = context.Request.Form["dataUrl"];
            var paramDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(context.Request.Form["dataParams"]);
            string paramStr = paramDict.Aggregate("", (current, item) => current + string.Format("{0}={1}&", item.Key, item.Value));

            string dataStr = ZHttp.GetHttpData(url, paramStr.TrimEnd('&'));
            var data = JsonConvert.DeserializeObject<JObject>(dataStr);

            var result = JsonConvert.DeserializeObject<List<IDictionary<string,string>>>(data["rows"].ToString());
            return result;
        }
    }
}
