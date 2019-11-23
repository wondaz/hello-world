using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class MenuController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }

    public class MenuApiController : BaseApiController
    {
        [System.Web.Http.AllowAnonymous]
        public IEnumerable<dynamic> Get()
        {
            //var userCode = this.User.Identity.Name;
            var result = new MenuService().GetUserMenu();
            return result;
        }

        public dynamic GetMenusAndButtons(RequestWrapper request)
        {
            return new MenuService().GetAdminMenusAndButtons(request);
            //return Convert.ToInt32(request["corpLevel"]) == 1
            //    ? new MenuService().GetAdminMenusAndButtons(request)
            //    : new MenuService().GetUserMenusAndButtons(request);
        }

        // GET api/menu
        public dynamic GetEnabled(string id)
        {
            var result = new MenuService().GetEnabledMenusAndButtons(id);
            return result;
        }

        [System.Web.Http.AllowAnonymous]
        public IEnumerable<dynamic> GetAll()
        {
            var MenuService = new MenuService();
            var pQuery = ParamQuery.Instance().Select("A.*,B.MenuName as ParentName")
                .From(@"sys_menu A left join sys_menu B on B.MenuCode = A.ParentCode")
                .AndWhere("a.isenable", "1").AndWhere("b.isenable", "1")
                .OrderBy("A.MenuSeq,A.MenuCode");
            var result = MenuService.GetList(pQuery);
            return result;
        }

        /// <summary>
        /// 地址：POST api/sys/edit
        /// 功能：保存菜单数据
        /// 调用：菜单数据页面，保存按钮
        /// </summary>
        [System.Web.Http.HttpPost]
        public void Edit(dynamic data)
        {
            var listWrapper = RequestWrapper.Instance().LoadSettingXmlString(@"
<settings>
    <table>
        sys_menu
    </table>
    <where>
        <field name='MenuCode' cp='equal' variable='_Id'></field>
    </where>
</settings>");

            var service = new MenuService();
            var result = service.Edit(null, listWrapper, data);

            service.Logger("编辑菜单", data);
        }

        public IEnumerable<dynamic> GetMenuButtons(string id)
        {
            return new MenuService().GetMenuButtons(id);
        }

        public IEnumerable<dynamic> GetButtons()
        {
            var pQuery = ParamQuery.Instance().OrderBy("ButtonSeq");
            return new sys_buttonService().GetModelList(pQuery);
        }

        [System.Web.Http.HttpPost]
        public void EditMenuButtons(string id, dynamic data)
        {
            var service = new MenuService();
            service.SaveMenuButtons(id, data as JToken);
        }

        [System.Web.Http.HttpPost]
        public void EditButton(dynamic data)
        {
            var listWrapper = RequestWrapper.Instance().LoadSettingXmlString(@"
<settings>
    <table>sys_button</table>
    <where>
        <field name='ButtonCode' cp='equal'></field>
    </where>
</settings>");
            var service = new sys_buttonService();
            var result = service.Edit(null, listWrapper, data);
        }
    }
}
