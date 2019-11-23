using Web.FourS.Areas.Biz4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class OldReturnAppealController : BaseController
    {
        // GET: FourS/OldReturnAppeal
        public ActionResult OldReturnAppealEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) { id = Guid.NewGuid().ToString(); };
            var model = new
            {
                _xml = "Biz4s.OldReturnAppealEdit.ClamiSaleList",
                _xml1 = "Biz4s.OldReturnAppealEdit.GridSaleList",
                urls = new
                {
                    delete = "/api/biz4s/OldReturnAppeal/PostReturnOldPartsList",
                    search = "/api/sys4s/comm/GetList",
                    save = "/api/biz4s/OldReturnAppeal/PostOldReturnAppeal"
                },
                form = new OldReturnAppealService().GetOldReturnAppeal(id),
                form1 = new
                {
                    P_PartName = "",
                    SPDCode = "",
                    F_ClaimTime = "",
                    B_CorpID = Frame.Core.FormsAuth.UserData.CorpID
                },
            };
            return View(model);
        }
        public ActionResult OldReturnAppealList() 
        {
            var model = new
            {
                pkey = "F_PKId",
                _xml = "Biz4s.OldReturnAppealEdit.GridOldReturnAppealList",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    delete = "/api/biz4s/OldReturnAppeal/PostOldReturnAppealandlist",
                    edit = "/biz4s/OldReturnAppeal/OldReturnAppealEdit/"
                },
                form = new
                {
                    F_OldPartsAppealCode = "",
                    F_Status = "",
                    F_OldPartsAppealTime = "",
                    B_CorpID =Frame.Core.FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }
    }
    public class OldReturnAppealApiController : BaseApiController 
    {
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic PostOldReturnAppeal(dynamic data) 
        {
            return new OldReturnAppealService().PostOldReturnAppeal(data);
        }
        
        public dynamic PostReturnOldPartsList(dynamic ID) 
        {
            return new OldReturnAppealService().DeleteOldReturnAppeal(ID);
        }
        public dynamic PostOldReturnAppealandlist(dynamic data) {
            return new OldReturnAppealService().DeleteOldReturnAppeal_List(data);
        }
    }
}