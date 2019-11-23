using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    /// <summary>
    /// 备件库存
    /// </summary>
    public class InvController : BaseController
    {
        // GET: FourS/Inventory
        public ActionResult StockOutList()
        {
            var model = new
            {
                _xml = "biz4s.inv.StockOutList",
                pkey = "StockOutCode",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/inv/PostDeleteStockOut",
                    outStock = "/api/biz4s/inv/PostPartsOutStock/",
                    audit = "/api/biz4s/inv/PostAuditOrder",//审核
                    edit1 = "/biz4s/inv/StockOutEdit/", //跳转到销售出库
                    edit2 = "/biz4s/inv/OtherOutEdit/", //跳转到其他出库
                },
                form = new //查询条件绑定字段
                {
                    TradeType = "",
                    StockID = "",
                    StockOutCode = "",
                    CustomerName = "",
                    BillState = "",
                    OutTime = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    stocks = new PartsStockInService().GetStocks("BJ")//获取备件仓库列表
                }
            };

            return View(model);
        }

        public ActionResult StockOutEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/inv/GetStockOutHead/",
                    detail = "/api/biz4s/inv/GetStockOutDetail/",
                    save = "/api/biz4s/inv/PostSaveStockOut",
                    audit = "/api/biz4s/inv/PostAuditOrder"//审核
                },
                dataSource = new
                {
                    sellOrderCodes = new PartsStockOutService().GetSellCodes(),
                    stocks = new PartsStockInService().GetStocks("BJ")//获取备件仓库列表
                }
            };
            return View(model);
        }

        public ActionResult OtherOutEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";
            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/inv/GetOtherOutHead/",
                    detail = "/api/biz4s/inv/GetOtherOutDetail/",
                    partsSpell = "/api/biz4s/parts/GetPartsSpell",
                    PartsInv = "/api/biz4s/inv/GetPartsInventory/",
                    delete = "/api/biz4s/inv/PostDeleteStockOutParts",
                    save = "/api/biz4s/inv/PostSaveStockOut",
                    audit = "/api/biz4s/inv/PostAuditOrder"//审核
                },
                dataSource = new
                {
                    //sellOrderCodes = new PartsStockOutService().GetSellCodes(),
                    stocks = new PartsStockInService().GetStocks("BJ")//获取备件仓库列表
                }
            };
            return View(model);
        }

        public ActionResult InventoryList()
        {
            var model = new
            {
                _xml = "biz4s.inv.InventoryList",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    import = "/api/biz4s/inv/PostImportParts"
                },
                form = new //查询条件绑定字段
                {
                    SparePartName = "",
                    SparePartCode = "",
                    FormsAuth.UserData.CorpID
                }
            };

            return View(model);
        }
    }

    public class InvApiController : BaseApiController
    {
        [HttpGet]
        public dynamic GetStockOutHead(string id)
        {
            return new PartsStockOutService().GetStockOutHead(id);
        }

        [HttpGet]
        public dynamic GetOtherOutHead(string id)
        {
            return new PartsStockOutService().GetOtherOutHead(id);
        }

        [HttpGet]
        public dynamic GetStockOutDetail(RequestWrapper request)
        {
            return new PartsStockOutService().GetStockOutDetail(request["id"]);
        }

        [HttpGet]
        public dynamic GetOtherOutDetail(RequestWrapper request)
        {
            return new PartsStockOutService().GetOtherOutDetail(request["id"]);
        }  

        public string GetStockKeeper(string id)
        {
            return new PartsStockInService().GetStockKeeper(id);
        }

        [HttpPost]
        public string PostSaveStockOut(dynamic data)
        {
            return new PartsStockOutService().SaveStockOut(data);
        }

        public string PostAuditOrder(dynamic data)
        {
            return new PartsStockOutService().AuditOrder(data);
        }

        /// <summary>
        /// 删除备件出库订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteStockOut(dynamic data)
        {
            return new PartsStockOutService().DeleteStockOut(data["id"].ToString());
        }

        /// <summary>
        /// 删除出库单物料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteStockOutParts(dynamic data)
        {
            return new PartsStockOutService().DeleteStockOutParts(data["id"].ToString());
        }

        [HttpPost]
        public string PostImportParts(dynamic data)
        {
            return new SparePartService().ImportParts(data);
        }
    }
}