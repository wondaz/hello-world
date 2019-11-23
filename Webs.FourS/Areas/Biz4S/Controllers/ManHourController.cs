using Web.FourS.Areas.Biz4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.FourS.Areas.Biz4S.Controllers
{
    public class ManHourController : Controller
    {
        // GET: FourS/ManHour
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 新增工时费
        /// </summary>
        /// <returns></returns>
        public ActionResult ManHourAdd(string id) 
        {
            if (string.IsNullOrEmpty(id)) id = "0";

            var model = new
            {
                pkey = "ManHourID",
                urls = new
                {
                    save = "/api/biz4s/ManHour/PostManHourAdd"
                },
                form = new ManHour().GetSaleManHour(id),
                dataSource = new
                {
                    B_AutoBrand = new Insurance().B_AutoBrand_GetList(),//品牌
                    B_AutoSeries = new Insurance().B_AutoSeries_GetList(),//车系
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),//车型
                }
            };
            return View(model);
        }
        public ActionResult ManHourList() 
        {
            var model = new
            {
                _xml = "biz4s.ManHourSele.ManHourList",
                pkey = "ManHourID",
                urls = new
                {
                    search = "/api/sys4s/comm/GetList",
                    edit = "fours/ManHour/ManHourAdd/",//跳转页面
                    delete = "/api/biz4s/ManHour/PostDeleteManHour"
                },
                form = new {
                    ManHourCode="",
                    SeriesID="",
                    ModelID = "",
                    CorpID = Frame.Core.FormsAuth.UserData.CorpID,
                },
                dataSource = new
                {
                    B_AutoSeries = new Insurance().B_AutoSeries_GetList(),//车系
                    B_AutoModel = new Insurance().B_AutoModel_GetList(),//车型
                }
            };
            return View(model);
        }
    }
    public class ManHourApiController : BaseApiController 
    {
        [HttpPost]
        public dynamic PostManHourAdd(dynamic data)
        {
            return new ManHour().PostManHour(data);
        }
        
        public dynamic PostDeleteManHour(dynamic id) 
        {
            return new ManHour().DeleteManHour(id);
        }
    }
}