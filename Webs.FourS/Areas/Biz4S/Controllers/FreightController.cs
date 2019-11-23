using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    /// <summary>
    /// 运费索赔管理
    /// </summary>
    public class FreightController : BaseController
    {
        public ActionResult FreightList()
        {
            var model = new
            {
                _xml = "biz4s.Freight.FreightList",
                pkey = "F_PKId",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/biz4s/Freight/FreightEdit/", //跳转到编辑页面                    
                    delete = "/api/biz4s/Freight/PostDeleteFreight",
                    revoke = "/api/biz4s/Freight/PostRevokeFreight"
                },
                form = new //查询条件绑定字段
                {
                    F_CostCode = "",
                    F_Status = "",
                    F_InputTime = "",
                    B_CorpID = FormsAuth.UserData.CorpID
                }
            };
            return View(model);
        }

        public ActionResult FreightEdit(string id)
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                keyVal = id,
                urls = new
                {
                    head = "/api/biz4s/Freight/GetFreightHead/",
                    grid1 = "/api/biz4s/Freight/GetFreightList/",
                    grid2 = "/api/biz4s/Freight/GetFreightAttachs/",
                    grid3 = "/api/biz4s/Freight/GetOldPartsReturn/",
                    save = "/api/biz4s/Freight/PostSaveFreight",
                    delete1 = "/api/biz4s/Freight/PostDeleteCostListItem",
                    delete2 = "/api/biz4s/Freight/PostDeleteAttachsItem",
                    upload = "/api/biz4s/Freight/PostFile"
                }
            };
            return View(model);
        }
    }

    public class FreightApiController : BaseApiController
    {
        [HttpPost]
        public string PostDeleteFreight(dynamic data)
        {
            return new FreightService().DeleteFreight(data["id"].ToString(), 1);
        }

        [HttpPost]
        public string PostRevokeFreight(dynamic data)
        {
            return new FreightService().DeleteFreight(data["id"].ToString(), 2);
        }

        [HttpPost]
        public string PostDeleteCostListItem(dynamic data)
        {
            return new FreightService().DeleteFreight(data["id"].ToString(), 3);
        }

        [HttpPost]
        public string PostDeleteAttachsItem(dynamic data)
        {
            return new FreightService().DeleteFreight(data["id"].ToString(), 4);
        }

        public dynamic GetFreightHead(string id)
        {
            return new FreightService().GetFreightHead(id);
        }

        public List<dynamic> GetFreightList(string id)
        {
            return new FreightService().GetFreightList(id, 1);
        }

        public List<dynamic> GetFreightAttachs(string id)
        {
            return new FreightService().GetFreightList(id, 2);
        }

        public List<dynamic> GetOldPartsReturn()
        {
            return new FreightService().GetFreightList("", 3);
        }

        public dynamic PostSaveFreight(dynamic data)
        {
            return new FreightService().SaveFreight(data);
        }

        [System.Web.Http.HttpPost]
        public dynamic PostFile()
        {
            return new CommonService().UploadFiles(HttpContext.Current.Request, "Freigh");
        }

    }
}