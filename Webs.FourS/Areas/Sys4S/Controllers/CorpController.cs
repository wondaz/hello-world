using System.Web.Mvc;
using Frame.Core;
using Frame.Core.Dict;
using System.Collections.Generic;
using Frame.Utils;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Sys4S.Controllers
{
    public class CorpController : BaseController
    {
        /// <summary>
        /// 查询下属企业
        /// </summary>
        /// <returns></returns>
        public ActionResult SubCorps()
        {
            var model = new
            {
                pkField = "CorpID",//主键字段名
                _xml = "sys4s.corp.sublist",
                urls = new
                {
                    edit = "/sys4s/corp/corpedit/" //跳转到编辑页面                    
                },
                //resx = BIHelper.GetIndexResx("企业"),//按钮操作提示信息
                form = new //查询条件绑定字段
                {
                    CorpName = "",
                    _CorpID = FormsAuth.UserData.CorpID,
                    Enable = new CodeService().DefaultValue("IsEnabled")//下拉框默认值
                }
            };

            return View(model);
        }

        /// <summary>
        /// 企业编辑页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CorpEdit(string id)
        {
            var model = new
            {
                masterKey = "CorpID", //主表Kkey
                keyVal = id,
                urls = new
                {
                    master = "/api/sys4s/corp/GetCorpInfo/", //获取主表数据接口
                    save = "/api/sys4s/corp/EditCorp/", //保存接口
                    spellCode = "/api/sys4s/comm/GetSpellCode/"
                },

                //新增时，表单默认值
                defaultForm = new
                {
                    Enable = new CodeService().DefaultValue("IsEnable"),//下拉框默认值
                    UpdatePerson = FormsAuth.UserData.UserName,
                    ParentID = FormsAuth.UserData.CorpID,
                    CollaborationID = 0,
                    CorpLevel = FormsAuth.UserData.CorpLevel + 1,
                    FormsAuth.UserData.UnionID
                }
            };
            return View(model);
        }
    }

    public class CorpApiController : BaseApiController
    {
        /// <summary>
        /// 企业维护树形列表
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public dynamic GetCorpTree(RequestWrapper request)
        {
            //if (!APIHelper.UserAuth(request["token"]))
            //{
            //    return new { message = APIHelper.ErrorMsg };
            //}

            return new SysCorpService().GetCorpTree(request["corpid"]);
        }

        /// <summary>
        /// 企业列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public dynamic GetList(RequestWrapper request)
        {
            //if (!APIHelper.UserAuth(request["token"]))
            //{
            //    return new { message = APIHelper.ErrorMsg };
            //}

            //点击树形节点
            if (!string.IsNullOrEmpty(request["CorpID"]))
            {
                int pageIndex = ZConvert.To(request["page"], 1);
                int pageSize = ZConvert.To(request["rows"], 20);
                var sql = request.GetXmlNodeValue("sql");
                sql = string.Format(sql, request["CorpID"], (pageIndex - 1) * pageSize + 1, pageIndex * pageSize);
                var rows = request.GetService().db.Sql(sql).QueryMany<dynamic>();
                return new
                {
                    rows,
                    total = rows.Count > 0 ? rows[0].RowsCount : 0
                };
            }

            var result = request.GetService().GetDynamicListWithPaging(request.ToParamQuery());
            return result;
        }

        /// <summary>
        /// 获取企业的下属企业
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public dynamic GetSubCorps(RequestWrapper request)
        {
            //if (!APIHelper.UserAuth(request["token"]) || string.IsNullOrEmpty(request["_corpid"]))
            //{
            //    return new { message = APIHelper.ErrorMsg };
            //}

            var sql = request.GetXmlNodeValue("sql");
            var whereSql = request.ToParamQuery().GetData().WhereSql;
            if (!string.IsNullOrEmpty(whereSql))
            {
                whereSql = " AND " + whereSql;
            }

            sql = string.Format(sql, request["_corpid"], whereSql);
            var rows = request.GetService().db.Sql(sql).QueryMany<dynamic>();
            return rows;
        }

        /// <summary>
        /// 根据企业ID,获取企业详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetCorpInfo(string id)
        {
            return new SysCorpService().GetCorpInfo(id);
        }

        /// <summary>
        /// 保存企业信息
        /// </summary>
        /// <param name="data"></param>
        public int EditCorp(dynamic data)
        {
            var formWrapper = RequestWrapper.Instance().LoadSettingXmlString(@"
<settings>
  <table>base_corp</table>
  <columns ignore='CorpID,UserCode'></columns>
  <where><field name='CorpID' cp='equal'></field></where>
</settings>");

            var newCorpid = new SysCorpService().Edit(formWrapper, null, data);
            return newCorpid;
        }

        /// <summary>
        /// 获取服务站名称列表
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public List<dynamic> GetServiceSites(string q)
        {
            return new SysCorpService().GetCorpNames(q, "serviceSite");
        }
    }

}