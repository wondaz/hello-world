using System;
using System.Web.Http.Filters;

namespace Web.FourS
{
    public class WebApiDisposeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            GC.Collect();
        }
    }
}