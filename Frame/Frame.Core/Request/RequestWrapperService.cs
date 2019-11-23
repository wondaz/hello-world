/*******************************************
 * 文件名称 ：RequestWrapperService.cs                          
 * 描述说明 ：请求包装 服务
********************************************/

namespace Frame.Core
{
    public partial class RequestWrapper
    {
        public ServiceBase GetService()
        {
            var module = GetXmlNodeValue("module");
            return new ServiceBase(module);
        }

        public ServiceBase GetService(string module)
        {
            return new ServiceBase(module);
        }
    }
}