using System;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_loginHistoryService : ServiceBase<sys_loginHistory>
    {
    }
    
    public class sys_loginHistory : ModelBase
    {
        //[Identity]
        //[PrimaryKey]
        //public int ID { get; set; }

        public string UserCode { get; set; }

        public string UserName { get; set; }

        public string HostName { get; set; }

        public string HostIP { get; set; }

        public string LoginCity { get; set; }

        public DateTime? LoginDate { get; set; }
    }
}
