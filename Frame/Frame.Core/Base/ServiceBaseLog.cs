/*************************************************************************
 * 文件名称 ：ServiceBaseLog.cs                          
 * 描述说明 ：定义数据服务基类中的日志处理
 **************************************************************************/

using System;
using log4net;
using Newtonsoft.Json;

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        protected static ILog Log = LogManager.GetLogger(String.Format("Service{0}", typeof(T).Name));

        protected static void Logger(string function, Action tryHandle, Action<Exception> catchHandle = null, Action finallyHandle = null)
        {
            LogHelper.Logger(Log, function, ErrorHandle.Throw, tryHandle, catchHandle, finallyHandle);
        }

        protected static void Logger(string function, ErrorHandle errorHandleType, Action tryHandle, Action<Exception> catchHandle = null, Action finallyHandle = null)
        {
            LogHelper.Logger(Log, function, errorHandleType, tryHandle, catchHandle, finallyHandle);
        }

        public void Logger(string target, object message, string functionName = "")
        {
            using (var context = Db.Context().UseSharedConnection(true))
            {
                context.Insert("sys_log")
                    .Column("UserCode", FormsAuth.UserData.UserCode)
                    .Column("UserName", FormsAuth.UserData.UserName)
                    //.Column("Position", position)
                    .Column("Target", target)
                    .Column("FunctionName", functionName)
                    .Column("Message", message.ToString())
                    //.Column("LogDate", DateTime.Now)
                    .Execute();
            }
        }
    }
}
