/******************************************
 * 文件名称 ：RequestWrapper.cs                          
 * 描述说明 ：请求包装
*******************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Frame.Core
{
    public partial class RequestWrapper
    {
        private List<string> ignores = new List<string> { null,"token", "table", "page", "rows", "sort", "order", "format", "FLUENTDATA_ROWNUMBER" };
        private string ignoreStartWith = "_";
        private NameValueCollection request { get; set; }

        private string settingXml = "<settings></settings>";
        
        public bool IsLoadedSettings { get { return !string.IsNullOrEmpty(settingXml); } }

        //索引器
        public string this[string name]
        {
            get
            {
                var field = string.Empty;
                if (name.IndexOf(".", StringComparison.Ordinal) >= 0)
                {
                    field = name.Split('.')[1];
                }

                return request[field] ?? request[GetAliasName(name)];
            }
            private set
            {
                if (name.IndexOf(".", StringComparison.Ordinal) >= 0)
                {
                    name = name.Split('.')[1];
                }

                request[name] = value;
            }
        }

        #region 构造函数
        public RequestWrapper(NameValueCollection query)
        {
            this.SetRequestData(query);
        }

        public RequestWrapper()
        {
            this.SetRequestData(new NameValueCollection());
        }

        public static RequestWrapper Instance()
        {
            return new RequestWrapper();
        }

        public static RequestWrapper InstanceFromRequest()
        {
            var request = new NameValueCollection(HttpContext.Current.Request.QueryString);
            return new RequestWrapper(request);
        }
        #endregion
    }
}