using Web.FourS.Areas.Biz4S.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    //旧件返厂
    public class ReturnOldPartsController : BaseController
    {
        // GET: FourS/ReturnOldParts
        public ActionResult ReturnOldPartsEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) 
            {
                id = Guid.NewGuid().ToString();
            }
            var model = new
            {
                _xml0 = "biz4s.ReturnOldParts.ReturnOldPartsList",
                _xml = "biz4s.ReturnOldParts.ClamiSaleList",
                urls = new{
                    delete = "/api/biz4s/ReturnOldParts/PostDeleteReturnOldPartsList",
                    search = "/api/sys4s/comm/GetList",
                    save = "/api/biz4s/ReturnOldParts/PostReturnOldParts",
                    grid2 = "/api/biz4s/ReturnOldParts/GetFreightAttachs/",
                    deleteAttach = "/api/biz4s/ReturnOldParts/PostdeleteAttach",
                    upload = "/api/biz4s/Freight/PostFile"
                },
                form = new ReturnOldPartsService().GetReeturnOldParts(id),
                form1 = new {
                    F_ClaimCode="",
                    F_ClaimTime=""
                },
            };
            return View(model);
        }

        public ActionResult ReturnOldPartsList() 
        {
            var model = new
            {
                pkey = "ID",
                _xml = "biz4s.ReturnOldParts.ReturnOldPartsGridList",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/ReturnOldParts/PostDeleteReturnOldParts",
                    edit = "/biz4s/ReturnOldParts/ReturnOldPartsEdit/"
                },
                form = new {
                    F_OldPartsCode = "",
                    F_OldReturnTime = "",
                    F_CarryName = "",
                    F_DeliveryNumber = "",
                    F_Status = "",
                    B_CorpID = Frame.Core.FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }
    }
    public class ReturnOldPartsApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostReturnOldParts(dynamic data) 
        {
            return new ReturnOldPartsService().PostReturnOldParts(data);
        }

        public dynamic PostDeleteReturnOldPartsList(dynamic ID) 
        {
            JObject data = ID;
            return new ReturnOldPartsService().DeleteReturnOldPartsList(data["id"].ToString());
        }
        
        public dynamic PostDeleteReturnOldparts(dynamic ID) 
        {
            JObject data = ID;
            return new ReturnOldPartsService().DeleteReturnOldparts(data["id"].ToString());
        }
        public List<dynamic> GetFreightAttachs(string id)
        {
            return new FreightService().GetFreightList(id, 2);
        }
        [HttpPost]
        public dynamic PostdeleteAttach(dynamic data) 
        {
            JObject datas = data;
            return new ReturnOldPartsService().deleteAttach(datas["id"].ToString());
        }
    }
}