﻿using System.Collections.Generic;
using System.Web.Mvc;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS
{
    public class MvcMenuFilter : ActionFilterAttribute
    {
        private bool _isEnable = true;

        public MvcMenuFilter()
        {
            _isEnable = true;
        }

        public MvcMenuFilter(bool IsEnable)
        {
            _isEnable = IsEnable;
        }

        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    if (_isEnable)
        //    {
        //        var list = new List<string>();
        //        var route = filterContext.RouteData.Values;
        //        var url = string.Format("/{0}/{1}/{2}", route["area"], route["controller"], route["action"]).ToLower();
        //        list.Add(url);
        //        if (url.EndsWith("/index"))
        //        {
        //            url = url.Substring(0, url.Length - 6);
        //            list.Add(url);
        //        }

        //        if (url.EndsWith("/home"))
        //        {
        //            url = url.Substring(0, url.Length - 5);
        //            list.Add(url);
        //        }

        //        if (!new sys_userService().AuthorizeUserMenu(list))
        //            filterContext.Result = new ContentResult() { Content = "你没有访问此功能的权限，请联系管理员！" };
        //    }

        //    base.OnActionExecuting(filterContext);
        //}
    }
}