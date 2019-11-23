using FluentData;
using System.Collections.Generic;

namespace Frame.Core.Dict
{
    public static class DataDict
    {
        private static readonly Dictionary<string, string> Dict = new Dictionary<string, string>();
        private static string _dbName = APP.DB_DEFAULT_CONN_NAME;
        public static string Format(string codeType, object codeValue)
        {
            if (string.IsNullOrEmpty(codeType) || codeValue == null || codeValue.ToString() == "")
            {
                return string.Empty;
            }

            var key = string.Format("{0}-{1}", codeType, codeValue);
            if (Dict.ContainsKey(key))
            {
                return Dict[key];
            }

            var codeText = new ServiceBase(_dbName).db.Sql(
                 "SELECT TOP 1 Text FROM [sys_code] where CodeType=@0 AND Value=@1", codeType, codeValue)
                 .QuerySingle<string>();

            if (!string.IsNullOrEmpty(codeText))
            {
                Dict.Add(key, codeText);
                return codeText;
            }

            return codeValue.ToString();
        }
    }
}
