/*************************************************************************
 * 文件名称 ：ServiceBaseQuery.cs                          
 * 描述说明 ：定义数据服务基类中的查询处理
 **************************************************************************/

using System.Collections.Generic;
using System.Dynamic;
using FluentData;

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        public List<T> GetModelList(ParamQuery param = null)
        {
            return BuilderParse(param).QueryMany();
        }

        public List<dynamic> GetDynamicList(ParamQuery param = null)
        {
            IDbCommand command = GetPreparedDbCommand(BuilderParse(param).Data);
            if(command == null) return new List<dynamic>();

            var result = command.QueryMany<dynamic>();
            return result;
        }

        public List<T> GetList(ParamQuery param = null)
        {
            IDbCommand command = GetPreparedDbCommand(BuilderParse(param).Data);
            if (command == null) return new List<T>();

            var result = command.QueryMany<T>();
            return result;
        }
        
        public dynamic GetModelListWithPaging(ParamQuery param = null)
        {
            dynamic result = new ExpandoObject();
            result.rows = this.GetModelList(param);
            result.total = this.QueryRowCount(param);
            return result;
        }

        public dynamic GetListWithPaging(ParamQuery param = null)
        {
            dynamic result = new ExpandoObject();
            result.rows = this.GetList(param);
            result.total = this.QueryRowCount(param);
            return result;
        }

        public dynamic GetDynamicListWithPaging(ParamQuery param = null)
        {
            dynamic result = new ExpandoObject();
            result.rows = this.GetDynamicList(param);
            result.total = this.QueryRowCount(param);
            return result;
        }

        public T GetModel(ParamQuery param)
        {
            //Logger("获取实体对象", () => result = BuilderParse(param).QuerySingle());
            var result = BuilderParse(param).QuerySingle();
            return result;
        }

        public dynamic GetDynamic(ParamQuery param)
        {
            var result = new ExpandoObject();
            var command = GetPreparedDbCommand(BuilderParse(param).Data);
            return command == null ? result : command.QuerySingle<dynamic>();
        }

        public TField GetField<TField>(ParamQuery param)
        {
            var result = default(TField);
            var command = GetPreparedDbCommand(BuilderParse(param).Data);
            return command == null ? result : command.QuerySingle<TField>();
        }

        private IDbCommand GetPreparedDbCommand(SelectBuilderData data)
        {
            if (data.PagingItemsPerPage > 0
                && string.IsNullOrEmpty(data.OrderBy))
            {
                Logger("[SQL] Order by must defined when using Paging.", null);
                return null;
                //throw new FluentDataException("Order by must defined when using Paging.");
            }
            data.Command.ClearSql.Sql(data.Command.Data.Context.Data.FluentDataProvider.GetSqlForSelectBuilder(data));
            return data.Command;
        }
    }
}
