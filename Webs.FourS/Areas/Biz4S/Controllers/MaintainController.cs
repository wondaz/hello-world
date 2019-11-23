using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    /// <summary>
    /// 维修派工单
    /// </summary>
    public class MaintainController : BaseController
    {

        public ActionResult WorkOrderList()
        {
            var model = new
            {
                _xml = "biz4s.maintain.WorkOrderList",
                pkey = "DispatchID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/biz4s/maintain/WorkOrderEdit/", //跳转到编辑页面
                    delete = "/api/biz4s/maintain/PostDeleteDispatchHead",
                    audit = "/api/biz4s/maintain/PostAuditDispatch"
                },
                form = new //查询条件绑定字段
                {
                    EmpName = "",
                    DispatchCode = "",
                    SignCode = "",
                    InputTime = "",
                    BillState = "",
                    MobileTel = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    EmpNames = new sys_userService().GetRoleMembersDict("FWGW")//服务顾问
                }
            };
            return View(model);
        }

        /// <summary>
        /// 维修工单编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult WorkOrderEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    signCode = "/api/biz4s/car/GetSignCode", //车牌号自动完成
                    partsSpell = "/api/biz4s/parts/GetPartsSpell",//备件拼音码
                    Manhour = "/api/biz4s/maintain/GetManhour",
                    head = "/api/biz4s/maintain/GetWorkOrderHead/",
                    grid1 = "/api/biz4s/maintain/GetDispatchItems/",
                    grid2 = "/api/biz4s/maintain/GetDispatchParts/",
                    grid3 = "/api/biz4s/maintain/GetDispatchAppends/",
                    grid4 = "/api/biz4s/maintain/GetDispatchInspects/",
                    //删除
                    delete1 = "/api/biz4s/maintain/PostDeleteDispatchItem",
                    delete2 = "/api/biz4s/maintain/PostDeleteDispatchParts",
                    delete3 = "/api/biz4s/maintain/PostDeleteDispatchAppend",
                    delete4 = "/api/biz4s/maintain/PostDeleteDispatchInspect",

                    save = "/api/biz4s/maintain/PostSaveDispatch",
                    audit = "/api/biz4s/maintain/PostAuditDispatch",//审核入库单
                },
                dataSource = new
                {
                    EmpNames = new sys_userService().GetRoleMembersDict("FWGW")//服务顾问
                }
            };
            return View(model);
        }
    }

    public class MaintainApiController : BaseApiController
    {
        public dynamic GetWorkOrderHead(string id)
        {
            return new WorkOrderService().GetWorkOrderHead(id);
        }

        public dynamic GetAutoArchive(string id)
        {
            return new WorkOrderService().GetAutoArchive(id);
        }

        [HttpGet]
        public List<dynamic> GetManhour(string q)
        {
            return new WorkOrderService().GetManhour(q);
        }

        [HttpGet]
        public List<dynamic> GetDispatchItems(string id)
        {
            return new WorkOrderService().GetDispatchDetail(id, 1);
        }

        [HttpGet]
        public List<dynamic> GetDispatchParts(string id)
        {
            return new WorkOrderService().GetDispatchDetail(id, 2);
        }

        [HttpGet]
        public List<dynamic> GetDispatchAppends(string id)
        {
            return new WorkOrderService().GetDispatchDetail(id, 3);
        }

        [HttpGet]
        public List<dynamic> GetDispatchInspects(string id)
        {
            return new WorkOrderService().GetDispatchDetail(id, 4);
        }

        public dynamic PostSaveDispatch(dynamic data)
        {
            return new WorkOrderService().SaveDispatch(data);
        }

        [HttpPost]
        public string PostDeleteDispatchHead(dynamic data)
        {
            return new WorkOrderService().DeleteDispatch(data["id"].ToString(),0);
        }

        [HttpPost]
        public string PostDeleteDispatchItem(dynamic data)
        {
            return new WorkOrderService().DeleteDispatch(data["id"].ToString(), 1);
        }

        [HttpPost]
        public string PostDeleteDispatchParts(dynamic data)
        {
            return new WorkOrderService().DeleteDispatch(data["id"].ToString(), 2);
        }

        [HttpPost]
        public string PostDeleteDispatchAppend(dynamic data)
        {
            return new WorkOrderService().DeleteDispatch(data["id"].ToString(), 3);
        }

        [HttpPost]
        public string PostDeleteDispatchInspect(dynamic data)
        {
            return new WorkOrderService().DeleteDispatch(data["id"].ToString(), 4);
        }

        public string PostAuditDispatch(dynamic data)
        {
            return new WorkOrderService().AuditDispatch(data);
        }
    }
}