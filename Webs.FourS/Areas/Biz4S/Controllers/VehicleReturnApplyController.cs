using Web.FourS.Areas.Biz4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class VehicleReturnApplyController : BaseController
    {
        // GET: FourS/VehicleReturnApply
        public ActionResult VehicleReturnApplyEdit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                id = "0";
            }
            var model = new
            {
                _xml = "Biz4s.ReturnOldParts.ClamiSaleList",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    SaleOrderCodes = "/api/Biz4s/VehicleReturnApply/GetSaleOrderCodes",//自动获取销售订单号
                    archive = "/api/Biz4s/VehicleReturnApply/GetArchiveInfo/",
                    save = "/api/Biz4s/VehicleReturnApply/PostVehicleReturn"
                },
                form = new VehicleReturnApplyServer().GetVehicleReturnApply(id),
                form1 = new VehicleReturnApplyServer().GetArchiveInfos(id)
                //form1 = new
                //{
                //    SalePrice = "",
                //    CustomerName = "",
                //    MobileTel="",
                //    Email="",
                //    Address="",
                //    VIN="",
                //    BrandName="",
                //    SeriesName="",
                //    ModelName="",
                //    OutsideColor="",
                //    InsideColor="",
                //    EngineCode="",
                //    MeasureCode=""
                //},
            };
            return View(model);
        }
        public ActionResult VehicleReturnApplyList() 
        {
            var model = new
            {
                pkey = "ID",
                _xml = "Biz4s.VehicleReturnApply.VehicleReturnApplyList",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/Biz4s/VehicleReturnApply/PostDeleteVehicleReturn",
                    edit = "/Biz4s/VehicleReturnApply/VehicleReturnApplyEdit/"
                },
                form = new
                {
                    CustomerName = "",
                    F_State = "",
                    SaleBackDate = "",
                    Vehicle_ReturnCode = "",
                    SaleOrderCode = "",
                    VIN = ""
                }
            };
            return View(model);
        }
    }
    public class VehicleReturnApplyApiController : BaseApiController 
    {
        /// <summary>
        /// 车辆销售单号列表获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<dynamic> GetSaleOrderCodes(string q) 
        {
            return new VehicleReturnApplyServer().SaleOrderCodes(q);
        }
        [HttpGet]
        public dynamic GetArchiveInfo(string id) 
        {
            return new VehicleReturnApplyServer().GetArchiveInfo(id);
        }
        [HttpPost]
        public dynamic PostVehicleReturn(dynamic data) {
            return new VehicleReturnApplyServer().PostVehicleReturn(data);
        }
        
        public dynamic PostDeleteVehicleReturn(dynamic id) {
            return new VehicleReturnApplyServer().DeleteVehicleReturn(id);
        }
       
    }
}