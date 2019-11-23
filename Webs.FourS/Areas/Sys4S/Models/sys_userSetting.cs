using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_userSettingService : ServiceBase<sys_userSetting>
    {
       
    }

    public class sys_userSetting : ModelBase
    {
        [Identity]
        [PrimaryKey]
        public int ID{ get; set; }
        public string UserCode{ get; set; }
        public string SettingCode{ get; set; }
        public string SettingName{ get; set; }
        public string SettingValue{ get; set; }
        public string Description { get; set; }
        public int CorpID { get; set; }

    }
}
