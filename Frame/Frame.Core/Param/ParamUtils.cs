using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Text;

namespace Frame.Core
{
    public class ParamUtils
    {
        private static readonly ConcurrentDictionary<Enum, string> CachedCpEnum =
            new ConcurrentDictionary<Enum, string>();

        public static string GetEnumDescription(Enum enumSubitem)
        {
            var description = CachedCpEnum.GetOrAdd(enumSubitem, _GetEnumDesc);
            return description;
        }

        private static string _GetEnumDesc(Enum enumSubitem)
        {
            var fieldinfo = enumSubitem.GetType().GetField(enumSubitem.ToString());
            var objs = fieldinfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (objs.Length == 0)
                return enumSubitem.ToString();

            var da = (DescriptionAttribute)objs[0];
            return da.Description;
        }

        public static string GetWhereSql(List<ParamWhere> where)
        {
            var whereSql = new StringBuilder();
            for (var index = 0; index < where.Count; index++)
            {
                var item = where[index];
                if (index == 0)
                {
                    whereSql.AppendFormat(" {0} ", item.Compare(item.Data));
                }
                else
                {
                    whereSql.AppendFormat("{0} {1} ", item.Data.AndOr, item.Compare(item.Data));
                }
            }

            return whereSql.ToString();
        }
    }
}
