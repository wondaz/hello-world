using System;
using FluentData;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Optimization;

namespace Web.FourS
{
    public class FrameworkConfig
    {
        public static void Register()
        {
            APP.OnDbExecuting = OnDbExecuting;
            APP.Init();
        }

        public static void OnDbExecuting(CommandEventArgs args)
        {
            var sql = args.Command.CommandText;
        }

        public static void LogDatabase(object sysLog,IDbContext db = null) 
        {
            var log = sysLog as sys_log;
            if (log==null) return;

            log.LogDate = DateTime.Now;

            if (db == null)
            {
                using (db = Db.Context())
                {
                    db.Insert("sys_log", log).AutoMap(x => x.ID).Execute();
                }
            }
            else
            {
                db.Insert("sys_log", log).AutoMap(x => x.ID).Execute();
            }
        }
    }

    public static class RegisterAreas
    {
        public static void InitConfig()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            FrameworkConfig.Register();
        }
    }
}