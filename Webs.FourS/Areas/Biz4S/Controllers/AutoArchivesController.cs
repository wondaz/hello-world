using Frame.Core;
using Web.FourS.Areas.Biz4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class AutoArchivesController : BaseController
    {
        //车辆档案管理
        public ActionResult AutoArchivesList() 
        {
            var model = new
            {
                _xml = "biz4s.AutoArchives.AutoArchivesSele",
                pkey = "ID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "/biz4s/AutoArchives/AutoArchivesEdit/", //跳转到编辑页面
                },
                form = new //查询条件绑定字段
                {
                    BrandID="",//品牌
                    SeriesID="",//车系
                    ModelID="",//车型
                    VIN="",
                    EngineCode="",//发动机号
                    SignCode="",//车牌号
                    FormsAuth.UserData.CorpID
                },
                dataSource = new
                {
                    B_AutoBrand = new Insurance().B_AutoBrand_GetList(),//品牌
                    B_AutoSeries = new Insurance().B_AutoSeries_GetList(),//车系
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),//车型
                }
            };
            return View(model);
        }
        public ActionResult AutoArchivesEdit(string id) 
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                pkey = "ID",
                urls = new
                {
                    save = "/api/biz4s/AutoArchives/PostAutoArchives",
                },
                form = new AutoArchivesSeleEntry().GetAutoArchivesModel(id),
                dataSource = new
                {
                    InsureCorpName = new Insurance().B_InsureCorp_GetList(),//保险公司
                }
            };
            return View(model);
        }
    }
    public class AutoArchivesApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostAutoArchives(dynamic data) 
        { 
            return new AutoArchivesSeleEntry().PostAutoArchives(data);        }
    }
}