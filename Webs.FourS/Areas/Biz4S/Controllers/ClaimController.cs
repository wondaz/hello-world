using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;
using Web.FourS.Areas.Biz4S.Models;
using Frame.Core;
namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class ClaimController : BaseController
    {
        /// <summary>
        /// 索赔管理
        /// </summary>
        /// <returns></returns>
        public ActionResult ClaimList() 
        {
            
            var model = new
            {
                _xml = "biz4s.Claim.ClaimSele",
                pkey = "ID",
                urls = new
                {
                    edit = "/biz4s/Claim/ClaimEntry/", //跳转到编辑页面
                    search = "/api/sys4s/comm/GetList",
                    delete="/api/biz4s/Claim/PostDeleteClaim"//删除
                },
                form = new //查询条件绑定字段
                {
                    F_ClaimCode = "",
                    F_ClaimTime = "",
                    F_Status = "",
                    B_ChassisCode = "",
                    F_IsPrint = "",
                    ModelID = "",
                    B_CorpID = FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    B_AutoModel = new Insurance().B_AutoModel_GetList()//车型
                }
            };
            return View(model);
        }
        public ActionResult ClaimEntry(string id) 
        {
            if (id == "" || id == null) id = "0";
            var model = new
            {
                _xml1 = "Biz4s.Claim.grid1",
                _xml2 = "Biz4s.Claim.grid2",
                _xml3 = "Biz4s.Claim.grid3",
                _xml4 = "Biz4s.Claim.grid4",
                _xml5 = "Biz4s.Claim.grid5",
                _xml6 = "Biz4s.Claim.grid6",
                pkey = id,
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    save = "/api/biz4s/Claim/PostClaim",
                    Submit = "/api/biz4s/Claim/PostSubmitClaim",
                    vin = "/api/biz4s/Insurance/GetSaleVIN",//VIN自动完成
                    archive = "/api/biz4s/Claim/GetClaimInfo/",//根据VIN获取车辆信息
                    delete = "/api/biz4s/Claim/PostDeleteClaimPartList",//删除子表行的数据
                    partsSpell = "/api/biz4s/parts/GetPartsSpell"
                },
                form = new ClaimService().GetClaimEnty(id),
                form1=new{
                    PB_FaultCode = "",
                    PB_FaultName = ""
                },
                form2=new{
                    ManHourCode="",
                    ManHourDescribe="",
                },
                form3=new{
                    F_IngredientCode="",
                    F_IngredientName="",
                },
                form5=new{
                    B_ChassisCode=""
                },
                dataSource = new
                {
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),//车型
                }
            };
            return View(model);
        }
    }
    public class ClaimApiController : BaseApiController 
    {
        
        public dynamic PostDeleteClaim(dynamic id)
        {
            return new ClaimService().DeleteClaim(id);
        }
        /// <summary>
        /// 保存索赔信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public dynamic PostClaim(dynamic data) 
        {
            return new ClaimService().PostClaim(data);
        }
        /// <summary>
        /// 提交索赔信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public dynamic PostSubmitClaim(dynamic data) 
        {
            return new ClaimService().PostSubmitClaim(data);
        }
        /// <summary>
        /// 通过VIN码获取车辆相关联的信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public dynamic GetClaimInfo(string id) 
        {
            return new ClaimService().GetClaimInfo(id);
        }
        
        public dynamic PostDeleteClaimPartList(dynamic id)
        {
            return new ClaimService().DeleteClaimPartList(id);
        }
    }
}