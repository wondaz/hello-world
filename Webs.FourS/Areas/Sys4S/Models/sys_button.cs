using System;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class sys_buttonService : ServiceBase<sys_button>
    {
        protected override bool OnBeforEditDetail(EditEventArgs arg)
        {
            var buttonCode = arg.row["ButtonCode"].ToString();

            if (arg.type == OptType.Del)
            {
                //É¾³ý½ÇÉ«²Ëµ¥°´Å¥Map£¬É¾³ý²Ëµ¥°´Å¥Map
                db.Sql(string.Format(@"delete sys_roleMenuButtonMap where ButtonCode = '{0}';delete sys_menuBtnMap where ButtonCode ='{0}' ", buttonCode)).Execute();
            }

            return base.OnBeforEditDetail(arg);
        }
    }
    
     public class sys_button : ModelBase
    {
        [PrimaryKey]
        public string ButtonCode { get; set; }
        public string ButtonName { get; set; }
        public int ButtonSeq { get; set; }
        public string Description { get; set; }
        public string ButtonIcon { get; set; }
        public string UpdatePerson { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int Selected { get; set; }
    }
}
