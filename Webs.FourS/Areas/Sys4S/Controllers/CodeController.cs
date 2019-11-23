using System.Collections.Generic;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class CodeController : BaseController
    {
        // GET: Sys/Code
        public ActionResult Index()
        {
            var model = new
            {
                corpid = FormsAuth.UserData.CorpID
            };
            return View(model);
        }
    }

    public class CodeApiController : BaseApiController
    {
        public dynamic GetCodeType(RequestWrapper request)
        {
            return new CodeTypeService().GetCodeType();
        }

        public dynamic GetList(RequestWrapper request)
        {
            var result = new CodeTypeService().GetDataDict(request["CodeType"]);
            return result;
        }

        public dynamic GetNewCode()
        {
            var service = new CodeService();
            return service.GetNewKey("ID", "maxplus");
        }

        [System.Web.Http.HttpPost]
        public void Edit(dynamic data)
        {
            var listWrapper = RequestWrapper.Instance().LoadSettingXmlString(string.Format(@"
<settings>
    <table>sys_code</table>
    <where>
        <field name='ID' cp='equal'></field>
        <field name='CorpID' cp='equal' value='{0}'></field>
    </where>
</settings>",FormsAuth.UserData.CorpID));
            var service = new CodeService();
            var result = service.Edit(null, listWrapper, data);
        }

        [System.Web.Http.HttpPost]
        public void EditCodeType(dynamic data)
        {
            var listWrapper = RequestWrapper.Instance().LoadSettingXmlString(string.Format(@"
<settings>
    <table>sys_codeType</table>
    <where>
        <field name='CodeType' cp='equal'></field>
        <field name='CorpID'   cp='equal' value='{0}'></field>
    </where>
</settings>",FormsAuth.UserData.CorpID));
            var service = new CodeTypeService();
            var result = service.Edit(null, listWrapper, data);
        }

        /// <summary>
        /// 获取绑定下拉框的数据字典
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<textValue> GetDictByType(string id)
        {
            return new CodeTypeService().GetDictItemsByType(id);
        }
    }
}