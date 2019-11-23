using System.Collections.Generic;

namespace Frame.Core.Generator
{
    public class Table
    {
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 表的主键
        /// </summary>
        public List<string> TableKeys { get; set; }

        public List<TableSchema> TableSchemas { get; set; }

        public string Identity
        {
            get{return Util.GetIdentity(TableSchemas);}
        }
    }
}
