using System;
using System.IO;
using System.Reflection;
using log4net;

namespace Frame.Core
{
    public class LogHelper
    {
        #region 全局设置
        public static void Init()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var xml = assembly.GetManifestResourceStream("Frame.Core.Logs.Default.config");
            log4net.Config.XmlConfigurator.Configure(xml);
        }

        public static void Init(string path)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(path));
        }

        public static void Init(Stream xml)
        {
            log4net.Config.XmlConfigurator.Configure(xml);
        }
        #endregion

        public static void Logger(ILog log, string msg)
        {
            log.Debug(msg);
        }

        public static void Logger(ILog log, string function, ErrorHandle errorHandleType, Action tryHandle, Action<Exception> catchHandle = null, Action finallyHandle = null)
        {
            try
            {
                log.Debug(function);
                tryHandle();
            }
            catch (Exception ex)
            {
                log.Error(function + "失败", ex);

                catchHandle.Invoke(ex);

                if (errorHandleType == ErrorHandle.Throw) 
                    throw ex;
            }
            finally
            {
                if (finallyHandle != null)
                {
                    finallyHandle();
                }
            }
        }
    }
}
