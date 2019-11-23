using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using log4net;
using FluentData;
using Frame.Utils;

namespace Frame.Core
{
    public static class Db
    {
        static ILog log = LogManager.GetLogger(APP.DB_DEFAULT_CONN_NAME);
        public static IDbContext Context()
        {
            //log = LogManager.GetLogger(APP.DB_DEFAULT_CONN_NAME);
            return Context(APP.DB_DEFAULT_CONN_NAME);
        }


        private static IDbProvider Provider(string providerName)
        {
            if (providerName == "SqlServer")
            {
                APP.DbProvider = DbProviderEnum.SqlServer;
                return new SqlServerProvider();
            }

            APP.DbProvider = DbProviderEnum.Oracle;
            return new OracleProvider();
        }

        public static IDbContext Context(string connName)
        {            
            var setting = ConfigurationManager.ConnectionStrings[connName];
            var db = new DbContext().ConnectionString(setting.ConnectionString, Provider(setting.ProviderName));
            if (ZConfig.GetConfigString("SqlLog") == "1")
            {
                db.OnExecuting(x =>
                {                    
                    //if (APP.OnDbExecuting != null) APP.OnDbExecuting(x);
                    var sql = x.Command.CommandText;
                    for (int i = x.Command.Parameters.Count - 1; i >= 0; i--)
                    {
                        var item = x.Command.Parameters[i] as IDataParameter;
                        sql = sql.Replace(item.ParameterName, string.Format("'{0}'", item.Value));
                    }
                    log.Debug(sql);
                });
            }

            db.OnError(e =>
            {
                var log = LogManager.GetLogger(connName);
                var ex = e.Exception as SqlException;
                if (ex != null)
                {
                    var error = "";
                    switch (ex.Number)
                    {
                        case -2:
                            error = "超时时间已到。在操作完成之前超时时间已过或服务器未响应";
                            break;
                        case 4060:
                            // Invalid Database
                            error = "数据库不可用,请检查系统设置后重试！";
                            break;
                        case 18456:
                            // Login Failed
                            error = "登陆数据库失败！";
                            break;
                        case 547:
                            // ForeignKey Violation
                            error = "数据已经被引用，更新失败，请先删除引用数据并重试！";
                            break;
                        case 2627:
                            // Unique Index/Constriant Violation
                            error = "主键重复，更新失败！";
                            break;
                        case 2601:
                            // Unique Index/Constriant Violation   
                            break;
                        // throw a general DAL Exception   
                    }
                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    log.Error(ex.Message);
                }
            });

            return db;
        }
    }
}
