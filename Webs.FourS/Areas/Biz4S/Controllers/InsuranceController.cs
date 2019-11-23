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
    public class InsuranceController : BaseController
    {
        // GET: FourS/Insurance
        
        public ActionResult InsuranEdit(string id) 
        {
            if(id == "" || id == null) id  ="0";
            //id = "f9711fbe-fb72-4707-b002-cbae19c987fe";
            var model = new
            {
                _xml = "biz4s.Insuransele.InsurSeleList",
                insSaleID = id,
                pkey = "SerialID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    save = "/api/biz4s/Insurance/PostInsurance",
                    vin = "/api/biz4s/Insurance/GetSaleVIN",//VIN自动完成
                    archive = "/api/biz4s/Insurance/GetArchiveInfo/",//根据VIN获取车辆信息
                },
                form = new Insurance().GetSaleOrder(id),
                form1= new 
                {
                    InsSaleID = ""
                },
                dataSource = new
                {
                    InsureCorpName = new Insurance().B_InsureCorp_GetList(),//保险公司
                    SaleMan = new sys_userService().GetRoleMembersDict("BXGW"),//推荐人（保险顾问）
                    B_AutoBrand = new Insurance().B_AutoBrand_GetList(),//品牌
                    B_AutoSeries = new Insurance().B_AutoSeries_GetList(),//车系
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),//车型
                    deptList = new Insurance().B_InsureType_GetList()

                }
            };
           return View(model);
        }

        public ActionResult SaleList() 
        {
            var model = new
            {
                _xml = "biz4s.insuranceList.insuranceList",
                pkey = "InsSaleID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/Insurance/PostDeleteInsurance",//删除
                    //outStock = "/api/biz4s/car/PostCarOutStock/",
                    audit = "/api/biz4s/Insurance/PostInsuranceOrder",
                    edit = "/biz4s/Insurance/InsuranEdit/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    InsSaleCode = "",//单据编号
                    CustomerName = "",//客户名称
                    BillState = "",//单据状态
                    SignCode = "",//车牌号
                    TranDate = "",//代办时间
                    InsOrderCode="",//保单号
                    CorpID = FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                }
            };
            return View(model);
        }
    }
    public class InsuranceApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostInsurance(dynamic data)
        {
            return new Insurance().PostInsurance(data);
        }
        public List<dynamic> GetSaleVIN(string q)
        {
            return new Insurance().GetVin(q);
        }

        public dynamic GetArchiveInfo(string id) 
        {
            return new Insurance().GetArchiveInfo(id);
        }

        
        public string PostDeleteBxMx(dynamic id) 
        {
            return new Insurance().DeleteBxMx(id);
        }

        
        public string PostDeleteInsurance(dynamic id) //删除保险单下拉列表
        {
            return new Insurance().DeleteInsurance(id);
        }
        [HttpPost]
        public int PostInsuranceOrder(dynamic data) //审核反审核
        {
            return new Insurance().PostInsuranceOrder(data);
        }
    }
}