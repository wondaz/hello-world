using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Frame.Utils;

namespace Frame.Core
{
    public static class CpHelper
    {
        private static Dictionary<string, Func<WhereData, string>> _dictionary =
            new Dictionary<string, Func<WhereData, string>>();

        static CpHelper()
        {
            var methods = typeof(Cp).GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var item in methods)
            {
                var temp = item;
                _dictionary.Add(item.Name.ToLower(), data => (string)temp.Invoke(null, new object[] { data }));
            }
        }

        public static Func<WhereData, string> Parse(string str, Func<WhereData, string> defaultCp = null)
        {
            str = (str ?? string.Empty).ToLower();
            return _dictionary.ContainsKey(str) ? _dictionary[str] : (defaultCp ?? Cp.Equal);
        }
    }

    public partial class Cp
    {
        public static string Equal(WhereData data)
        {
            return SQL(data, "{0} =  '{1}'");
        }

        public static string UpperEqual(WhereData data)
        {
            return SQL(data, "{0} =  '{1}'").ToUpper();
        }

        public static string NotEqual(WhereData data)
        {
            return SQL(data, "{0} <> '{1}'");
        }

        public static string Greater(WhereData data)
        {
            return SQL(data, "{0} >  '{1}'");
        }

        public static string GreaterEqual(WhereData data)
        {
            return SQL(data, "{0} >= '{1}'");
        }

        public static string DtGreaterEqual(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} >= '{1}'"
                    : "{0} >= TO_DATE('{1}','yyyy-mm-dd hh24:mi:ss')");
        }
        public static string DtGreater(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} > '{1}'"
                    : "{0} > TO_DATE('{1}','yyyy-mm-dd hh24:mi:ss')");
        }
        public static string DtLessEqual(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} <= '{1} 23:59:59'"
                    : "{0} <= TO_DATE('{1} 23:59:59','yyyy-mm-dd hh24:mi:ss')");
        }

        public static string DtLess(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} < '{1} 23:59:59'"
                    : "{0} < TO_DATE('{1} 23:59:59','yyyy-mm-dd hh24:mi:ss')");
        }

        public static string Less(WhereData data)
        {
            return SQL(data, "{0} < '{1}'");
        }

        public static string LessEqual(WhereData data)
        {
            return SQL(data, "{0} <=  '{1}'");
        }

        public static string In(WhereData data)
        {
            return SQL(data, "{0} in ({1})");
        }

        public static string Like(WhereData data)
        {
            return SQL(data, "{0} like '%{1}%'");
        }

        public static string UpperLike(WhereData data)
        {
            return SQL(data, "{0} like UPPER('%{1}%')");
        }

        public static string LikePY(WhereData data)
        {
            return SQL(data, "{0} like '%{1}%' or [dbo].[fun_getPY]({0}) like N'%{1}%'");
        }

        public static string StartWith(WhereData data)
        {
            return SQL(data, "{0} like '{1}%'");
        }

        public static string StartWithPY(WhereData data)
        {
            return SQL(data, "{0} like '{1}%' or [dbo].[fun_getPY]({0}) like N'{1}%'");
        }

        public static string Custom(WhereData data)
        {
            if (data.Value == null || data.Value.ToString().Trim() == "")
            {
                return string.Format("{0} = ''", data.Column);
            }

            return string.Format((data.Extend[0]).ToString(), data.Value.ToString().Trim());
        }

        public static string EndWith(WhereData data)
        {
            return SQL(data, "{0} like '%{1}'");
        }

        public static string Between(WhereData data)
        {
            return SQL(data, "{0} between '{1}' AND '{0}'");
        }

        public static string DateRange(WhereData data)
        {
            return GetDateRangeSql(data, new[] { '到', ',', '，' });
        }

        public static string DateEqual(WhereData data)
        {
            var dateFormat = "{0} = TO_DATE('{1}','yyyy-mm-dd hh24:mi:ss')";
            if (APP.DbProvider == DbProviderEnum.SqlServer)
            {
                dateFormat = "{0}='{1}'";
            }
            return SQL(data, dateFormat);
        }

        public static string Map(WhereData data)
        {
            return SQL(data, "{0} in (select {0} from {0} where {0}='{1}')");
        }

        public static string Child(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} in (select ID from [dbo].[GetChild]('{0}','{1}'))"
                    : "{0} in (select COLUMN_VALUE from table(GetChild('{1}')))");
        }

        public static string MapChild(WhereData data)
        {
            return SQL(data,
                APP.DbProvider == DbProviderEnum.SqlServer
                    ? "{0} in (select {0} from {2} where {3} in (select ID from [dbo].[GetChild]('{4}','{1}')))"
                    : "{0} in (select {0} from {2} where {3} in (select COLUMN_VALUE from table(GetChild('{1}'))))");
        }

        private static string SQL(WhereData cp, string stringFormat)
        {
            var list = cp.Extend.ToList();
            list.Insert(0, cp.Column);
            list.Insert(1, cp.Value != null ? cp.Value.ToString().Trim() : null);
            return string.Format(stringFormat, list.ToArray());
        }

        private static string GetDateRangeSql(WhereData cp, char[] separator, bool ignoreEmpty = true)
        {
            var sSql = string.Empty;
            string fromDateStr;
            string toDateStr;
            if (APP.DbProvider == DbProviderEnum.Oracle)
            {
                fromDateStr = "{0} >= TO_DATE('{1}','yyyy-mm-dd')";
                toDateStr = "{0} <= TO_DATE('{1} 23:59:59','yyyy-mm-dd hh24:mi:ss')";
            }
            else
            {
                fromDateStr = "datediff(day,'{1}',{0}) >=0";
                toDateStr = "datediff(day,'{1}',{0})<=0";
            }

            var values = ZConvert.ToString(cp.Value).Split(separator);
            var startDate = values[0].Trim();
            var endDate = values.Length == 2 ? values[1].Trim() : startDate;

            if (!string.IsNullOrWhiteSpace(startDate) || !ignoreEmpty)
                sSql = string.Format(fromDateStr, cp.Column, startDate);

            if (!string.IsNullOrWhiteSpace(endDate) || !ignoreEmpty)
                sSql += (sSql.Length > 0 ? " AND " : string.Empty) + string.Format(toDateStr, cp.Column, endDate);

            return sSql;
        }
    }
}
