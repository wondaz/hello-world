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
    /// 维修派工结算单
    /// </summary>
    public class SettleController : BaseController
    {
        /// <summary>
        /// 结算单管理
        /// </summary>
        /// <returns></returns>
        public ActionResult SettlementList()
        {
            var model = new
            {
                _xml = "biz4s.maintain.SettlementList",
                pkey = "BalanceID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/biz4s/Settle/SettlementEdit/", //跳转到编辑页面                    
                    //delete = "/api/biz4s/Settle/DeleteSettlement/",
                    audit = "/api/biz4s/Settle/PostAuditSettlement"
                },
                form = new //查询条件绑定字段
                {
                    BalanceCode = "",
                    SignCode = "",
                    InputTime = "",
                    BillState = "",
                    FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }

        /// <summary>
        /// 结算单编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SettlementEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/Settle/GetWorkOrderHead/",
                    grid1 = "/api/biz4s/Settle/GetDispatchItems/",
                    grid2 = "/api/biz4s/Settle/GetDispatchParts/",
                    grid3 = "/api/biz4s/Settle/GetDispatchAppends/",
                    grid4 = "/api/biz4s/Settle/GetDispatchInspects/",

                    save = "/api/biz4s/Settle/PostSaveBalance",
                    audit = "/api/biz4s/Settle/PostAuditSettlement",//审核入库单
                },
                dataSource = new
                {
                    dispatchCodes = new SettlementService().GetDispatchCodes(),//已审核的派工单
                    InsureCorpName = new Insurance().B_InsureCorp_GetList()//保险公司
                }
            };
            return View(model);
        }
    }

    public class SettleApiController : BaseApiController
    {
        public dynamic GetWorkOrderHead(string id)
        {
            return new SettlementService().GetWorkOrderHead(id);
        }

        public dynamic GetMoneySum(string id)
        {
            return new SettlementService().GetMoneySum(id);
        }

        [HttpGet]
        public List<dynamic> GetDispatchItems(string id)
        {
            return new SettlementService().GetDispatchDetail(id, 1);
        }

        [HttpGet]
        public List<dynamic> GetDispatchParts(string id)
        {
            return new SettlementService().GetDispatchDetail(id, 2);
        }

        [HttpGet]
        public List<dynamic> GetDispatchAppends(string id)
        {
            return new SettlementService().GetDispatchDetail(id, 3);
        }

        [HttpGet]
        public List<dynamic> GetDispatchInspects(string id)
        {
            return new SettlementService().GetDispatchDetail(id, 4);
        }

        public dynamic PostSaveBalance(dynamic data)
        {
            return new SettlementService().SaveBalance(data);
        }

        /// <summary>
        /// 审核结算单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string PostAuditSettlement(dynamic data)
        {
            return new SettlementService().AuditSettlement(data);
        }
    }
}