using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using Frame.Core;

namespace Web.FourS
{
    public class RequestWrapperParameterBinding : HttpParameterBinding
    {
        private struct AsyncVoid { }
        public RequestWrapperParameterBinding(HttpParameterDescriptor desc) : base(desc) { }
        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            // Set the binding result here
            var request = System.Web.HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query);
            var requestWrapper = new RequestWrapper(new NameValueCollection(request));
            if (!string.IsNullOrEmpty(request["_xml"]))
            {
                var xmlType = request["_xml"].Split('.');
                var xmlPath = "";
                if (xmlType.Length == 3)
                {
                    xmlPath = string.Format("~/Areas/{0}/Views/Shared/{1}.xml", xmlType[0], xmlType[1]);
                }

                requestWrapper.LoadXmlByUrl(xmlPath, xmlType[2]);
            }

            SetValue(actionContext, requestWrapper);

            // now, we can return a completed task with no result
            TaskCompletionSource<AsyncVoid> tcs = new TaskCompletionSource<AsyncVoid>();
            tcs.SetResult(default(AsyncVoid));
            return tcs.Task;
        }
    }
}