/*************************************************************************
 * 文件名称 ：ServiceBaseUtils.cs                          
 * 描述说明 ：定义数据服务基类中的工具类
 **************************************************************************/

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using FluentData;
using Frame.Core.Generator;
using Frame.Utils;

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        public ISqlGen SqlGenObject
        {
            get
            {
                ISqlGen sqlGen;
                if (APP.DbProvider == DbProviderEnum.SqlServer)
                {
                    sqlGen = new SqlServerGen();
                }
                else
                {
                    sqlGen = new OracleGen();
                }
                return sqlGen;
            }
        }

        public List<TableSchema> ColumnSchema { get; set; }
        private static Dictionary<string, object> GetPersonDateForUpdate()
        {
            var dict = new Dictionary<string, object>
            {
                {APP.FIELD_UPDATE_PERSON, string.Format("'{0}'", FormsAuth.UserData.UserName)},
                {
                    APP.FIELD_UPDATE_DATE,
                    APP.DbProvider == DbProviderEnum.SqlServer
                        ?string.Format("'{0}'", DateTime.Now):
                        string.Format("to_date('{0}' , 'yyyy-mm-dd hh24:mi:ss')", DateTime.Now)
                }
            };

            return dict;
        }

        protected ISelectBuilder<T> BuilderParse(ParamQuery param)
        {
            if (param == null)
            {
                param = new ParamQuery();
            }

            var data = param.GetData();
            var sFrom = data.From.Length == 0 ? typeof(T).Name : data.From;
            var selectBuilder = db.Select<T>(string.IsNullOrEmpty(data.Select) ? (sFrom + ".*") : data.Select)
                .From(sFrom)
                .Where(data.WhereSql)
                .GroupBy(data.GroupBy)
                .Having(data.Having)
                .OrderBy(data.OrderBy)
                .Paging(data.PagingCurrentPage, data.PagingItemsPerPage);
            return selectBuilder;
        }

        protected string BuilderParse(ParamInsert param)
        {
            var data = param.GetData();
            var dict = GetPersonDateForUpdate();
            var columnNames = new StringBuilder();
            var columnValues = new StringBuilder();
            var tableName = data.From.Length == 0 ? typeof(T).Name : data.From;
            GetTableSchema(tableName);
            foreach (var column in data.Columns.Where(column => !dict.ContainsKey(column.Key.ToUpper())))
            {
                var colData = ColumnSchema.FirstOrDefault(s => s.ColumnName.ToUpper() == column.Key.ToUpper());
                if (colData == null) continue;

                columnNames.AppendFormat("{0},", column.Key);
                if (column.Value == null)
                {
                    columnValues.Append("NULL,");
                }
                else
                {
                    columnValues.AppendFormat("{0},", FormatParamValue(column.Value, colData.SqlTypeName));
                }
            }

            //var properties = Utils.ZReflection.GetProperties(typeof(T));
            foreach (var item in dict)
            {
                var colData = ColumnSchema.FirstOrDefault(s => s.ColumnName.ToUpper() == item.Key.ToUpper());
                if (colData == null) continue;

                //更新数据中包含UpdateDate值，且不为null，则使用数据中的值，否则取当前时间
                var updateValue = "";
                if (data.Columns.ContainsKey(item.Key) && data.Columns[item.Key] != null &&
                    data.Columns[item.Key].ToString() != "")
                {
                    updateValue = APP.DbProvider == DbProviderEnum.SqlServer
                        ? string.Format("'{0}'", data.Columns[item.Key])
                        : string.Format(
                            item.Key.ToUpper() == APP.FIELD_UPDATE_DATE ? "to_date('{0}' , 'yyyy-mm-dd hh24:mi:ss')" : "'{0}'",
                            data.Columns[item.Key]);
                }

                columnNames.AppendFormat("{0},", item.Key);
                columnValues.AppendFormat("{0},", updateValue == "" ? item.Value : updateValue);
            }
            
            var insertSql = string.Format("INSERT INTO {0}({1}) VALUES({2})", tableName, columnNames.Remove(columnNames.Length - 1, 1), columnValues.Remove(columnValues.Length - 1, 1));
            return insertSql;
        }

        private string FormatParamValue(object value, string sqlTypeName)
        {
            if (APP.DbProvider == DbProviderEnum.Oracle && sqlTypeName.ToUpper() == "DATE")
            {
                return string.Format("to_date('{0}' , 'yyyy-mm-dd hh24:mi:ss')", value);
            }

            return string.Format("'{0}'", value);
        }

        private void GetTableSchema(string tableName)
        {
            if (ColumnSchema == null || (ColumnSchema.Any() && ColumnSchema[0].TableName.ToUpper() != tableName.ToUpper()))
            {
                tableName = tableName.Replace("\n", "").Trim();
                ColumnSchema = db.Sql(SqlGenObject.SqlGetTableSchemas(tableName)).QueryMany<TableSchema>();
            }
        }
        protected string BuilderParse(ParamUpdate param)
        {
            var data = param.GetData();
            var tableName = data.Update.Length == 0 ? typeof(T).Name : data.Update;
            GetTableSchema(tableName);
            var sqlCols = new StringBuilder();
            var dict = GetPersonDateForUpdate();
            foreach (var column in data.Columns.Where(column => !dict.ContainsKey(column.Key.ToUpper())))
            {
                var colData = ColumnSchema.FirstOrDefault(s => s.ColumnName.ToUpper() == column.Key.ToUpper());
                if (colData == null) continue;

                if (column.Value == null)
                {
                    sqlCols.AppendFormat("{0} = NULL,", column.Key);
                }
                else
                {
                    sqlCols.AppendFormat("{0}={1},", column.Key, FormatParamValue(column.Value, colData.SqlTypeName));
                }
            }

            //var properties = Utils.ZReflection.GetProperties(typeof(T));
            foreach (var item in dict)
            {
                var colData = ColumnSchema.FirstOrDefault(s => s.ColumnName.ToUpper() == item.Key.ToUpper());
                if (colData == null) continue;
                var updateValue = "";
                if (data.Columns.ContainsKey(item.Key) && data.Columns[item.Key] != null && data.Columns[item.Key].ToString() != "")
                {
                    //更新数据中包含UpdateDate值，且不为null，则使用数据中的值，否则取当前时间
                    updateValue = APP.DbProvider == DbProviderEnum.SqlServer
                         ? string.Format("'{0}'", data.Columns[item.Key])
                         : string.Format(
                             item.Key.ToUpper() == APP.FIELD_UPDATE_DATE ? "to_date('{0}' , 'yyyy-mm-dd hh24:mi:ss')" : "'{0}'",
                             data.Columns[item.Key]);
                }

                sqlCols.AppendFormat("{0}={1},", item.Key, updateValue == "" ? item.Value : updateValue);
            }

            var updateSql = string.Format("UPDATE {0} SET {1} WHERE {2}", tableName, sqlCols.Remove(sqlCols.Length - 1, 1), data.WhereSql);
            return updateSql;
        }

        //protected IDeleteBuilder BuilderParse(ParamDelete param)
        //{
        //    var data = param.GetData();
        //    var deleteBuilder = db.Delete(data.From.Length == 0 ? typeof(T).Name : data.From);

        //    //todo wdz
        //    deleteBuilder.Where(data.WhereSql);
        //    //deleteBuilder.Where(data.Where[0].Data.Column,data.Where[0].Data.Value);

        //    var tableName = data.From.Length == 0 ? typeof (T).Name : data.From;
        //    string delSql = string.Format("DELETE FROM {0} WHERE {1}", tableName, data.WhereSql);
        //    return deleteBuilder;
        //}

        protected string BuilderDelSql(ParamDelete param)
        {
            var data = param.GetData();
            var tableName = data.From.Length == 0 ? typeof(T).Name : data.From;
            string delSql = string.Format("DELETE FROM {0} WHERE {1}", tableName, data.WhereSql);
            return delSql;
        }

        protected IStoredProcedureBuilder BuilderParse(ParamSP param)
        {
            var data = param.GetData();
            var spBuilder = db.StoredProcedure(data.Name);
            foreach (var item in data.Parameter)
                spBuilder.Parameter(item.Key, item.Value);

            foreach (var item in data.ParameterOut)
                spBuilder.ParameterOut(item.Key, item.Value);

            return spBuilder;
        }

        protected int QueryRowCount(ParamQuery param)
        {
            var data = param.GetData();
            var sFrom = data.From.Length == 0 ? typeof(T).Name : data.From;
            var selectBuilder = db.Select<int>("COUNT(1)")
                .From(sFrom)
                .Where(data.WhereSql);
            return selectBuilder.QuerySingle();
        }

        protected dynamic Extend(object form1, object form2)
        {
            var expando = (IDictionary<string, object>)new ExpandoObject();
            EachHelper.EachObjectProperty(form1, (i, name, value) =>
            {
                expando.Add(name, value);
            });

            if (form2 != null)
            {
                EachHelper.EachObjectProperty(form2, (i, name, value) =>
                {
                    if (expando.ContainsKey(name))
                    { expando[name] = value; }
                    else
                    {
                        expando.Add(name, value);
                    }
                });
            }


            return expando;
        }
    }
}
