using System.Collections.Generic;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;
using Frame.Core;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class RoleController : BaseController
    {
        public ActionResult RoleList()
        {
            var model = new
            {
                corpID = FormsAuth.UserData.CorpID,
                collaborationID = 100,
                unionID = FormsAuth.UserData.UnionID,
                corpLevel = FormsAuth.UserData.CorpLevel
            };

            return View(model);
        }
    }

    public class RoleApiController : BaseApiController
    {
        public dynamic GetRoleMenu(string id)
        {
            var service = new sys_roleService();
            var pQuery = ParamQuery.Instance().Select(string.Format(@" A.MenuCode,
        (case when exists(select 1 from sys_roleMenuMap B where B.MenuCode = A.MenuCode and B.RoleCode = '{0}')
              then '1' 
              else '0' end) as chk
            ", id)).From("sys_menu A")
             .AndWhere("A.IsEnable", 1);

            var result = service.GetDynamicList(pQuery);
            return result;
        }

        public dynamic GetRoleMembers(string id)
        {
            var result = new sys_userService().GetRoleMembers(id);
            return result;
        }

        public IEnumerable<sys_role> GetRoleList()
        {
            var result = new sys_roleService().GetRoleList();
            return result;
        }

        [System.Web.Http.HttpPost]
        public void Edit(dynamic data)
        {
            var listWrapper = RequestWrapper.Instance().LoadSettingXmlString(@"
<settings>
    <table>sys_role</table>   
    <columns ignore='RoleType'></columns>
    <where>
        <field name='RoleID' cp='equal'></field>
    </where>
</settings>");

            var service = new sys_roleService();
            var result = service.Edit(null, listWrapper, data);
        }

        [System.Web.Http.HttpPost]
        public void EditPermission(string id, dynamic data)
        {
            var service = new sys_roleService();
            service.EditRoleMenu(id, data.menus as JToken);
            service.EditRoleMenuButton(id, data.buttons as JToken);
        }

        [System.Web.Http.HttpPost]
        public void EditRoleMembers(string id, dynamic data)
        {
            var service = new sys_roleService();
            service.SaveRoleMembers(id, data as JToken);
        }
    }
}
