using System.Collections.Generic;
using Frame.Core;
using Frame.Utils;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class CommApiController : BaseApiController
    {
        /// <summary>
        /// 查询通用接口方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [System.Web.Http.AllowAnonymous]
        public dynamic GetList(RequestWrapper request)
        {
            //if (!CheckUserAuth(request["token"]))
            //{
            //    return new { message = "没有授权" };
            //}

            var result = request.GetService().GetDynamicListWithPaging(request.ToParamQuery());
            return result;
        }

        /// <summary>
        /// 在数据字典中，根据值获取文本
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string GetDictText(RequestWrapper request)
        {
            return new CommonService().GetDictText(request["type"], request["val"]);
        }

        /// <summary>
        /// 获取拼音码
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetSpellCode(string id)
        {
            if (id == null) return "";           
            var spellcode = ZString.GetSpellCode(id);           
            return spellcode;
        }      
    }
}