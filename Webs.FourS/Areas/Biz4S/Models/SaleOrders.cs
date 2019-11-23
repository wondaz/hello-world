using FluentData;
using Frame.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class SaleOrdersService : ServiceBase<SaleOrders>
    {
        public dynamic GetSaleOrder(string id)
        {
            var projectInfo = db.Sql(string.Format(@"SELECT * FROM View_saleOrders WHERE SaleOrderID={0}", id)).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return projectInfo;
            }

            return GetNewModel(new
            {
                SaleDate = DateTime.Now,
                SaleOrderCode = NewKey.DateFlowCode(db, "整车销售单", "ZCXS"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                DeliverCarDate = DateTime.Now.ToShortDateString()
            });
        }

        /// <summary>
        /// 获取待入库的VIN下拉列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<dynamic> GetVIN(string id)
        {
            return db.Sql(string.Format(@"SELECT TOP 30 a.VIN FROM [dbo].[B_AutoArchives] a 
LEFT JOIN [dbo].[A_SaleOrders] s on a.VIN=s.VIN
WHERE a.corpID={0} AND s.SaleOrderID IS NULL AND a.VIN LIKE '%{1}%'", FormsAuth.UserData.CorpID, id)).QueryMany<dynamic>();
        }

        /// <summary>
        /// 在库的VIN（可销售出库）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<dynamic> GetSaleVIN(string id)
        {
            return db.Sql(string.Format(@"SELECT TOP 30 VIN FROM A_AutoStorage WHERE state=0 and corpid={0} AND VIN LIKE '%{1}%'", FormsAuth.UserData.CorpID, id)).QueryMany<dynamic>();
        }

        public List<dynamic> GetSignCode(string signCode)
        {
            return db.Sql(string.Format(@"SELECT TOP 30 SignCode FROM [B_AutoArchives] WHERE SignCode LIKE '%{0}%'", signCode)).QueryMany<dynamic>();
        }

        public dynamic PostSaleOrder(JObject data)
        {
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_CarSaleOrder_Edit")
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("SaleOrderID", Convert.ToInt32(data["SaleOrderID"]))
                    .Parameter("SaleOrderCode", data["SaleOrderCode"].ToString())
                    .Parameter("SaleMan", data["SaleMan"] == null ? "" : data["SaleMan"].ToString())
                    .Parameter("SaleDate", Convert.ToDateTime(data["SaleDate"]))
                    .Parameter("DeliverCarDate", Convert.ToDateTime(data["DeliverCarDate"]))
                    .Parameter("ContractNo", data["ContractNo"] == null ? "" : data["ContractNo"].ToString())
                    .Parameter("CustomerID", Convert.ToInt32(data["CustomerID"]))
                    .Parameter("CustomerName", data["CustomerName"] == null ? "" : data["CustomerName"].ToString())
                    .Parameter("MobileTel", data["MobileTel"] == null ? "" : data["MobileTel"].ToString())
                    .Parameter("Address", data["Address"] == null ? "" : data["Address"].ToString())
                    .Parameter("VIN", data["VIN"] == null ? "" : data["VIN"].ToString())
                    .Parameter("SalePrice", Convert.ToDecimal(data["SalePrice"]))
                    .Parameter("PreferentialPrice", Convert.ToDecimal(data["PreferentialPrice"]))
                    .Parameter("FeeTotal", Convert.ToDecimal(data["FeeTotal"]))
                    .Parameter("Subscription", Convert.ToDecimal(data["Subscription"]))
                    .Parameter("PayType", data["PayType"] == null ? "" : data["PayType"].ToString())
                    .Parameter("IsInstalment", data["IsInstalment"] == null ? 0 : Convert.ToInt32(data["IsInstalment"]))
                    .Parameter("BankID", data["BankID"] == null ? "" : data["BankID"].ToString())
                    .Parameter("CustomerFistPay", Convert.ToDecimal(data["CustomerFistPay"]))
                    .Parameter("BankPay", Convert.ToDecimal(data["BankPay"]))
                    .Parameter("ArrearageRatifier", data["ArrearageRatifier"] == null ? "" : data["ArrearageRatifier"].ToString())
                    .Parameter("Remark", data["Remark"].ToString())
                    .Parameter("InputName", user.UserName)
                    .Parameter("BrandID", Convert.ToInt32(data["BrandID"]))
                    .Parameter("SeriesID", Convert.ToInt32(data["SeriesID"]))
                    .Parameter("ModelID", Convert.ToInt32(data["ModelID"]))
                    .Parameter("OutsideColor", data["OutsideColor"] == null ? "" : data["OutsideColor"].ToString())
                    .Parameter("InsideColor", data["InsideColor"] == null ? "" : data["InsideColor"].ToString())
                    .Parameter("IsOrderCar", data["IsOrderCar"] == null ? 0 : Convert.ToInt32(data["IsOrderCar"]))
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                var result = builder.ParameterValue<string>("Result");
                var resultArray = result.Split('#');
                return new { result = true, orderid = resultArray[0], customerid = resultArray[1] };
            }
            catch (Exception ex)
            {
                return new { result = false, msg = ex.Message };
            }
        }

        /// <summary>
        /// 删除销售订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeleteSaleOrder(string id)
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除整车销售订单", () =>
            {
                dbContext.Sql("DELETE FROM A_SaleOrders WHERE SaleOrderID = @0", id).Execute();
                //dbContext.Sql("DELETE FROM B_Customer WHERE CustomerID = (SELECT CustomerID FROM A_SaleOrders WHERE SaleOrderID = @0)", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }

        /// <summary>
        /// 销售车辆出库
        /// </summary>
        /// <param name="dat"></param>
        /// <returns></returns>
        public string CarOutStock(JObject data)
        {
            var result = "ok";
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_CarSaleOutStock")
                    .Parameter("SaleOrderID", Convert.ToInt32(data["id"]))
                    .Parameter("VIN", data["vin"].ToString())
                    .Parameter("UpdatePerson", user.UserName)
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }


        public int AuditSaleOrder(JObject data)
        {
            var result = db.Sql("UPDATE [A_SaleOrders] SET BillState = @0,AuditName=@1,AuditTime=GETDATE(),UpdateDate=GETDATE() WHERE SaleOrderID = @2", data["status"].Value<int>(), FormsAuth.UserData.UserName, data["id"].Value<int>()).Execute();

            return result;
        }
    }

    public class SaleOrders : ModelBase
    {
        [PrimaryKey]
        public int SaleOrderID { get; set; }
        public string SaleOrderCode { get; set; }
        public string QuoteID { get; set; }
        public DateTime? SaleDate { get; set; }
        public string ContractNo { get; set; }
        public string SaleMan { get; set; }
        public string BillState { get; set; }
        public string DeliverCarPlace { get; set; }
        public DateTime? DeliverCarDate { get; set; }
        public int CustomerID { get; set; }

        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public string Address { get; set; }
        public string VIN { get; set; }
        public string EngineCode { get; set; }
        public string MeasureCode { get; set; }
        public string BrandID { get; set; }
        public string BrandName { get; set; }
        public string SeriesID { get; set; }
        public string SeriesName { get; set; }
        public string ModelID { get; set; }
        public string ModelName { get; set; }
        public string OutsideColor { get; set; }
        public string InsideColor { get; set; }
        public string PayType { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal PreferentialPrice { get; set; }
        public int IsInsurance { get; set; }
        public decimal InsuranceFee { get; set; }
        public int IsPart { get; set; }
        public decimal PartFee { get; set; }
        public int IsSign { get; set; }
        public decimal SignFee { get; set; }
        public decimal TransferFee { get; set; }
        public decimal? FeeTotal { get; set; }
        public int IsBespeakCar { get; set; }
        public decimal Subscription { get; set; }
        public int IsInstalment { get; set; }
        public decimal CustomerFistPay { get; set; }
        public string BankID { get; set; }
        //public string BankName { get; set; }
        public int IsOrderCar { get; set; }
        public decimal BankPay { get; set; }
        public int IsPay { get; set; }
        public int IsOut { get; set; }
        public string ArrearageRatifier { get; set; }
        public string InputTime { get; set; }
        public string InputName { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public string SaleType { get; set; }
        public int IsOld { get; set; }
        public string Remark { get; set; }
        public int State { get; set; }
        public string TransmissionState { get; set; }
    }
}