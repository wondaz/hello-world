using System;
using System.Text;
using Frame.Core;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class MenuService : ServiceBase<sys_menu>
    {
        protected override bool OnBeforEditDetail(EditEventArgs arg)
        {
            var menuCode = arg.row["_id"].ToString();

            if (arg.type == OptType.Del)
            {
                //删除角色菜单Map
                db.Sql(string.Format(@"delete sys_roleMenuMap where menucode='{0}'", menuCode)).Execute();
                //删除菜单按钮Map
                db.Sql(string.Format(@"delete sys_menuBtnMap where menucode='{0}'", menuCode)).Execute();
            }

            return base.OnBeforEditDetail(arg);
        }

        /// <summary>
        /// 一级管理员可以看到的菜单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public dynamic GetAdminMenusAndButtons(RequestWrapper request)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT V.MenuName AS \"MenuName\",V.MenuCode AS \"MenuCode\",V.ParentCode AS \"ParentCode\",");
            sb.AppendLine(string.Format("(SELECT 1 FROM sys_roleMenuMap WHERE CorpID={0} AND RoleCode= '{1}' AND MenuCode=V.MenuCode) as \"menuchecked\"", FormsAuth.UserData.CorpID, request["roleCode"]));

            var buttons = db.Sql(@"SELECT B.ButtonCode,B.ButtonName,B.Description FROM sys_button B").QueryMany<sys_button>();
            foreach (var button in buttons)
            {
                sb.AppendFormat(",(SELECT CASE WHEN max(A.ID) IS NULL THEN -1 WHEN MAX(B.ID) IS NULL THEN 0 ELSE 1 END FROM  sys_menuBtnMap A LEFT JOIN sys_roleMenuBtnMap B ON B.MenuCode=A.MenuCode AND B.ButtonCode=A.ButtonCode AND B.RoleCode='{1}' AND B.CorpID={2} WHERE A.MenuCode = V.MenuCode and  A.ButtonCode = '{0}') AS \"btn_{0}\" ", button.ButtonCode, request["roleCode"], FormsAuth.UserData.CorpID);
                sb.AppendLine();
            }
            
            sb.Append(@"FROM SYS_MENU V WHERE V.IsVisible=1 ORDER BY V.ParentCode, V.MenuCode");
            var result = db.Sql(sb.ToString()).QueryMany<dynamic>();
            return new { menus = result, buttons };
        }

        /// <summary>
        /// 2、3、4级别的管理员可以看到的菜单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public dynamic GetUserMenusAndButtons(RequestWrapper request)
        {
            var menuFilter = string.Empty;
            var corpParentID = db.Sql("SELECT ParentID FROM view_allcorps WHERE CorpID=" + FormsAuth.UserData.CorpID).QuerySingle<int>();

            // todo 只有管理员才能有创建下级企业和用户的权限
            //var IsSystemRole = db.Sql(string.Format("SELECT IsSystemRole FROM sys_role WHERE CorpID={0} AND RoleCode='{1}'", request["corpID"], request["roleCode"])).QuerySingle<int>();

            //if (IsSystemRole != 1)
            //{
            //    menuFilter = " AND MenuCode NOT IN (1606,1607)";
            //}
            var buttons = db.Sql(string.Format(@"SELECT B.ButtonCode,B.ButtonName,B.Description FROM sys_roleMenuBtnMap A INNER JOIN sys_button B ON A.ButtonCode=B.ButtonCode WHERE A.RoleCode='XJQX' AND A.CorpID={0}", corpParentID)).QueryMany<sys_button>();

            var sb = new StringBuilder();
            sb.AppendLine(string.Format("WITH CTE AS (SELECT vsf.MenuName AS \"MenuName\",vsf.MenuCode AS \"MenuCode\",vsf.ParentCode AS \"ParentCode\" FROM SYS_MENU vsf WHERE MenuCode IN (SELECT MenuCode FROM sys_roleMenuMap WHERE CorpID={0} AND RoleCode = 'XJQX') {1} )", corpParentID, menuFilter));
            sb.AppendLine(string.Format("SELECT V.* ,(SELECT 1 FROM sys_roleMenuMap WHERE CorpID={0} AND RoleCode= '{1}' AND MenuCode=V.MenuCode) as \"menuchecked\"", FormsAuth.UserData.CorpID, request["roleCode"]));
            foreach (var button in buttons)
            {
                sb.AppendFormat(",(SELECT CASE WHEN max(A.ID) IS NULL THEN -1 WHEN MAX(B.ID) IS NULL THEN 0 ELSE 1 END FROM  sys_menuBtnMap A LEFT JOIN sys_roleMenuBtnMap B ON B.MenuCode=A.MenuCode AND B.ButtonCode=A.ButtonCode AND B.RoleCode='{1}' AND B.CorpID={2} WHERE A.MenuCode = V.MenuCode and  A.ButtonCode = '{0}') AS \"btn_{0}\" ", button.ButtonCode, request["roleCode"], FormsAuth.UserData.CorpID);
                sb.AppendLine();
            }

            sb.Append(@"FROM CTE V ORDER BY V.ParentCode, V.MenuCode");
            var result = db.Sql(sb.ToString()).QueryMany<dynamic>();
            return new { menus = result, buttons };
        }

        public dynamic GetUserMenu()
        {
            //角色的菜单
            var sql = string.Format(@"
select distinct B.MenuCode,b.ParentCode,b.MenuName,b.URL,B.IconClass,
b.MenuSeq,b.Description,B.IsVisible,b.IsEnable,b.UpdatePerson,b.UpdateDate
from sys_roleMenuMap A
inner join sys_menu  B on B.MenuCode = A.MenuCode
where B.IsEnable=1
  and A.RoleCode in (
  select RoleCode from sys_userrolemap where userCode = '{0}'
) order by B.MenuSeq,B.MenuCode", FormsAuth.UserData.UserCode);
            var result = db.Sql(sql).QueryMany<sys_menu>();
            return result;
        }

        public dynamic GetEnabledMenusAndButtons(string RoleCode)
        {
            var buttons = db.Sql("select * from sys_button order by ButtonSeq").QueryMany<sys_button>();
            var sql = "select A.MenuName,A.MenuCode,A.ParentCode,A.IconClass as iconCls,A.Description,B.MenuName as ParentName";
            sql += String.Format(",(select 1 from sys_roleMenuMap tb_role where tb_role.RoleCode='{0}' and tb_role.MenuCode=A.MenuCode) as menuchecked", RoleCode);
            foreach (var button in buttons)
                sql += String.Format(@",(
select case when max(tb1_{0}.ID) is null then -1 
            when max(tb2_{0}.ID) is null then 0 
            else 1 end 
from  sys_menuBtnMap AS tb1_{0}
left join sys_roleMenuButtonMap AS tb2_{0} ON tb2_{0}.MenuCode=tb1_{0}.MenuCode AND tb2_{0}.ButtonCode=tb1_{0}.ButtonCode AND tb2_{0}.RoleCode='{1}'
where tb1_{0}.MenuCode = A.MenuCode and  tb1_{0}.ButtonCode = '{0}'  
)as 'btn_{0}' ", button.ButtonCode, RoleCode);

            sql += @"
from sys_menu as A
left join sys_menu B on B.MenuCode = A.ParentCode
where A.IsEnable = 1
order by A.MenuSeq,A.MenuCode";

            var result = db.Sql(sql).QueryMany<dynamic>();
            return new { menus = result, buttons = buttons };
        }

        public dynamic GetMenuButtons(string MenuCode)
        {
            var sql = String.Format(@"
select A.ButtonCode,A.ButtonName,A.ButtonSeq,A.ButtonIcon,
       case when B.ID is null then 0 else 1 end as Selected
from sys_button A
left join sys_menuBtnMap B on B.MenuCode = '{0}' and B.ButtonCode = A.ButtonCode
order by ButtonSeq", MenuCode);
            var result = db.Sql(sql).QueryMany<sys_button>();
            return result;
        }
        public void SaveMenuButtons(string menuCode, JToken buttonList)
        {
            db.UseTransaction(true);
            Logger("设置菜单按钮", () =>
            {
                db.Delete("sys_menuBtnMap").Where("MenuCode", menuCode).Execute();
                foreach (JToken item in buttonList.Children())
                {
                    db.Insert("sys_menuBtnMap").Column("MenuCode", menuCode).Column("ButtonCode", item["ButtonCode"]).Execute();
                }
                db.Commit();
            }, e => db.Rollback());
        }

    }

    public class sys_menu : ModelBase
    {
        [PrimaryKey]
        public string MenuCode
        {
            get;
            set;
        }

        public string ParentCode
        {
            get;
            set;
        }
        public string ParentName
        {
            get;
            set;
        }
        public string MenuName
        {
            get;
            set;
        }

        public string URL
        {
            get;
            set;
        }

        public string IconClass
        {
            get;
            set;
        }

        public string IconURL
        {
            get;
            set;
        }

        public string MenuSeq
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public int IsVisible
        {
            get;
            set;
        }

        public int IsEnable
        {
            get;
            set;
        }


        public string UpdatePerson
        {
            get;
            set;
        }

        public DateTime? UpdateDate
        {
            get;
            set;
        }
    }
}
