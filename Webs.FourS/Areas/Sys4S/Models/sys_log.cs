using System;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_logService : ServiceBase<sys_log>
    {
    }
    
    public class sys_log : ModelBase
    {
        [Identity]
        [PrimaryKey]
        public int ID { get; set; }

        public string UserCode { get; set; }

        public string UserName { get; set; }

        public string Position { get; set; }

        public string Target { get; set; }

        public string Type { get; set; }

        public string Message { get; set; }

        public DateTime? LogDate { get; set; }

    }
}
