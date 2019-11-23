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
    public class CustomerMgrController : Controller
    {
        // GET: FourS/CustomerMgr
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CustomerEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                pkey = "CustomerID",
                urls = new
                {
                    save = "/api/biz4s/CustomerMgr/PostCustomerMgr",
                    vin = "/api/biz4s/Make/GetListSignCode",//VIN自动完成
                    archive = "/api/biz4s/Make/GetArchiveInfo/"//根据VIN获取车辆信息
                },
                form = new CustomerEntry().GetCustomerModel(id),
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("ZCXSY"),//销售顾问
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),
                }
            };
            return View(model);
        }
        public ActionResult CustomerList() 
        {
            var model = new
            {
                _xml = "biz4s.Customersele.Customersele",
                pkey = "CustomerID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/CustomerMgr/PostDeleteCustomer",//删除
                    //outStock = "/api/biz4s/car/PostCarOutStock/",
                    audit = "/api/biz4s/Insurance/PostInsuranceOrder",
                    edit = "/biz4s/CustomerMgr/CustomerEntry/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    CustomerName = "",//客户名称
                    MobileTel = "",//移动电话
                    Salesman = "",//销售顾问
                    TypeID = "",//客户类别
                    LevelID = "",//客户等级
                    IntentLevelID = "",//意向级别
                    InputDate="",//建档日期
                    ISLatency="",//潜在客户
                    ISServer="",//保有客户
                    CorpID = FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("ZCXSY"),//整车销售员
                }
            };
            return View(model);
        }
    }
    public class CustomerMgrApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostCustomerMgr(dynamic data)
        {
            return new CustomerEntry().PostCustomerMgr(data);
        }
        
        public dynamic PostDeleteCustomer(dynamic id) 
        {
            return new CustomerEntry().DeleteCustomer(id);
        }
    }
}