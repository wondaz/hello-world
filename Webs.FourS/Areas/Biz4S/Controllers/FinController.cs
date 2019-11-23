using System.Web.Mvc;
using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class FinController : BaseController
    {
        /// <summary>
        /// 整车销售收款
        /// </summary>
        /// <returns></returns>
        public ActionResult CarPayList()
        {
            var model = new
            {
                _xml1 = "Biz4s.fin.carPayList",
                _xml2 = "Biz4s.fin.carPaidItems",
                pkey = "VIN",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/api/biz4s/fin/PostPayItem"
                },
                form = new //查询条件绑定字段
                {
                    Saleman = "",
                    SaleDate = "",
                    IsPay = "0",
                    FormsAuth.UserData.CorpID
                },
                form2 = new //查询条件绑定字段
                {
                    VIN = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("ZCXSY")//整车销售员
                }
            };
            return View(model);

        }

        /// <summary>
        /// 备件销售收款
        /// </summary>
        /// <returns></returns>
        public ActionResult PartsPayList()
        {
            var model = new
            {
                _xml1 = "Biz4s.fin.partsPayList",
                _xml2 = "Biz4s.fin.partsPaidItems",
                pkey = "SellOrderCode",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/api/biz4s/fin/PostPayItem"
                },
                form = new //查询条件绑定字段
                {
                    SellOrderCode = "",
                    Seller = "",
                    SellTime = "",
                    IsPay = "0",
                    FormsAuth.UserData.CorpID
                },
                form2 = new //查询条件绑定字段
                {
                    SellOrderCode = "",
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("BJXSY")//备件销售员
                }
            };
            return View(model);
        }

        public ActionResult RepairPayList()
        {
            var model = new
            {
                _xml1 = "Biz4s.fin.RepairPayList",
                _xml2 = "Biz4s.fin.RepairPaidItems",
                pkey = "DispatchCode",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/api/biz4s/fin/PostPayItem"
                },
                form = new //查询条件绑定字段
                {
                    DispatchCode = "",
                    EmpName = "",
                    MeetAutoTime = "",
                    IsPay = "0",
                    FormsAuth.UserData.CorpID
                },
                form2 = new //查询条件绑定字段
                {
                    DispatchCode = "",
                    FormsAuth.UserData.CorpID
                },   
                dataSource = new
                {
                    Salemans = new sys_userService().GetRoleMembersDict("FWGW")//维修服务员
                }
            };
            return View(model);
        }
    }

    public class FinApiController : BaseApiController
    {
        [HttpGet]
        public dynamic GetPayDetail(string id)
        {
            return new CustomerPaidService().GetPayDetail(id);
        }

        public string PostPayItem(dynamic data)
        {
            return new CustomerPaidService().SavePayItem(data);
        }
    }
}