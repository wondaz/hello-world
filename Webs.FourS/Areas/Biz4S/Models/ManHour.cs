using System;
using System.Threading.Tasks;
using Frame.Core;
using System.Text;
using System.Collections.Generic;
using Web.FourS.Areas.Sys4S.Models;
using FluentData;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class ManHour : ServiceBase<ManHourModel>
    {
        public dynamic GetSaleManHour(string id)
        {

            var projectInfo = db.Sql(string.Format("select * from S_ManHour where ManHourID ='{0}'", id)).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return projectInfo;
            }
            return GetNewModel(new
            {
                ManHourCode = NewKey.DateFlowCode(db, "工时编码", "GSBM"),
                UpdateName = FormsAuth.UserData.UserName,
                UpdateDate = DateTime.Now.ToShortDateString()
            });
        }
        public dynamic PostManHour(JObject data) 
        {
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_ManHour_Update")
                    .Parameter("ManHourID", Convert.ToInt32(data["ManHourID"]))
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("ManHourCode", data["ManHourCode"] == null ? "" : data["ManHourCode"].ToString())
                    .Parameter("ManHourDescribe", data["ManHourDescribe"] == null ? "" : data["ManHourDescribe"].ToString())
                    .Parameter("ManHour", data["ManHour"] == null ? "" : data["ManHour"].ToString())
                    .Parameter("DispatchManHour", data["DispatchManHour"] == null ? "" : data["DispatchManHour"].ToString())
                    .Parameter("Remark", data["Remark"] == null ? "" : data["Remark"].ToString())
                    .Parameter("Taxis", data["Taxis"] == null ? "" : data["Taxis"].ToString())
                    .Parameter("State", data["State"] == null ? "" : data["State"].ToString())
                    .Parameter("TransmissionState", data["TransmissionState"] == null ? "" : data["TransmissionState"].ToString())
                    .Parameter("AddonsManHour", data["AddonsManHour"] == null ? "" : data["AddonsManHour"].ToString())
                    .Parameter("SeriesID", data["SeriesID"] == null ? "" : data["SeriesID"].ToString())
                    .Parameter("ModelID", data["ModelID"] == null ? "" : data["ModelID"].ToString())
                    .Parameter("MaintainType", data["MaintainType"] == null ? "" : data["MaintainType"].ToString())
                    .Parameter("BrandID", data["BrandID"] == null ? "" : data["BrandID"].ToString())
                    .Parameter("UpdateName", data["UpdateName"] == null ? "" : data["UpdateName"].ToString())
                    .ParameterOut("Result", DataTypes.Int32);
                builder.Execute();
                int ManHourID = builder.ParameterValue<int>("Result");
                return new { result = true, ManHourID };
            }
            catch (Exception ex)
            {
                return new { result = false, msg = ex.Message };
            }
        }
        public dynamic DeleteManHour(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除工时费", () =>
            {
                dbContext.Sql("DELETE FROM S_ManHour WHERE ManHourID = @0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }
    }

    public class ManHourModel : ModelBase 
    {
        [PrimaryKey]
        public int ManHourID { get; set; }
        public string ManHourCode { get; set; }//工时编码
        public string ManHourDescribe { get; set; }//工时描述
        public decimal ManHour { get; set; }//工时
        public decimal DispatchManHour { get; set; }//派工工时
        public string Remark { get; set; }//备注
        public int Taxis { get; set; }//排列顺序
        public int State { get; set; }
        public string TransmissionState { get; set; }
        public decimal AddonsManHour { get; set; }//附加工时
        public string SeriesID { get; set; }//车系
        public string ModelID { get; set; }//车型
        public string MaintainType { get; set; }//工时类型
        public string BrandID { get; set; }//品牌ID
        public int CorpID { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdateName { get; set; }
        public string MaintainTypeName { get; set; }//工时类型txt
        public string SeriesName { get; set; }//车系名
        public string ModelName { get; set; }//车型名
    }
}