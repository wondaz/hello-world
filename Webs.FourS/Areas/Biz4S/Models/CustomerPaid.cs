using FluentData;
using Frame.Core;
using Newtonsoft.Json.Linq;
using System;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class CustomerPaidService : ServiceBase<CustomerPaid>
    {
        public dynamic GetPayDetail(string id)
        {
            var formData = db.Sql(string.Format(@"SELECT * FROM A_PaymentDetail WHERE PaymentID={0}", id)).QuerySingle<dynamic>();
            if (formData != null)
            {
                return formData;
            }

            return GetNewModel(new
            {
                PaymentCode = NewKey.DateFlowCode(db, "付款单", "FKD"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                PaymentTime = DateTime.Now.ToShortDateString(),
                Cashier = FormsAuth.UserData.UserName,
                PaymentMode = new CommonService().GetDictDefaultVal("PaymentMode")
            });
        }

        /// <summary>
        /// 保存整车付款记录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SavePayItem(JObject data)
        {
            try
            {
                var user = FormsAuth.UserData;
                var amount = Convert.ToDecimal(data["Amount"]);//总额
                var payMoney = Convert.ToDecimal(data["PayMoney"]); //本次收款
                var sumPaid = Convert.ToDecimal(data["SumPaid"]);//已收总金额
                var builder = db.StoredProcedure("SP_PaymentEdit")
                    .Parameter("VIN", data["VIN"] == null ? "" : data["VIN"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("PaymentID", Convert.ToInt32(data["PaymentID"]))
                    .Parameter("PaymentCode", data["PaymentCode"].ToString())
                    .Parameter("PaymentTime", Convert.ToDateTime(data["PaymentTime"]))
                    .Parameter("Cashier", user.UserName)
                    .Parameter("PaymentType", data["PaymentType"].ToString())
                    .Parameter("OriBillCode", data["OriBillCode"].ToString())
                    //.Parameter("Bank", data["row"]["BankID"].ToString())
                    .Parameter("CustomerID", Convert.ToInt32(data["CustomerID"]))
                    .Parameter("PaymentMode", data["PaymentMode"].ToString())
                    .Parameter("Amount", amount)
                    .Parameter("PayMoney", payMoney)
                    .Parameter("Debt", amount - payMoney - sumPaid)
                    .Parameter("InvoiceCode", data["InvoiceCode"].ToString())
                    .Parameter("CheckCode", data["CheckCode"].ToString())
                    .Parameter("Remark", data["Remark"].ToString())
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

    public class CustomerPaid : ModelBase
    {
        public int PaymentID { get; set; }
        public int CustomerID { get; set; }
        public string PaymentType { get; set; }
        public string PaymentCode { get; set; }
        public DateTime PaymentTime { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// 本次付款金额
        /// </summary>
        public decimal PayMoney { get; set; }
        /// <summary>
        /// 欠款总额
        /// </summary>
        public decimal Debt { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public string PaymentMode { get; set; }
        public string InvoiceCode { get; set; }
        public string CheckCode { get; set; }
        public string Cashier { get; set; }
        public DateTime InputTime { get; set; }
        public string Remark { get; set; }

        /// <summary>
        /// 原始单据号
        /// </summary>
        public string OriBillCode { get; set; }
        public string VIN { get; set; }
    }
}