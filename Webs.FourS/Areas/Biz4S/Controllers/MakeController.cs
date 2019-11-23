using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using Web.FourS.Areas.Sys4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class MakeController : BaseController
    {
        // GET: FourS/Make
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult MakeSaleEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                pkey = "BookingID",
                urls = new
                {
                    //audit = "/api/biz4s/car/PostAuditSaleOrder",//审核
                    save = "/api/biz4s/Make/PostMake",
                    vin = "/api/biz4s/Make/GetListSignCode",//VIN自动完成
                    archive = "/api/biz4s/Make/GetArchiveInfo/"//根据VIN获取车辆信息
                    //series = "/api/biz4s/car/GetAutoSerials/",
                    //models = "/api/biz4s/car/GetAutoModels/"
                },
                form = new MakeSelect().GetSaleOrder(id),
                dataSource = new
                {
                   
                }
            };
            return View(model);
        }
        public ActionResult MakeSeleList() 
        {
            var model = new
            {
                _xml = "biz4s.makesale.MakeSeleList",
                pkey = "BookingID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/Make/PostDeleteMake",
                    //outStock = "/api/biz4s/car/PostCarOutStock/",
                    //audit = "/api/biz4s/car/PostAuditSaleOrder",
                    edit = "/biz4s/Make/MakeSaleEdit/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    //Saleman = "",
                    //BillState = "",
                    //CustomerName = "",
                    //SaleDate = "",

                    BookingInTime = "",//预约时间
                    PRI = "",//优先级
                    BookingName = "",//预约人
                    BillTypeID = "",//预约单类型
                    SignCode = "",//车牌号
                    CorpID = FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                }
            };
            return View(model);
        }

        public ActionResult MakeBulletinList() 
        {
            var model = new
            {
                _xml = "biz4s.makesale.MakeSeleList",
                pkey = "BookingID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/Make/DeleteMake/",
                    //outStock = "/api/biz4s/car/PostCarOutStock/",
                    edit = "/biz4s/Make/MakeSaleEdit/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    BookingInTime = "",//预约时间
                    PRI = "",//优先级
                    BookingName = "",//预约人
                    BillTypeID = "",//预约单类型
                    SignCode = "jhsjjsjsjs",//车牌号
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                }
            };
            return View(model);
        }
    }

    public class MakeApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostMake(dynamic data)
        {
            return new MakeSelect().PostMake(data);
        }
        /// <summary>
        /// 删除预约单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string PostDeleteMake(dynamic id)
        {
            return new MakeSelect().DeleteMake(id);
        }

        public List<dynamic> GetListSignCode(string q) 
        {
            return new MakeSelect().GetListSignCode(q);
        }
        public dynamic GetArchiveInfo(string id) 
        {
            return new MakeSelect().GetArchiveInfo(id);
        }
        
    }
}