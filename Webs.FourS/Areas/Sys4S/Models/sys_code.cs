using System;
using System.Collections.Generic;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class CodeService : ServiceBase<sys_code>
    {
        /// <summary>
        /// 获取数据字典默认值（用于下拉框默认值）
        /// </summary>
        /// <param name="sType"></param>
        /// <returns></returns>
        public string DefaultValue(string sType)
        {
            var result =
                db.Sql(@"SELECT TOP 1 Value FROM sys_code WHERE CodeType=@0 AND IsEnable=1 AND IsDefault=1", sType)
                    .QuerySingle<string>();

            return result ?? "";
        }
    }

    public class sys_code : ModelBase
    {
    }

    public class textValue : ModelBase
    {
        public string value { get; set; }

        public string text { get; set; }
    }
}
