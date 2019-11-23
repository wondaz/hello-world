using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class AutoArchivesService : ServiceBase<AutoArchives>
    {
        public dynamic GetArchiveInfo(string id)
        {
            return db.Sql(string.Format(@"SELECT * FROM [dbo].[B_AutoArchives] WHERE VIN LIKE '%{0}%'", id)).QuerySingle<dynamic>();
        }

        public List<textValue> GetAutoBrand()
        {
            var result = db.Sql("SELECT A1.BrandID as value,A1.BrandName as text FROM B_AutoBrand A1 WHERE A1.State=1").QueryMany<textValue>();
            return result;
        }

        public dynamic GetAutoSerials(string id)
        {
            return db.Sql(string.Format(@"SELECT SeriesID AS value,SeriesName as text FROM [dbo].[B_AutoSeries] WHERE BrandID ={0}", id)).QueryMany<textValue>();
        }

        public dynamic GetAutoModels(string id)
        {
            return db.Sql(string.Format(@"SELECT ModelID AS value,ModelName as text FROM [dbo].[B_AutoModel] WHERE SeriesID ={0}", id)).QueryMany<textValue>();
        }        
    }

    public class AutoArchives : ModelBase
    {
        public int ID { get; set; }
        public string VIN { get; set; }
        //public string BrandID { get; set; }
        public string BrandName { get; set; }
        //public string SeriesID { get; set; }
        public string SeriesName { get; set; }
        //public string ModeID { get; set; }
        public string ModeName { get; set; }
        public string OutsideColor { get; set; }
        public string InsideColor { get; set; }
        public string EngineCode { get; set; }
        public string MeasureCode { get; set; }
        public string ProduceArea { get; set; }
        public decimal CostPrice { get; set; }
        public string CarState { get; set; }
        public string CarType { get; set; }
        public DateTime ProduceDate { get; set; }
        public string ProduceCorp { get; set; }
        public string AssembleMemo { get; set; }
        public string SignCode { get; set; }
        public string Remark { get; set; }
        public string InputDate { get; set; }
        public DateTime InputName { get; set; }
        public int State { get; set; }
        public int IsInStock { get; set; }
        public int IsPack { get; set; }
        public string TransmissionState { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateName { get; set; }
    }
}