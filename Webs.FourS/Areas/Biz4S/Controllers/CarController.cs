using System.Collections.Generic;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;
using Web.FourS.Areas.Biz4S.Models;
using Frame.Core;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class CarController : BaseController
    {
        /// <summary>
        /// 车辆销售单查询
        /// </summary>
        /// <returns></returns>
        public ActionResult CarSaleList()
        {
            var model = new
            {
                _xml = "biz4s.car.carSaleList",
                pkey = "SaleOrderID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/car/PostDeleteSaleOrder",//删除
                    outStock = "/api/biz4s/car/PostCarOutStock/",//出库
                    audit = "/api/biz4s/car/PostAuditSaleOrder",//审核、反审核
                    edit = "/biz4s/car/CarSaleEdit/", //跳转到编辑页面
                    vin = "/api/biz4s/car/GetSaleVIN", //VIN自动完成
                },
                form = new //查询条件绑定字段
                {
                    Saleman = "",
                    MobileTel = "",
                    BillState = "",
                    CustomerName = "",
                    SaleDate = "",
                    VIN = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("ZCXSY")//整车销售顾问
                }
            };
            return View(model);
        }

        /// <summary>
        /// 销售单编辑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CarSaleEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                pkey = "SaleOrderID",
                urls = new
                {
                    audit = "/api/biz4s/car/PostAuditSaleOrder",//审核
                    save = "/api/biz4s/car/PostSaleOrder",
                    vin = "/api/biz4s/car/GetSaleVIN", //VIN自动完成
                    archive = "/api/biz4s/car/GetArchiveInfo/",//根据VIN获取车辆信息
                    series = "/api/biz4s/car/GetAutoSerials/",
                    models = "/api/biz4s/car/GetAutoModels/"
                },
                form = new SaleOrdersService().GetSaleOrder(id),
                dataSource = new
                {
                    salemans = new sys_userService().GetRoleMembersDict("ZCXSY"),//整车销售员
                    brands = new AutoArchivesService().GetAutoBrand()
                }
            };
            return View(model);
        }

        /// <summary>
        /// 车辆入库操作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CarIntoStoreEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";
            var model = new
            {
                _xml = "biz4s.car.carIntoStoreEdit",
                //pkey = "",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    vin = "/api/biz4s/car/GetVIN", //VIN自动完成
                    save = "/api/biz4s/car/PostCarIntoStock"//车型入库操作
                },
                dform = new CarInStockHeadService().GetInStockHead(id),
                form = new //查询条件绑定字段
                {
                    VIN = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    warehouse = new PartsStockInService().GetStocks("XC")//新车
                }
            };
            return View(model);
        }

        /// <summary>
        /// 整车库存
        /// </summary>
        /// <returns></returns>
        public ActionResult CarStockList()
        {
            var model = new
            {
                _xml = "biz4s.car.carStockList",
                pkey = "VIN",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    vin = "/api/biz4s/car/GetVIN", //VIN自动完成
                },
                form = new //查询条件绑定字段
                {
                    VIN = "",
                    SeriesName = "",
                    ModelName = "",
                    CorpID = FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }
    }

    public class CarApiController : BaseApiController
    {
        /// <summary>
        /// VIN输入框自动完成
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public List<dynamic> GetVIN(string q)
        {
            return new SaleOrdersService().GetVIN(q);
        }

        /// <summary>
        /// VIN自动完成
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public List<dynamic> GetSaleVIN(string q)
        {
            return new SaleOrdersService().GetSaleVIN(q);
        }

        /// <summary>
        /// 根据车牌自动完成
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public List<dynamic> GetSignCode(string q)
        {
            return new SaleOrdersService().GetSignCode(q);
        }


        /// <summary>
        /// 根据车辆VIN，获取车型档案信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetArchiveInfo(string id)
        {
            return new AutoArchivesService().GetArchiveInfo(id);
        }

        /// <summary>
        /// 获取系列
        /// </summary>
        /// <param name="id">品牌ID</param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetAutoSerials(string id)
        {
            return new AutoArchivesService().GetAutoSerials(id);
        }

        /// <summary>
        /// 获取车型
        /// </summary>
        /// <param name="id">系列ID</param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetAutoModels(string id)
        {
            return new AutoArchivesService().GetAutoModels(id);
        }

        [HttpPost]
        public dynamic PostSaleOrder(dynamic data)
        {
            return new SaleOrdersService().PostSaleOrder(data);
        }

        public string PostCarIntoStock(dynamic data)
        {
            return new CarInStockHeadService().SaveCarIntoStock(data);
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostDeleteSaleOrder(dynamic data)
        {
            return new SaleOrdersService().DeleteSaleOrder(data["id"].ToString());
        }

        /// <summary>
        /// 车辆出库
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public string PostCarOutStock(dynamic data)
        {
            return new SaleOrdersService().CarOutStock(data);
        }


        /// <summary>
        /// 审核、反审核销售订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public int PostAuditSaleOrder(dynamic data)
        {
            return new SaleOrdersService().AuditSaleOrder(data);
        }
    }
}