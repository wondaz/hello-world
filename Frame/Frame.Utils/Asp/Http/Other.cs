using System;
using System.Web;
using System.IO;
using System.Text;
using System.Net;

namespace Frame.Utils
{
    /// <summary>
    /// WEB请求上下文信息工具类
    /// </summary>
    public partial class ZHttp
    {
        #region 获取当前站点的Application实例
        /// <summary>
        /// 获取当前站点的Application实例
        /// </summary>
        public static System.Web.HttpApplicationState Application
        {
            get
            {
                return HttpContext.Current.Application;
            }
        }
        #endregion

        #region 取得页面返回的信息
        public static string GeturlData(string url)
        {
            url = RootFullPath + url.Trim('/');
            string urlData = "";
            try
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(url);
                System.Net.WebResponse response = request.GetResponse();
                Stream resStream = response.GetResponseStream();
                var sr = new System.IO.StreamReader(resStream, Encoding.Default);
                urlData = sr.ReadToEnd();
                resStream.Close();
                sr.Close();
            }
            catch { }
            return urlData.Trim();
        }
        #endregion

        public static string GetHttpData(string requestUrl, string paramsStr)
        {
            HttpWebRequest request = null;
            try
            {
                if (!string.IsNullOrEmpty(paramsStr))
                {
                    requestUrl = string.Format("{0}?{1}", requestUrl, paramsStr);
                }
                request = (HttpWebRequest)WebRequest.Create(requestUrl);
                var encoding = Encoding.UTF8;
                request.Method = "GET";
                request.Timeout = 300000;
                request.KeepAlive = false;

                request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/x-silverlight, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/x-ms-application, application/x-ms-xbap, application/vnd.ms-xpsdocument, application/xaml+xml, application/x-silverlight-2-b1, */*";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return String.Empty;
                using (var reader = new StreamReader(responseStream, encoding))
                {
                    var resultStr = reader.ReadToEnd();
                    request.Abort();
                    response.Close();
                    return resultStr;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
            finally
            {
                if (request != null)
                {
                    request.Abort();
                }
            }
        }

        /// <summary>
        /// 返回JSon数据
        /// </summary>
        /// <param name="jsonData">要处理的JSON数据</param>
        /// <param name="url">要提交的URL</param>
        /// <returns>处理结果</returns>
        public static string PostHttpData(string jsonData, string url)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(jsonData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json";
            Stream reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);

            //声明一个HttpWebRequest请求
            request.Timeout = 90000;
            //设置连接超时时间
            request.Headers.Set("Pragma", "no-cache");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamReceive = response.GetResponseStream();
            Encoding encoding = Encoding.UTF8;

            StreamReader streamReader = new StreamReader(streamReceive, encoding);
            string  strResult = streamReader.ReadToEnd();
            streamReceive.Dispose();
            streamReader.Dispose();

            return strResult;
        }
    }
}

