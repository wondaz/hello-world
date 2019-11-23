using System;
using System.Threading.Tasks;
using Frame.Core;
using System.Text;
using System.Collections.Generic;
using FluentData;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class VehicleReturnApplyServer : ServiceBase<VehicleReturnApply>
    {
        public List<dynamic> SaleOrderCodes(string id) 
        {
            string sql = string.Format(@"select TOP 30 SaleOrderCode from A_SaleOrders where SaleOrderCode like '%{0}%'", id);
            
            return db.Sql(sql).QueryMany<dynamic>();
        }
        public dynamic GetVehicleReturnApply(string id) 
        {
            string sql = string.Format(@"select * from Vehicle_ReturnApply where ID='{0}'",id);
            var parinfos = db.Sql(sql).QuerySingle<dynamic>();
            if (parinfos != null)
            {
                return parinfos;
            }
            return GetNewModel(new
            {
                Vehicle_ReturnCode = NewKey.DateFlowCode(db, "整车销退", "ZCXT"),
                InputName =  FormsAuth.UserData.UserName,
                InputTime =  DateTime.Now.ToString("yyyy-MM-dd")
            });
        }
        public dynamic GetArchiveInfos(string id) 
        {
            string sql = string.Format(@"select A.SalePrice,C.CustomerName,C.MobileTel,C.Email,C.Address,
            A.VIN,B.BrandName,B.SeriesName,B.ModelName,B.OutsideColor,B.InsideColor,B.EngineCode,
            B.MeasureCode from A_SaleOrders A inner join [B_AutoArchives] B on A.VIN=B.VIN
            inner join B_Customer C on C.VIN=B.VIN WHERE A.SaleOrderCode in
            (select SaleOrderCode from Vehicle_ReturnApply where id='{0}')", id);

            var parinfos = db.Sql(sql).QuerySingle<dynamic>();
            if (parinfos != null)
            {
                return parinfos;
            }
            return GetNewModel(new
            {
                CustomerName = "",
                MobileTel = "",
                Email = "",
                Address = "",
                VIN = "",
                BrandName = "",
                SeriesName = "",
                ModelName = "",
                OutsideColor = "",
                InsideColor = "",
                EngineCode = "",
                MeasureCode = ""
            });
        }
        public dynamic GetArchiveInfo(string id)
        {
            string sql = string.Format(@"select A.SalePrice,C.CustomerName,C.MobileTel,C.Email,C.Address,
            A.VIN,B.BrandName,B.SeriesName,B.ModelName,B.OutsideColor,B.InsideColor,B.EngineCode,
            B.MeasureCode from A_SaleOrders A inner join [B_AutoArchives] B on A.VIN=B.VIN
            inner join B_Customer C on C.VIN=B.VIN WHERE A.SaleOrderCode ='{0}'", id);

            var parinfos = db.Sql(sql).QuerySingle<dynamic>();
            if (parinfos != null)
            {
                return parinfos;
            }
            return GetNewModel(new
            {
                CustomerName = "",
                MobileTel = "",
                Email = "",
                Address = "",
                VIN = "",
                BrandName = "",
                SeriesName = "",
                ModelName = "",
                OutsideColor = "",
                InsideColor = "",
                EngineCode = "",
                MeasureCode = ""
            });
        }
        /// <summary>
        /// 提交整车销退信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic PostVehicleReturn(JObject data) {
            var head = data["head"];
            var dbContext = db.UseTransaction(true);
            var result = "ok";
            Logger("保存整车销退信息", () => {
                var builder = dbContext.StoredProcedure("SP_Vehicle_ReturnApply_Edit")
                    .Parameter("Vehicle_ReturnCode",head["Vehicle_ReturnCode"]==null ? "" : head["Vehicle_ReturnCode"].ToString())
                    .Parameter("VIN",head["VIN"]==null ? "" : head["VIN"].ToString())
                    .Parameter("SaleOrderCode", head["SaleOrderCode"] == null ? "" : head["SaleOrderCode"].ToString())
                    .Parameter("SaleBackDate",head["SaleBackDate"]==null ? DateTime.Now : Convert.ToDateTime(head["SaleBackDate"].ToString()))
                    .Parameter("Amount",head["Amount"]==null ? 0 : Convert.ToDecimal(head["Amount"].ToString()))
                    .Parameter("BackReason", head["BackReason"] == null ? "" : head["BackReason"].ToString())
                    .Parameter("Remark",head["Remark"]==null ? "" : head["Remark"].ToString())
                    .Parameter("CorpID", FormsAuth.UserData.CorpID)
                    .Parameter("InputName", FormsAuth.UserData.CorpName)
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
                dbContext.Commit();
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });
            return result;
        }
        public dynamic DeleteVehicleReturn(JObject data) {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);

            Logger("删除整车销退信息", () =>
            {
                string delelist = string.Format(" delete from Vehicle_ReturnApply where ID='{0}'", id);
                dbContext.Sql(delelist).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
        
    }
    public class VehicleReturnApply : ModelBase 
    {
        public int ID { get; set; }
        public string Vehicle_ReturnCode { get; set; }//车辆销退申请单号
        public string VIN { get; set; }//车架号
        public string SaleOrderCode { get; set; }//订单号
        public DateTime? SaleBackDate { get; set; }//申请日期
        public decimal Amount { get; set; }//销退金额
        public string BackReason { get; set; }//销退原因
        public string Remark { get; set; }//备注说明
        public int CorpID { get; set; }//服务站ID
        public DateTime? InputTime { get; set; }//制单时间
        public string InputName { get; set; }//制单人
        public string AuditName { get; set; }//审核人
        public DateTime? AuditTime { get; set; }//审核时间

        public string SalePrice { get; set; }
        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string BrandName { get; set; }
        public string SeriesName { get; set; }
        public string ModelName { get; set; }
        public string OutsideColor { get; set; }
        public string InsideColor { get; set; }
        public string EngineCode { get; set; }
        public string MeasureCode { get; set; }
    }
}