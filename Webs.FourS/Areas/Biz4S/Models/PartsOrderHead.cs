using FluentData;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class PartsOrderService : ServiceBase<PartsOrderHead>
    {
        public dynamic GetPartsOrderHead(string orderCode)
        {
            var formData = db.Sql(string.Format(@"SELECT [SellOrderCode],S.[CorpID],S.[OriType],S.[OriCode],S.[SellType],S.[SellTime],
S.[CustomerID],C.[Address],S.[Seller],S.[TotalAmount],S.[InputTime],S.[InputName],S.[AuditName]
,S.[AuditTime],S.[BillState],S.[Remark],S.[State],S.[IsOut],S.[TransmissionState],S.[IsPay]
,S.[UpdatePerson],S.[UpdateDate],C.CustomerName,C.MobileTel
FROM [dbo].[P_SellOrderHead] s left join [B_Customer] c on s.customerID=C.customerID WHERE S.SellOrderCode='{0}' AND S.CorpID = {1}", orderCode, FormsAuth.UserData.CorpID)).QuerySingle<dynamic>();
            if (formData != null)
            {
                return formData;
            }

            return GetNewModel(new
            {
                SellOrderCode = NewKey.DateFlowCode(db, "备件销售单", "BJXS"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                CorpID = FormsAuth.UserData.CorpID,
                SellTime = DateTime.Now.ToShortDateString(),
                SellType = new CommonService().GetDictDefaultVal("PartsSaleType")                
            });
        }

        public string SavePartsHead(JObject data)
        {
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            Logger("编辑备件销售主表", () =>
            {
                var user = FormsAuth.UserData;
                var builder = dbContext.StoredProcedure("SP_SellOrderHead_Edit")
                    .Parameter("SellOrderCode", data["SellOrderCode"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("SellType", data["SellType"] == null ? "" : data["SellType"].ToString())
                    .Parameter("SellTime", Convert.ToDateTime(data["SellTime"]))
                    .Parameter("CustomerName", data["CustomerName"] == null ? "" : data["CustomerName"].ToString())
                    .Parameter("Address", data["Address"].ToString())
                    .Parameter("MobileTel", data["MobileTel"] == null ? "" : data["MobileTel"].ToString())
                    .Parameter("Seller", data["Seller"] == null ? "" : data["Seller"].ToString())
                    .Parameter("InputName", user.UserName)
                    .Parameter("Remark", data["Remark"] == null ? "" : data["Remark"].ToString())
                    .Parameter("State", data["State"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
                if (result == "ok")
                {
                    dbContext.Commit();
                }
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });

            return result;
        }

        public string SavePartsDetail(JArray data)
        {
            string result = "";
            var user = FormsAuth.UserData;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑备件销售子表", () =>
                {
                    foreach (JObject item in data)
                    {
                        var builder = db.StoredProcedure("SP_SellOrderDetail_Edit")
                            .Parameter("SellOrderCode", item["SellOrderCode"].ToString())
                            .Parameter("SparePartCode", item["SparePartCode"] == null ? "" : item["SparePartCode"].ToString())
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("Price", item["Price"] == null ? 0 : Convert.ToDecimal(item["Price"].ToString()))
                            .Parameter("Quantity", item["Quantity"] == null ? 1 : Convert.ToInt32(item["Quantity"].ToString()))
                            .Parameter("Remark", item["Remark"] == null ? "" : item["Remark"].ToString())
                            .Parameter("UpdatePerson", user.UserName)
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
                    }

                    if (result == "ok")
                    {
                        dbContext.Commit();
                    }

                }, e => dbContext.Rollback());
            }

            return result;
        }

        /// <summary>
        /// 删除备件销售订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeletePartsOrder(string id)
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除备件销售订单", () =>
            {
                dbContext.Sql("DELETE FROM P_SellOrderHead WHERE SellOrderCode = @0", id).Execute();
                dbContext.Sql("DELETE FROM P_SellOrderDetail WHERE SellOrderCode = @0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }

        /// <summary>
        /// 删除备件销售订单备件
        /// </summary>
        /// <param name="serialID"></param>
        /// <returns></returns>
        public string DeleteOrderParts(string serialID)
        {
            var result = "ok";
            Logger("删除备件销售订单", () =>
            {
                var builder = db.StoredProcedure("SP_SellOrderDetail_Delete")
                    .Parameter("SerialID", serialID)
                     .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
            }, e =>
            {
                result = e.Message;
            });

            return result;
        }

        public dynamic GetCustomerByPhone(string id)
        {
            return db.Sql("SELECT CustomerName,Address FROM B_Customer WHERE MobileTel = @0",id).QuerySingle<dynamic>();
        }

        /// <summary>
        /// 审核备件销售订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int AuditOrder(JObject data)
        {
            var result = db.Sql("UPDATE [P_SellOrderHead] SET BillState = @0,AuditName=@1,AuditTime=GETDATE(),UpdateDate=GETDATE() WHERE SellOrderCode = @2", data["status"].Value<int>(), FormsAuth.UserData.UserName, data["code"].Value<string>()).Execute();

            return result;
        }
    }

    public class PartsOrderHead : ModelBase
    {
        public string SellOrderCode { get; set; }
        public int CorpID { get; set; }
        public string OriType { get; set; }
        public string OriCode { get; set; }
        public string SellType { get; set; }
        public DateTime SellTime { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string MobileTel { get; set; }
        public string Seller { get; set; }
        public string TotalAmount { get; set; }
        public DateTime? InputTime { get; set; }
        public string InputName { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public int BillState { get; set; }
        public string Remark { get; set; }
        public int State { get; set; }
        public string IsOut { get; set; }
        public string TransmissionState { get; set; }
        public string IsPay { get; set; }
        public string UpdatePerson { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}