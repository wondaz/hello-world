using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using Web.FourS.Areas.Sys4S.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class PartsController : BaseController
    {
        /// <summary>
        /// 备件销售单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult PartsSaleList()
        {
            var model = new
            {
                _xml = "biz4s.parts.partsSaleList",
                pkey = "SellOrderCode",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/parts/PostDeletePartsOrder",
                    outStock = "/biz4s/inv/StockOutEdit/",//跳转到出库单
                    audit = "/api/biz4s/parts/PostAuditOrder",//审核销售单
                    edit = "/biz4s/parts/PartsSaleEdit/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    Seller = "",
                    SellOrderCode = "",
                    SellType = "",
                    CustomerName = "",
                    BillState = "",
                    SellTime = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("BJXSY")//备件销售员
                }
            };
            return View(model);
        }

        /// <summary>
        /// 销售单编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult PartsSaleEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                isNew = id == "0",
                pkey = "SerialID",
                _xml = "biz4s.parts.partsSaleDetail",
                urls = new
                {
                    head = "/api/biz4s/parts/GetPartsOrderHead/",
                    detail = "/api/sys4s/comm/GetList/",
                    partsSpell = "/api/biz4s/parts/GetPartsSpell",
                    saveHead = "/api/biz4s/parts/PostSavePartsHead",
                    saveDetail = "/api/biz4s/parts/PostSavePartsDetail",
                    delete = "/api/biz4s/parts/PostDeleteOrderParts",
                    audit = "/api/biz4s/parts/PostAuditOrder",//审核销售单
                },
               // form = new PartsSaleService().GetSaleOrder(id),
                dataSource = new
                {
                    sellers = new sys_userService().GetRoleMembersDict("BJXSY")//备件销售员
                }
            };
            return View(model);
        }

        /// <summary>
        /// 采购订单管理
        /// </summary>
        /// <returns></returns>
        public ActionResult BuyOrderList()
        {
            var model = new
            {
                _xml = "biz4s.PartsOrder.BuyOrderList",
                pkey = "TradeOrderID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/biz4s/parts/BuyOrderEdit/", //跳转到编辑页面

                    delete = "/api/biz4s/parts/PostDeletePartsBuyOrder",
                    intoStock = "/biz4s/parts/IntoStockEdit/",
                    audit = "/api/biz4s/parts/PostAuditBuyOrder"                    
                },
                form = new //查询条件绑定字段
                {
                    TradeOrderCode = "",
                    TradeType = "",
                    PlanTime = "",
                    BillState = "",
                    OrderMan = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    OrderMan = new sys_userService().GetRoleMembersDict("BJCGY")//备件采购员
                }
            };
            return View(model);
        }

        
        /// <summary>
        /// 采购订单编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult BuyOrderEdit(string id)
        {
            //TradeOrderID
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                pkey = "SerialID",
                urls = new
                {
                    head = "/api/biz4s/parts/GetTradeOrderHead/",
                    detail = "/api/biz4s/parts/GetTradeOrderDetail/",
                    partsSpell = "/api/biz4s/parts/GetPartsSpell",
                    delete = "/api/biz4s/parts/PostDeleteBuyOrderParts",
                    save = "/api/biz4s/parts/PostSaveBuyOrder",
                    audit = "/api/biz4s/parts/PostAuditBuyOrder"//审核
                }
            };
            return View(model);
        }

        /// <summary>
        /// 备件采购入库管理
        /// </summary>
        /// <returns></returns>
        public ActionResult StockInList()
        {
            var model = new
            {
                _xml = "biz4s.partsOrder.StockInList",
                pkey = "StockInID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit1 = "/biz4s/parts/StockInEdit/", //跳转到采购入库编辑页面
                    edit2 = "/biz4s/parts/StockOtherEdit/", //跳转到采购入库编辑页面
                    delete = "/api/biz4s/parts/PostDeleteStockInOrder",
                    audit = "/api/biz4s/parts/PostAuditStockInOrder"                    
                },
                form = new //查询条件绑定字段
                {
                    StockInCode = "",
                    TradeType = "",
                    InTime = "",
                    BillState = "",
                    FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }

        public ActionResult StockInEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/parts/GetStockInHead/",
                    detail = "/api/biz4s/parts/GetStockInDetail/",
                    save = "/api/biz4s/parts/PostSaveStockIn",
                    audit = "/api/biz4s/parts/PostAuditStockInOrder"//审核入库单
                },
                dataSource = new
                {
                    buyOrderCodes = new PartsStockInService().GetTradeCodes(),
                    stocks = new PartsStockInService().GetStocks("BJ")//获取备件仓库列表
                }
            };
            return View(model);
        }

        /// <summary>
        /// 其他入库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult StockOtherEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/parts/GetStockOtherHead/",
                    detail = "/api/biz4s/parts/GetStockOtherDetail/",
                    save = "/api/biz4s/parts/PostSaveStockIn",
                    audit = "/api/biz4s/parts/PostAuditStockInOrder",//审核入库单
                    partsSpell = "/api/biz4s/parts/GetPartsSpell",
                    delete = "/api/biz4s/parts/PostDeleteStockInParts",
                },
                dataSource = new
                {
                    buyOrderCodes = new PartsStockInService().GetTradeCodes(),
                    stocks = new PartsStockInService().GetStocks("BJ")//获取备件仓库列表
                }
            };
            return View(model);
        }
    }

    public class PartsApiController : BaseApiController
    {
        [HttpGet]
        public dynamic GetPartsOrderHead(string id)
        {
            return new PartsOrderService().GetPartsOrderHead(id);
        }

        [HttpGet]
        public dynamic GetSpareParts(string id)
        {
            return new SparePartService().GetSpareParts(id);
        }

        [HttpGet]
        public List<dynamic> GetPartsSpell(string q)
        {
            return new SparePartService().GetPartsSpell(q);
        }

        [HttpPost]
        public string PostSavePartsHead(dynamic data)
        {
            var result = new PartsOrderService().SavePartsHead(data);
            return result;
        }

        [HttpPost]
        public string PostSavePartsDetail(dynamic data)
        {
            var result = new PartsOrderService().SavePartsDetail(data);
            return result;
        }

        /// <summary>
        /// 删除备件销售订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeletePartsOrder(dynamic data)
        {
            return new PartsOrderService().DeletePartsOrder(data["id"].ToString());
        }

        /// <summary>
        /// 删除订单中的备件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteOrderParts(dynamic data)
        {
            return new PartsOrderService().DeleteOrderParts(data["id"].ToString());
        }

        [HttpGet]
        public dynamic GetCustomerByPhone(string id)
        {
            return new PartsOrderService().GetCustomerByPhone(id);
        }

        /// <summary>
        /// 审核、反审核销售订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public int PostAuditOrder(dynamic data)
        {
            return new PartsOrderService().AuditOrder(data);
        }

        [HttpGet]
        public dynamic GetTradeOrderHead(string id)
        {
            return new PartsTradeOrderService().GetTradeOrderHead(id);
        }

        [HttpGet]
        public dynamic GetTradeOrderDetail(string id)
        {
            return new PartsTradeOrderService().GetTradeOrderDetail(id);
        }

        [HttpPost]
        public string PostDeleteBuyOrderParts(dynamic data)
        {
            return new PartsTradeOrderService().DeleteBuyOrderParts(data["id"].ToString());
        }

        [HttpPost]
        public dynamic PostSaveBuyOrder(dynamic data)
        {
            return new PartsTradeOrderService().SaveBuyOrder(data);
        }

        /// <summary>
        /// 删除备件采购订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeletePartsBuyOrder(dynamic data)
        {
            return new PartsTradeOrderService().DeletePartsBuyOrder(data["id"].ToString());
        }

        /// <summary>
        /// 审核、反审核采购订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public int PostAuditBuyOrder(dynamic data)
        {
            return new PartsTradeOrderService().AuditBuyOrder(data);
        }

        /// <summary>
        /// 删除配件入库单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteStockInOrder(dynamic data)
        {
            return new PartsStockInService().DeleteStockInOrder(data["id"].ToString());
        }

        [HttpGet]
        public dynamic GetStockInHead(string id)
        {
            return new PartsStockInService().GetStockInHead(id);
        }

        [HttpGet]
        public dynamic GetStockInDetail(string id)
        {
            return new PartsStockInService().GetStockInDetail(id);
        }

        [HttpPost]
        public dynamic PostSaveStockIn(dynamic data)
        {
            return new PartsStockInService().SaveStockIn(data);
        }

        /// <summary>
        /// 审核、反审核备件入库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostAuditStockInOrder(dynamic data)
        {
            return new PartsStockInService().AuditStockInOrder(data);
        }

        [HttpGet]
        public dynamic GetStockOtherHead(string id)
        {
            return new PartsStockInService().GetStockOtherHead(id);
        }

        [HttpGet]
        public dynamic GetStockOtherDetail(string id)
        {
            return new PartsStockInService().GetStockOtherDetail(id);
        }

        /// <summary>
        /// 删除订单中的备件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteStockInParts(dynamic data)
        {
            return new PartsStockInService().DeleteStockInParts(data["id"].ToString());
        }
    }
}