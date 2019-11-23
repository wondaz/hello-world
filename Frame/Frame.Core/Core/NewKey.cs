using System;
using System.Collections.Generic;
using System.Linq;
using FluentData;
using Frame.Utils;

namespace Frame.Core
{
    //采番
    public class NewKey
    {
        public static string datetime()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
        }

        public static string GetGuidString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        //最大值加一
        public static string Maxplus(IDbContext db, string table, string field)
        {
            var sqlWhere = " where 1 = 1 ";
            var dbkey = APP.DbProvider == DbProviderEnum.SqlServer
                ? db.Sql(String.Format("select isnull(max({0}),0) from {1} {2}", field, table, sqlWhere)).QuerySingle<string>()
                : db.Sql(String.Format("select NVL(max({0}),0) from {1} {2}", field, table, sqlWhere)).QuerySingle<string>();
            var cachedKeys = GetCacheKey(table, field);
            var currentKey = MaxOfAllKey(cachedKeys, ZConvert.ToString(dbkey));
            var key = ZConvert.ToString(currentKey + 1);
            SetCacheKey(table, field, key);
            return key;
        }

        public static string maxplus(IDbContext db, string table, string field, string where = "")
        {
            var sqlWhere = "";
            if (!String.IsNullOrEmpty(@where))
            {
                sqlWhere += " where " + @where;
            }

            var dbkey = APP.DbProvider == DbProviderEnum.SqlServer
                ? db.Sql(String.Format("select isnull(max({0}),0) from {1} {2}", field, table, sqlWhere)).QuerySingle<string>()
                : db.Sql(String.Format("select NVL(max({0}),0) from {1} {2}", field, table, sqlWhere)).QuerySingle<string>();
            var cachedKeys = GetCacheKey(table, field);
            var currentKey = MaxOfAllKey(cachedKeys, ZConvert.ToString(dbkey));
            var key = ZConvert.ToString(currentKey + 1);
            SetCacheKey(table, field, key);
            return key;
        }

        //日期时间加上N位数字加一
        public static string Dateplus(IDbContext db, string table, string field, string datestringFormat, int numberLength)
        {
            var dbkey = db.Sql(String.Format("select isnull(max({0}),0) from {1}", field, table)).QuerySingle<string>();
            var mykey = DateTime.Now.ToString(datestringFormat) + String.Empty.PadLeft(numberLength, '0');
            var cachedKeys = GetCacheKey(table, field);
            var currentKey = MaxOfAllKey(cachedKeys, ZConvert.ToString(dbkey), mykey);
            var key = ZConvert.ToString(currentKey + 1);
            SetCacheKey(table, field, key);
            return key;
        }

        /// <summary>
        /// oracle - 获取流水号
        /// </summary>
        /// <param name="db"></param>
        /// <param name="billName"></param>
        /// <param name="billCode"></param>
        /// <returns></returns>
        public static string GetFlowCode(IDbContext db, string billName, string billCode)
        {
            var builder = db.StoredProcedure("PKG_COMMONFUNCTION.SP_GenerateFlowCode")
                .Parameter("SBILLNAME", billName)
                .Parameter("SPREFIXCODE", billCode)
                .ParameterOut("SBILLCODE", DataTypes.String, 50);
            builder.Execute();
            return builder.ParameterValue<string>("SBILLCODE");
        }

        /// <summary>
        /// sql server - 获取流水号
        /// </summary>
        /// <param name="db"></param>
        /// <param name="billName"></param>
        /// <param name="billCode"></param>
        /// <returns></returns>
        public static string DateFlowCode(IDbContext db, string billName,string billCode)
        {
            var flowCode = db.StoredProcedure("SP_GenerateFlowCode")
               .Parameter("BillName", billName)
               .Parameter("BillCode",billCode)
               .ParameterOut("FlowCode", DataTypes.String, 20)
               .Parameter("CorpID",FormsAuth.UserData.CorpID).QuerySingle<string>();
            return flowCode;
        }

        private static string GetCacheKey(string table, string field)
        {
            var tableKeys = GetTableKeys(table);
            return GetFieldKeys(tableKeys, field);
        }

        private static Dictionary<string, string> GetTableKeys(string table)
        {
            var tableKeys = ZCache.GetCache(String.Format("currentkey_{0}", table)) as Dictionary<string, string> ??
                            new Dictionary<string, string>();

            return tableKeys;
        }

        private static string GetFieldKeys(Dictionary<string, string> tableKeys, string field)
        {
            return tableKeys.ContainsKey(field) ? tableKeys[field] : "0"; ;
        }

        private static void SetCacheKey(string table, string field, string key)
        {
            var tableKeys = GetTableKeys(table);
            var fieldKeys = GetFieldKeys(tableKeys, field);
            tableKeys[field] = ZConvert.ToString(MaxOfAllKey(fieldKeys, key));
            ZCache.SetCache(String.Format("currentkey_{0}", table), tableKeys);
        }

        private static long MaxOfAllKey(string cachedKeys, params string[] otherKey)
        {
            var keys = new List<string> { cachedKeys };
            keys.AddRange(otherKey);
            var max = keys.Max<object>(x => ZConvert.To<long>(x));
            return max;
        }
    }
}
