using System;
using System.Collections.Generic;
using Frame.Core;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_roleService : ServiceBase<sys_role>
    {
        public List<sys_role> GetRoleList()
        {
            return db.Sql(string.Format(@"SELECT RoleID,RoleCode,CorpID,RoleName,Description,IsSystemRole,UpdateDate,UpdatePerson,
                CASE WHEN IsSystemRole=1 THEN '系统角色' ELSE '自定义角色' END RoleType
            FROM sys_role WHERE IsSystemRole=1 OR CorpID={0}", FormsAuth.UserData.CorpID)).QueryMany<sys_role>();
        }

        protected override void OnAfterEditDetail(EditEventArgs arg)
        {
            if (arg.type == OptType.Del)
            {
                string sql = string.Format("DELETE FROM sys_roleMenuMap WHERE CorpID={0} AND RoleCode='{1}'",
                    FormsAuth.UserData.CorpID, arg.row["RoleCode"]);
                db.Sql(sql).Execute();

                string sql2 = string.Format("DELETE FROM sys_roleMenuBtnMap WHERE CorpID={0} AND RoleCode='{1}'",
                    FormsAuth.UserData.CorpID, arg.row["RoleCode"]);
                db.Sql(sql2).Execute();
            }
            if (arg.type == OptType.Add)
            {
                string sql = "UPDATE sys_role SET RoleCode=RoleID WHERE RoleCode IS NULL OR RoleCode=''";
                db.Sql(sql).Execute();
            }

            base.OnAfterEditDetail(arg);
        }

        public void EditRoleMenu(string roleid, JToken menuList)
        {
            db.UseTransaction(true);
            Logger("修改角色菜单权限", () =>
            {
                if (APP.DbProvider == DbProviderEnum.SqlServer)
                {
                    db.Sql(string.Format("DELETE A FROM sys_roleMenuMap A INNER JOIN  sys_role B ON A.RoleCode=B.RoleCode AND A.CorpID=B.CorpID WHERE B.RoleID={0}", roleid)).Execute();
                }
                else//oracle
                {
                    db.Sql(string.Format(@"DELETE FROM SYS_ROLEMENUMAP A WHERE EXISTS (SELECT 1 FROM SYS_ROLE B WHERE B.ROLECODE = A.ROLECODE AND B.CORPID = A.CORPID AND B.ROLEID = {0})", roleid)).Execute();
                }

                foreach (JToken item in menuList.Children())
                {
                    var sql = string.Format(@"INSERT INTO sys_roleMenuMap(RoleCode,MenuCode,CorpID) VALUES('{0}','{1}',{2})", item["RoleCode"], item["MenuCode"], FormsAuth.UserData.CorpID);
                    db.Sql(sql).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

        public void EditRoleMenuButton(string roleid, JToken menuButtonList)
        {
            db.UseTransaction(true);
            Logger("修改角色菜单按钮权限", () =>
            {
                if (APP.DbProvider == DbProviderEnum.SqlServer)
                {
                    db.Sql(string.Format("DELETE A FROM sys_roleMenuBtnMap A INNER JOIN  sys_role B ON A.RoleCode=B.RoleCode AND A.CorpID=B.CorpID WHERE B.RoleID={0}", roleid)).Execute();
                }
                else
                {
                    db.Sql(string.Format("DELETE FROM sys_roleMenuBtnMap A WHERE EXISTS(SELECT 1 FROM sys_role B WHERE B.RoleCode=A.RoleCode AND B.CorpID=A.CorpID AND B.RoleID={0})", roleid)).Execute();
                }
                
                foreach (JToken item in menuButtonList.Children())
                {
                    var sql = string.Format(@"INSERT INTO sys_roleMenuBtnMap(RoleCode,MenuCode,CorpID,ButtonCode) VALUES('{0}','{1}',{2},'{3}')", item["RoleCode"], item["MenuCode"], FormsAuth.UserData.CorpID, item["ButtonCode"]);
                    db.Sql(sql).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

        public void SaveRoleMembers(string roleCode, JToken memberList)
        {
            db.UseTransaction(true);
            Logger("设置角色成员", () =>
            {
                db.Delete("sys_userRoleMap").Where("RoleCode", roleCode).Execute();
                db.Delete("sys_organizeRoleMap").Where("RoleCode", roleCode).Execute();
                foreach (JToken item in memberList.Children())
                {
                    var memberCode = item["MemberCode"].ToString();
                    var memberType = item["MemberType"].ToString();
                    if (memberType.ToLower() == "user")
                    {
                        db.Insert("sys_userRoleMap")
                            .Column("RoleCode", roleCode)
                            .Column("UserCode", memberCode)
                            .Execute();
                    }
                    else
                    {
                        db.Insert("sys_organizeRoleMap").Column("RoleCode", roleCode).Column("OrganizeCode", memberCode).Execute();
                    }
                }
                db.Commit();
            }, e => db.Rollback());
        }
    }

    public class sys_role : ModelBase
    {
        //RoleID,RoleCode,CorpID,RoleName,Description,IsSystemRole
        [PrimaryKey]
        public int RoleID { get; set; }
        public string RoleCode { get; set; }
        public int CorpID { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public int IsSystemRole { get; set; }
        public string UpdatePerson { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string RoleType { get; set; }
    }
}
