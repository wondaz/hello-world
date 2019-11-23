using System;
using System.Collections.Generic;
using System.Linq;
using Frame.Core;
using Frame.Utils;
using Newtonsoft.Json.Linq;
using System.Web.Security;
using FluentData;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_userService : ServiceBase<sys_user>
    {
        public dynamic Login(JObject request)
        {
            var userCode = request.Value<string>("usercode");
            var password = request.Value<string>("password");

            //用户名密码检查
            if (string.IsNullOrEmpty(userCode.Trim()) || string.IsNullOrEmpty(password))
                return new { status = "error", message = "用户名或密码不能为空！", ticket = "" };

            //用户名密码验证
            var userInfo = db.Sql(string.Format(
@"SELECT a.UserID, a.UserCode, a.UserName, a.Password, a.IsAdmin, a.CorpID, b.CorpName, b.CorpShortName, b.UnionID, 
                b.CorpAddress, b.CorpLevel, b.ParentID, b.CorpCode, b.CorpPhone, a.DeptID, c.DeptCode, c.DeptName
FROM dbo.sys_user AS a INNER JOIN dbo.base_corp AS b ON a.CorpID = b.CorpID
				left join base_dept c on a.DeptID=c.DeptID WHERE   (a.IsEnable = 1) AND (b.Enable = 1)
 AND (a.UserCode = '{0}') AND (a.Password = '{1}')", userCode, password)).QuerySingle<LoginUser>();
            if (userInfo == null || string.IsNullOrEmpty(userInfo.UserCode))
            {
                return new { status = "error", message = "用户名或密码不正确！", ticket = "" };
            }

            var effectiveHours = ZConfig.GetConfigInt("LoginEffectiveHours");
            FormsAuth.SignIn(userInfo.UserCode, userInfo, 60 * effectiveHours);

            //添加登陆日志
            LoginHistory(request);
            var ticketStr = GetTicket(userCode, password);
            //返回登陆成功
            return new { status = "success", message = "登陆成功！", ticket = ticketStr };
        }

        private string GetTicket(string strUser, string strPwd)
        {
            try
            {
                var ticket = new FormsAuthenticationTicket(0, strUser, DateTime.Now,
                    DateTime.Now.AddHours(4), true, string.Format("{0}#{1}", strUser, strPwd), FormsAuthentication.FormsCookiePath);
                return FormsAuthentication.Encrypt(ticket);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private void LoginHistory(JObject request)
        {
            //如果是内网就获取，否则出错获取不到，且影响效率
            var lanIP = ZHttp.ClientIP;
            var hostName = ZHttp.IsLanIP(lanIP) ? ZHttp.ClientHostName : string.Empty;
            var userCode = request.Value<string>("usercode");
            var userName = FormsAuth.UserData.UserName;
            var IP = request.Value<string>("ip");
            var city = request.Value<string>("city");
            if (IP != lanIP)
                IP = string.Format("{0}/{1}", IP, lanIP).Trim('/').Replace("::1", "localhost");

            var item = new sys_loginHistory
            {
                UserCode = userCode,
                UserName = userName,
                HostName = hostName,
                HostIP = IP,
                LoginCity = city,
                LoginDate = DateTime.Now
            };
            db.Insert("sys_loginHistory", item).AutoMap().Execute();
        }

        public Dictionary<string, object> GetCurrentUserSettings()
        {
            var userCode = FormsAuth.UserData.UserCode;
            var settings = db.Sql(string.Format("SELECT * FROM sys_userSetting WHERE UserCode='{0}'", userCode)).QueryMany<sys_userSetting>();

            var result = settings.ToDictionary<sys_userSetting, string, object>(item => item.SettingCode, item => item.SettingValue);

            var defaults = GetDefaultUserSetttins();

            foreach (var item in defaults)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
        }

        public Dictionary<string, object> GetDefaultUserSetttins()
        {
            var defaults = new Dictionary<string, object>();
            defaults.Add("theme", "default");
            defaults.Add("navigation", "accordion");
            defaults.Add("gridrows", "20");
            return defaults;
        }

        public List<dynamic> GetRoleMembers(string role)
        {
            var result = db.Sql(string.Format(@"
 select A1.UserName as MemberName ,A1.UserCode as MemberCode,'user' as MemberType
  from sys_user A1
 where A1.UserCode in (select B1.UserCode from sys_userRoleMap B1 where B1.RoleCode = '{0}')", role)).QueryMany<dynamic>();
            return result;
        }

        public List<textValue> GetRoleMembersDict(string roleCode)
        {
            var result = db.Sql(string.Format(@"
 SELECT A1.UserCode as value,A1.UserName as text 
  FROM sys_user A1 WHERE A1.IsEnable=1 AND A1.CorpID={0} AND 
   A1.UserCode in (SELECT B1.UserCode FROM sys_userRoleMap B1 WHERE B1.RoleCode = '{1}')", FormsAuth.UserData.CorpID, roleCode)).QueryMany<textValue>();
            return result;
        }

        public dynamic GetUserRole(string userCode)
        {
            var sql = string.Format(@"select  A.RoleCode,A.RoleName
,(case when B.RoleCode is null then 'false' else 'true' end) as Checked,B.ID,B.UserCode
from sys_role A  left join sys_userRoleMap B on B.RoleCode = A.RoleCode AND B.UserCode = '{0}'
WHERE (A.IsSystemRole=1 OR A.CorpID={1})", userCode, FormsAuth.UserData.CorpID);
            return db.Sql(sql).QueryMany<UserRole>();
        }

        public void SaveUserRoles(string UserCode, JToken RoleList)
        {
            db.UseTransaction(true);
            Logger("设置用户角色", () =>
            {
                db.Delete("sys_userRoleMap").Where("UserCode", UserCode).Execute();
                foreach (JToken item in RoleList.Children())
                {
                    var RoleCode = item["RoleCode"].ToString();
                    db.Insert("sys_userRoleMap").Column("UserCode", UserCode).Column("RoleCode", RoleCode).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

        public int ResetUserPassword(string userCode)
        {
            var service = new ServiceBase(APP.DB_NMPS_CONN_NAME);
            var result = service.db.Update("T_SYS_Operator")
                .Column("Password", "96E79218965EB72C92A549DD5A330112")
                .Where("UserName", userCode)
                .Execute();
            return result;
        }

        protected override void OnAfterEditDetail(EditEventArgs arg)
        {
            if (arg.type == OptType.Add)
            {
                ResetUserPassword(arg.row["UserCode"].ToString());
            }
            if (arg.type == OptType.Del)
            {
                var sql = string.Format("DELETE FROM sys_userRoleMap WHERE UserCode='{0}'", arg.row["UserCode"]);
                db.Sql(sql).Execute();
            }

            base.OnAfterEditDetail(arg);
        }

        public bool AuthorizeUserMenu(List<string> urls)
        {
            var userCode = FormsAuth.UserData.UserCode;
            var result = db.Sql(string.Format(@"
SELECT 1
from SYS_roleMenuMap A
left join sys_userrolemap B on B.RoleCode = A.RoleCode
left join sys_menu C on C.MenuCode = A.MenuCode
where B.UserCode = '{1}'
and C.URL IN ('{0}')", string.Join("','", urls), userCode)).QueryMany<int>();

            return result.Count > 0;
        }


        public List<textValue> GetDept()
        {
            var result = db.Sql("select DeptID as value,DeptName as text from base_dept WHERE DELFLAG=0").QueryMany<textValue>();
            return result;
        }

        public bool EditPassword(JObject jdata)
        {
            if (jdata == null) return false;

            var userCode = FormsAuth.UserData.UserCode;
            var oldPsd = jdata["oldPsd"].ToString();
            var newPsd = jdata["newPsd"].ToString();

            var isRight = ValidatePassword(userCode, oldPsd);
            if (!isRight) return false;

            var result1 = db.Update("sys_user").Column("Password", newPsd).Where("UserCode", userCode).Execute();
            var result2 = db.Update("T_BASE_SUPPLIERINFO").Column("PASSWD", newPsd).Where("SUPPLIERCODE", userCode).Execute();
            return result1 + result2 > 0;
        }

        private bool ValidatePassword(string userCode, string psd)
        {
            if (string.IsNullOrEmpty(psd)) return false;

            var result = db.Sql(string.Format("SELECT COUNT(1) FROM sys_user WHERE UserCode='{0}' AND Password='{1}'", userCode, psd)).QuerySingle<int>();
            return result > 0;
        }

        public void SaveCurrentUserSettings(JObject settings)
        {
            var UserCode = FormsAuth.UserData.UserCode;
            foreach (JProperty item in settings.Children())
            {
                var result = db.Update("sys_userSetting")
                    .Column("SettingValue", item.Value.ToString())
                    .Where("UserCode", UserCode)
                    .Where("SettingCode", item.Name)
                    .Execute();

                if (result <= 0)
                {
                    var model = new sys_userSetting();
                    model.UserCode = UserCode;
                    model.SettingCode = item.Name;
                    model.SettingValue = item.Value.ToString();
                    db.Insert<sys_userSetting>("sys_userSetting", model).AutoMap(x => x.ID).Execute();
                }
            }
        }

        public string SaveUser(JObject data)
        {
            var dbContext = new ServiceBase(APP.DB_NMPS_CONN_NAME).db.UseTransaction(true);
            string result = "ok";
            var userInfo = FormsAuth.UserData;
            Logger("编辑用户", () =>
            {
                foreach (JProperty item in data["list"].Children())
                {
                    foreach (var row in item.Value.Children())
                    {
                        // item.Name
                        var builder = dbContext.StoredProcedure("SP_Operator_Edit")
                         .Parameter("UserCode", row["UserCode"].ToString())
                         .Parameter("UserName", row["UserName"].ToString())
                         .Parameter("Department", row["department"].ToString())
                         .Parameter("MobileTel", row["mobileTel"].ToString())
                         .Parameter("IsEnable", row["IsEnable"].ToString())
                         //.Parameter("UpdatePerson", userInfo.UserName)
                         .Parameter("CorpID", userInfo.CorpID)
                         .Parameter("UnionID", userInfo.UnionID)                         
                         //.Parameter("CollaborationID", userInfo.CollaborationID)
                         .Parameter("IsAdmin", 0)
                         .Parameter("OptType", item.Name)
                         .Parameter("B_CORPNAME","")
                         .Parameter("B_CORPSHORT","")
                         .Parameter("B_ADDRESS","")
                         .Parameter("B_LINKMAN","")
                         .Parameter("B_LINKTEL","")
                         .Parameter("B_LINKMOBILE","")
                         .Parameter("B_FAX","")
                         .Parameter("B_UPDATENAME","")
                         .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");

                        if (result.ToLower() != "ok") break;
                    }

                    if (result.ToLower() != "ok") break;
                }

                if (result == "ok")
                {
                    dbContext.Commit();
                }
                else
                {
                    dbContext.Rollback();
                }

            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });

            return result;
        }
    }

    public class sys_user : ModelBase
    {

        [PrimaryKey]
        public string UserCode { get; set; }

        public int CorpID { get; set; }

        public string UserName { get; set; }

        public string department { get; set; }

        public string mobileTel { get; set; }

        public string Password { get; set; }

        public int IsEnable { get; set; }

        public string UpdatePerson { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    public class UserRole : ModelBase
    {
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public string Checked { get; set; }
        public int ID { get; set; }
        public string UserCode { get; set; }

    }
}
