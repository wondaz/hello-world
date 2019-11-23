using System.Web.Http;
using System.Web.Mvc;
using Frame.Core;
using System.Web.Security;
using System;

namespace Web.FourS.Areas
{
    public class BaseController : Controller
    {
    }

    //[RequestAuthorize]
    public class BaseApiController : ApiController
    {
        /// <summary>
        /// 导出数据
        /// </summary>
        public void Download()
        {
            Exporter.Instance().Download();
        }

        static BaseApiController()
        {
        }

        /// <summary>
        /// 验证用户是否授权
        /// </summary>
        /// <param name="encryptTicket"></param>
        /// <returns></returns>
        public static bool CheckUserAuth(string encryptTicket)
        {
            if (string.IsNullOrEmpty(encryptTicket) || encryptTicket == "undefined")
            {
                return false;
            }

            var ticket = FormsAuthentication.Decrypt(encryptTicket);
            if (ticket == null || ticket.Expired)
            {
                return false;
            }

            var strTicket = ticket.UserData;
            var index = strTicket.IndexOf("#", StringComparison.Ordinal);
            var strUserCode = strTicket.Substring(0, index);            
            return FormsAuth.UserData.UserCode.ToUpper() == strUserCode.ToUpper();
        }
    }
}
