using FluentData;
using Frame.Core;
using Frame.Core.Dict;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class CarInStockHeadService : ServiceBase<CarInStockHead>
    {
        public dynamic GetInStockHead(string id)
        {
            var formData = db.Sql(string.Format(@"SELECT '' AS CostPrice,* FROM A_InStockHead WHERE InStockID={0}", id)).QuerySingle<dynamic>();
            if (formData != null)
            {
                return formData;
            }

            return GetNewModel(new
            {
                InStockCode = NewKey.DateFlowCode(db, "整车入库单", "RK"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                InStockDate = DateTime.Now.ToShortDateString(),
                InstockType = new CodeService().DefaultValue("IntoStore"),//下拉框默认值
                StockID = new PartsStockInService().GetDefaultStock("XC")
            });
        }

        /// <summary>
        /// 保存整车采购入库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SaveCarIntoStock(JObject data)
        {
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_CarIntoStock")
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("InStockID", Convert.ToInt32(data["InStockID"]))
                    .Parameter("InStockCode", data["InStockCode"] == null ? "" : data["InStockCode"].ToString())
                    .Parameter("OriginCode", data["OriginCode"] == null ? "" : data["OriginCode"].ToString())
                    .Parameter("InstockType", data["InstockType"] == null ? "" : data["InstockType"].ToString())
                    .Parameter("InStockDate", Convert.ToDateTime(data["InStockDate"]))
                    .Parameter("StockID", data["StockID"] == null ? "" : data["StockID"].ToString())
                    .Parameter("InLocation", data["InLocation"] == null ? "" : data["InLocation"].ToString())
                    .Parameter("TransactMan", data["TransactMan"] == null ? "" : data["TransactMan"].ToString())
                    .Parameter("RemarkHead", data["RemarkHead"] == null ? "" : data["RemarkHead"].ToString())
                    .Parameter("VIN", data["VIN"].ToString())
                    .Parameter("InputName", user.UserName)
                    .Parameter("CostPrice", data["CostPrice"] == null ? 0 : Convert.ToDecimal(data["CostPrice"]))
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                var result = builder.ParameterValue<string>("Result");
                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }

    public class CarInStockHead : ModelBase
    {
        public int InStockID { get; set; }
        public string InStockCode { get; set; }
        public int CorpID { get; set; }
        public string VIN { get; set; }
        public string OriginCode { get; set; }
        public DateTime InStockDate { get; set; }
        public int? StockID { get; set; }
        public string Keeper { get; set; }
        public string TransactMan { get; set; }
        public string InstockType { get; set; }
        public string BillState { get; set; }
        public DateTime InputTime { get; set; }
        public string InputName { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public string Remark { get; set; }
        public int State { get; set; }
        public string TransmissionState { get; set; }
        public decimal? CostPrice { get; set; }
    }
}