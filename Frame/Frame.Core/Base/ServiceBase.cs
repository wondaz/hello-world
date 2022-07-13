/***************************************
 * 文件名称 ：ServiceBase.cs                          
 * 描述说明 ：定义数据服务基类
 ***************************************/

using System;
using System.Dynamic;
using FluentData;

namespace Frame.Core
{
    public class ServiceBase : ServiceBase<ModelBase>
    {
        public ServiceBase() { }
        public ServiceBase(string module)
            : base(module) { }
    }

    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        protected ServiceBase()
        {
            var moduleName = AttributeHelper.GetModuleAttribute(this.GetType());
            if (!string.IsNullOrEmpty(moduleName))
            {
                ModuleName = moduleName;
            }
        }

        protected ServiceBase(string moduleName)
        {
            ModuleName = moduleName;
        }

        ~ServiceBase()
        {
            try
            {
                db.Dispose();
            }
            catch(Exception ex)
            {
            }
        }

        public static ServiceBase<T> Instance()
        {
            return new ServiceBase<T>();
        }


        public dynamic GetNewModel(dynamic defaultValue)
        {
            return new T().Extend(defaultValue);
        }

        public int StoredProcedure(ParamSP param)
        {
            var result = 0;
            Logger("执存储过程", () => result = BuilderParse(param).Execute());
            return result;
        }

        public string GetNewKey(string field, string rule, int qty = 1, ParamQuery pQuery = null)
        {
            var result = string.Empty;

            Logger("获取新主键", () =>
            {
                for (var i = 0; i < qty; i++)
                {
                    string newkey, table = typeof(T).Name;
                    ;
                    switch (rule)
                    {
                        case "guid":
                            newkey = NewKey.GetGuidString();
                            break;
                        case "datetime":
                            newkey = NewKey.datetime();
                            break;
                        case "Dateplus":
                            newkey = NewKey.Dateplus(db, table, field, "yyyyMMdd", 3);
                            break;
                        case "maxplus":
                            newkey = NewKey.maxplus(db, table, field);
                            break;
                        default:
                            newkey = "";
                            break;
                    }

                    result += "," + newkey;
                }
            });

            return result.Trim(',');
        }

        #region 变量

        private IDbContext _db;
        public IDbContext db
        {
            get
            {
                if (string.IsNullOrEmpty(ModuleName))
                {
                    ModuleName = APP.DB_DEFAULT_CONN_NAME;
                }
                _db = Db.Context(ModuleName);
                return _db;
                //return _db ?? (_db = Frame.Core.Db.Context(ModuleName));
            }
        }

        //public IDbContext db => _db ?? (_db = Db.Context(ModuleName));

        private AjaxMessge Msg { get; set; }

        public string ModuleName { private get; set; }

        #endregion
    }
}