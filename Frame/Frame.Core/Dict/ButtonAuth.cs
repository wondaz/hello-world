using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Frame.Core.Dict
{
    public static class ButtonAuth
    {
        public static MvcHtmlString GetButtonHtmlString(string userInfo)
        {
            var buttons = GetUserMenuButtons(userInfo);
            if (buttons == null || !buttons.Any())
            {
                return new MvcHtmlString("");
            }

            //var buttons = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SysButton>>(buttonStr);
            var toolbar = new TagBuilder("div");
            toolbar.AddCssClass("z-toolbar");
            foreach (var btn in buttons)
            {
                var link = new TagBuilder("a");
                link.MergeAttribute("href", "#");

                if (btn.ButtonCode.Equals("download"))
                {
                    link.MergeAttribute("class", "easyui-splitbutton");
                    link.MergeAttribute("data-options", "menu:'#dropdown',iconCls:'icon-download'");
                }
                else
                {
                    link.MergeAttribute("plain", "true");
                    link.MergeAttribute("class", "easyui-linkbutton");
                    link.MergeAttribute("icon", btn.ButtonIcon);
                    link.MergeAttribute("title", btn.ButtonName);
                    link.MergeAttribute("data-bind", "click:" + btn.ButtonCode + "Click");
                }

                link.SetInnerText(btn.ButtonName);
                toolbar.InnerHtml += link.ToString();
            }

            return new MvcHtmlString(toolbar.ToString());
        }

        public static List<dynamic> GetUserMenuButtons(string userInfo, string pageUrl = "")
        {
            if (string.IsNullOrEmpty(pageUrl))
            {
                pageUrl = HttpContext.Current.Request.CurrentExecutionFilePath.TrimStart('/');
            }

            var userData = userInfo.Split('-');
            using (var db = Db.Context(APP.DB_DEFAULT_CONN_NAME))
            {
                var menuCode = db.Sql(string.Format("select functionId from t_sys_function where functionUrl = '{0}'", pageUrl)).QuerySingle<string>();

                if (userData[1] == "1" && userData[2] == "1")
                {
                    var buttonsql = string.Format(@"
SELECT A.ButtonCode,A.ButtonName,A.ButtonIcon
FROM sys_button A
INNER JOIN sys_MenuBtnMap B ON B.ButtonCode = A.ButtonCode AND B.MenuCode = '{0}'
ORDER BY A.ButtonSeq", menuCode);
                    return db.Sql(buttonsql).QueryMany<dynamic>();
                }

                var sql = string.Format(@"
SELECT A.ButtonCode,A.ButtonName,A.ButtonIcon
FROM sys_button A
INNER JOIN sys_roleMenuBtnMap B ON B.ButtonCode = A.ButtonCode AND B.MenuCode = '{0}'
WHERE B.RoleCode IN (SELECT RoleCode FROM sys_userRoleMap WHERE userCode = '{1}')
ORDER BY A.ButtonSeq", menuCode, userData[0]);
                return db.Sql(sql).QueryMany<dynamic>();
            }
        }

        //public class SysButton
        //{
        //    public string ButtonCode { get; set; }
        //    public string ButtonName { get; set; }
        //    public string ButtonIcon { get; set; }
        //}
    }
}
