using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class UserController : BaseController
    {
        public ActionResult UserList()
        {
            var model = new
            {
                corpID = FormsAuth.UserData.CorpID
            };
            return View(model);
        }
    }

    public class UserApiController : BaseApiController
    {
        public dynamic Get(RequestWrapper request)
        {
            var settings = @"<settings  defaultOrderBy='UserCode'>
            <select>UserCode,UserName,CorpID,department,mobileTel,IsEnable,UpdateDate</select>
            <from>sys_user WHERE IsAdmin=0 AND CorpID={0} {1}</from>
            </settings>";

            var roleCode = string.IsNullOrEmpty(request["RoleCode"])
                ? ""
                : string.Format(
                    "AND userCode IN (SELECT UserCode FROM sys_userRoleMap WHERE RoleCode = '{0}')",
                    request["RoleCode"]);
            request.LoadSettingXmlString(string.Format(settings,FormsAuth.UserData.CorpID, roleCode));
            return new sys_userService().GetModelListWithPaging(request.ToParamQuery());
        }

        /// <summary>
        /// 根据usercode获取角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetUserRole(string id)
        {
            return new sys_userService().GetUserRole(id);
        }

        /// <summary>
        /// 获取部门列表
        /// </summary>
        /// <returns></returns>
        //public dynamic GetDept()
        //{
        //    return new sys_userService().GetDept();
        //}

        [System.Web.Http.HttpPost]
        public int PostResetPassword(string id)
        {
            var service = new sys_userService();
            return service.ResetUserPassword(id);
        }

        [System.Web.Http.HttpPost]
        public string Edit(dynamic data)
        {
            return new sys_userService().SaveUser(data);
        }


        [System.Web.Http.HttpPost]
        public void EditUserRoles(string id, dynamic data)
        {
            var service = new sys_userService();
            service.SaveUserRoles(id, data as JToken);
        }

        public dynamic GetRoleMembersDict(string id)
        {
            var service = new sys_userService();
            return service.GetRoleMembersDict(id);
        }

        [System.Web.Http.HttpPost]
        public bool EditPassword(dynamic data)
        {
            var service = new sys_userService();
            var result = service.EditPassword(data);
            return result;
        }
    }
}
