/*************************************************************************
 * 文件名称 ：EditEventArgs.cs                          
 * 描述说明 ：编辑事件参数
 * 
 * 创建信息 :   on 2012-11-10
 * 修订信息 : modify by (person) on (date) for (reason)
 * 
 * 版权信息 : Copyright (c) 2013 
 **************************************************************************/

using FluentData;
using Newtonsoft.Json.Linq;

namespace Frame.Core
{
    public class EditEventArgs
    {
        public IDbContext db { get; set; }
        public JToken form { get; set; }
        //public dynamic formOld { get; set; }
        public JToken row { get; set; }
        //public dynamic rowOld { get; set; }
        public JToken list { get; set; }
        public RequestWrapper wrapper { get; set; }
        public OptType type { get; set; }
        public dynamic executeValue { get; set; }

        /// <summary>
        /// 插入数据时，返回的自增列的值
        /// </summary>
        public int IdentityVal { get; set; }

        public string TableName { get; set; }
        public EditEventArgs()
        {
            type = OptType.None;
        }
    }
}
