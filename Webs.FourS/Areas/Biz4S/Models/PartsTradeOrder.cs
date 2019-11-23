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
    public class PartsTradeOrderService : ServiceBase<PartsTradeOrder>
    {
        /// <summary>
        /// 获取备件采购主表信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetTradeOrderHead(string id)
        {
            var formData = db.Sql(string.Format(@"SELECT case a.BillState when 0 then '未审' when 1 then '已审' WHEN 2 THEN '已发货' ELSE '已入库' end BillStateName,a.* FROM P_TradeOrderHead a WHERE a.TradeOrderID={0}", id)).QuerySingle<dynamic>();
            if (formData != null)
            {
                return formData;
            }

            return GetNewModel(new
            {
                TradeOrderCode = NewKey.DateFlowCode(db, "备件采购订单", "BJCG"),
                TradeType = new CommonService().GetDictDefaultVal("PartsBuyType"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                PlanTime = DateTime.Now.ToShortDateString(),
                OrderMan = FormsAuth.UserData.UserName,
                BillState = -1,//-1表示新增
                BillStateName = "新订单"
            });
        }

        public List<dynamic> GetTradeOrderDetail(string id)
        {
            //修改
            List<dynamic> result = db.Sql(@"
SELECT
       B.SparePartName
      ,B.Spec
      ,B.Unit 
	  ,A.[SerialID]
      ,A.[TradeOrderID]
      ,A.[SparePartCode]
      ,A.[Price]
      ,A.[Quantity]
      ,A.[Amount]
      ,A.[OrderID]
      ,UpdateDate
FROM P_TradeOrderDetail A
     INNER JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
WHERE A.[TradeOrderID] = @0 AND A.CorpID=@1", id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
            return result;
        }

        public string DeleteBuyOrderParts(string pkid)
        {
            try
            {
                db.Sql("DELETE FROM P_TradeOrderDetail WHERE SerialID=@0 AND CorpID=@1", pkid, FormsAuth.UserData.CorpID).Execute();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 删除备件采购订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeletePartsBuyOrder(string id)
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除备件采购订单", () =>
            {
                dbContext.Sql("DELETE FROM P_TradeOrderDetail WHERE TradeOrderID = @0 AND CorpID=@1", id, FormsAuth.UserData.CorpID).Execute();
                dbContext.Sql("DELETE FROM P_TradeOrderHead WHERE TradeOrderID = @0 AND CorpID=@1", id, FormsAuth.UserData.CorpID).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }

        public dynamic SaveBuyOrder(JObject data)
        {
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            int TradeOrderID = 0;
            Logger("编辑备件采购订单", () =>
            {
                //主表
                var user = FormsAuth.UserData;
                var head = data["head"];
                var builder = dbContext.StoredProcedure("SP_TradeOrderHead_Edit")
                    .Parameter("TradeOrderID", Convert.ToInt32(head["TradeOrderID"]))
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("TradeOrderCode", head["TradeOrderCode"].ToString())
                    .Parameter("TradeType", head["TradeType"].ToString())
                    .Parameter("OrderMan", user.UserName)
                    .Parameter("PlanTime", Convert.ToDateTime(head["PlanTime"]))
                    .Parameter("Remark", head["Remark"] == null ? "" : head["Remark"].ToString())
                    .ParameterOut("Result", DataTypes.Int32);
                builder.Execute();
                TradeOrderID = builder.ParameterValue<int>("Result");
                if (TradeOrderID > 0)
                {
                    //子表
                    int orderid = 0;
                    foreach (JObject row in (JArray)data["rows"])
                    {
                        orderid++;
                        builder = db.StoredProcedure("SP_TradeOrderDetail_Edit")
                            .Parameter("SerialID", Convert.ToInt32(row["SerialID"]))
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("TradeOrderID", TradeOrderID)
                            .Parameter("SparePartCode", row["SparePartCode"].ToString())
                            .Parameter("Price", row["Price"] == null ? 0 : Convert.ToDecimal(row["Price"].ToString()))
                            .Parameter("Quantity", row["Quantity"] == null ? 1 : Convert.ToInt32(row["Quantity"].ToString()))
                            .Parameter("OrderID", orderid)
                            .Parameter("UpdatePerson", user.UserName)
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
                    }
                }

                if (result == "ok")
                {
                    dbContext.Commit();
                }
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });

            return new { result, TradeOrderID };
        }

        /// <summary>
        /// 审核备件采购订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int AuditBuyOrder(JObject data)
        {
            var result = db.Sql("UPDATE [P_TradeOrderHead] SET BillState = @0,AuditName=@1,AuditTime=GETDATE(),UpdateDate=GETDATE() WHERE TradeOrderID = @2", data["status"].Value<int>(), FormsAuth.UserData.UserName, data["id"].Value<int>()).Execute();

            return result;
        }
    }

    public class PartsTradeOrder : ModelBase
    {
        public int TradeOrderID { get; set; }
        public int CorpID { get; set; }
        public string TradeOrderCode { get; set; }
        public string OrderMan { get; set; }
        public string TradeType { get; set; }
        public string OriType { get; set; }
        public string OriCode { get; set; }
        public DateTime PlanTime { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InputTime { get; set; }
        public string InputName { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public int BillState { get; set; }
        public string BillStateName { get; set; }
        public string IsIn { get; set; }
        public string Remark { get; set; }

    }
}