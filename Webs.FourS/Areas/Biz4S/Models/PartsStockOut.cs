using FluentData;
using Frame.Core;
using Frame.Utils;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class PartsStockOutService : ServiceBase<ModelBase>
    {
        /// <summary>
        /// 获取销售出库单主表信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetStockOutHead(string id)
        {
            //从导航菜单进入
            if (id == "0" || id.Trim() == "")
            {
                var form = new
                {
                    StockOutID = new PartsStockInService().GetDefaultStock("BJ"),
                    OriCode = "",
                    StockOutCode = "",
                    TradeType = "1",//销售出库
                    OutTime = DateTime.Now.ToShortDateString(),
                    StockID = new PartsStockInService().GetDefaultStock("BJ"),
                    CorpManager = "",
                    CustomerID = 0,
                    CustomerName = "",
                    MobileTel = "",
                    Address = "",
                    Remark = "",
                    TotalAmount = "",
                    BillState = 0,
                    BillStateName = "",
                    InputName = FormsAuth.UserData.UserName,
                    InputTime = DateTime.Now.ToShortDateString(),
                    AuditName = "",
                    AuditTime = ""
                };

                return Extend(form, null);
            }

            //修改记录
            var headData = db.Sql(@"SELECT TOP 1 a.[StockOutID], 
       a.[OriCode], 
       a.[StockOutCode], 
       a.[TradeType], 
       a.[OutTime], 
       a.[StockID], 
       a.[CorpManager], 
       a.[CustomerID], 
       d.CustomerName, 
       d.MobileTel, 
       d.[Address], 
       a.[Remark], 
       a.[TotalAmount], 
       a.[BillState],
       CASE WHEN a.[BillState] = 1 THEN '已审' ELSE '未审' END AS BillStateName,
       a.[InputTime], 
       a.[InputName], 
       a.[AuditName], 
       a.[AuditTime]
FROM P_StockOutHead AS a
     LEFT JOIN B_Customer AS d ON a.CustomerID = d.CustomerID
WHERE (a.StockOutCode=@0 OR OriCode=@0) AND a.CorpID = @1", id, FormsAuth.UserData.CorpID).QuerySingle<dynamic>();
            if (headData != null)
            {
                return headData;
            }

            //从销售单转出库单
            var form1 = db.Sql(string.Format(@"
SELECT S.SellOrderCode AS OriCode,        
       S.[CustomerID], 
       C.CustomerName, 
       C.MobileTel, 
       C.Address, 
       S.TotalAmount
FROM [dbo].[P_SellOrderHead] S
     LEFT JOIN [B_Customer] c ON s.customerID = C.customerID
WHERE S.SellOrderCode = '{0}'
      AND S.CorpID = {1}", id, FormsAuth.UserData.CorpID)).QuerySingle<dynamic>();

            var form2 = new
            {
                StockOutID = 0,
                StockOutCode = NewKey.DateFlowCode(db, "备件出库单", "BJCK"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString(),
                OutTime = DateTime.Now.ToShortDateString(),
                TradeType = "1",//销售出库
                StockID = new PartsStockInService().GetDefaultStock("BJ"),
                CorpManager = "",
                Remark = "",
                AuditName = "",
                AuditTime = "",
                BillState = 0,
                BillStateName = ""
            };
            return Extend(form1, form2);
        }

        public List<dynamic> GetStockOutDetail(string id)
        {
            //修改
            List<dynamic> result = db.Sql(@"
SELECT A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity, 
       A.Amount, 
       A.PKID,
	   ISNULL(P.Quantity,0) as Stock
FROM P_StockAccountBook A
     LEFT JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
	 INNER JOIN P_StockOutHead S ON (S.StockOutCode = A.BillCode AND S.CorpID=A.CorpID)
     LEFT JOIN P_AssemStorage P ON (P.SparePartCode = A.SparePartCode AND P.CorpID=A.CorpID)
WHERE (A.BillCode = @0 OR S.OriCode = @0) AND A.CorpID=@1", id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
            if (result.Count > 0)
            {
                return result;
            }

            //从销售单转出库单
            result = db.Sql(@"
SELECT  
       A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity, 
       A.Amount,
       0 AS PKID,
	   ISNULL(P.Quantity,0) as Stock
FROM P_SellOrderDetail A
     LEFT JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
	 LEFT JOIN P_AssemStorage P ON (P.SparePartCode = A.SparePartCode AND P.CorpID=A.CorpID)
WHERE A.SellOrderCode = @0 AND A.CorpID=@1", id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
            return result;
        }

        /// <summary>
        /// 获取其他出库单主表信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetOtherOutHead(string id)
        {
            //从导航菜单进入
            if (id == "0" || id.Trim() == "")
            {
                var form = new
                {
                    StockOutID = 0,
                    OriCode = "",
                    StockOutCode = NewKey.DateFlowCode(db, "其他出库单", "QTCK"),
                    TradeType = "3",//其他出库
                    OutTime = DateTime.Now.ToShortDateString(),
                    StockID = new PartsStockInService().GetDefaultStock("BJ"),
                    CorpManager = "",
                    CustomerID = 0,
                    CustomerName = "",
                    MobileTel = "",
                    Address = "",
                    Remark = "",
                    TotalAmount = "",
                    BillState = 0,
                    BillStateName = "新单据",
                    InputName = FormsAuth.UserData.UserName,
                    InputTime = DateTime.Now.ToShortDateString(),
                    AuditName = "",
                    AuditTime = ""
                };

                return Extend(form, null);
            }

            //修改记录
            var headData = db.Sql(@"SELECT TOP 1 a.[StockOutID], 
       a.[OriCode], 
       a.[StockOutCode], 
       a.[TradeType], 
       a.[OutTime], 
       a.[StockID], 
       a.[CorpManager], 
       a.[CustomerID], 
       d.CustomerName, 
       d.MobileTel, 
       d.[Address], 
       a.[Remark], 
       a.[TotalAmount], 
       a.[BillState],
       CASE WHEN a.[BillState] = 1 THEN '已审' ELSE '未审' END AS BillStateName,
       a.[InputTime], 
       a.[InputName], 
       a.[AuditName], 
       a.[AuditTime]
FROM P_StockOutHead AS a
     LEFT JOIN B_Customer AS d ON a.CustomerID = d.CustomerID
WHERE (a.StockOutCode=@0) AND a.CorpID = @1", id, FormsAuth.UserData.CorpID).QuerySingle<dynamic>();
            return headData;
        }

        public List<dynamic> GetOtherOutDetail(string id)
        {
            //修改
            List<dynamic> result = db.Sql(@"
SELECT A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity, 
       A.Amount, 
       A.PKID,
	   ISNULL(P.Quantity,0) as Stock
FROM P_StockAccountBook A
     LEFT JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
	 INNER JOIN P_StockOutHead S ON (S.StockOutCode = A.BillCode AND S.CorpID=A.CorpID)
     LEFT JOIN P_AssemStorage P ON (P.SparePartCode = A.SparePartCode AND P.CorpID=A.CorpID)
WHERE (A.BillCode = @0) AND A.CorpID=@1", id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
            return result;
        }

        /// <summary>
        /// 获取未生成出库单的销售单号
        /// </summary>
        /// <returns></returns>
        public List<textValue> GetSellCodes()
        {
            return db.Sql("SELECT SellOrderCode as text,SellOrderCode as value FROM P_SellOrderHead WHERE BillState=1 AND IsPay=1 AND IsOut=0 AND CorpID=@0 ORDER BY SellOrderCode", FormsAuth.UserData.CorpID).QueryMany<textValue>();
        }

        /// <summary>
        /// 保存出库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SaveStockOut(JObject data)
        {
            string result = "";
            var user = FormsAuth.UserData;
            var head = (JObject)data["head"];
            var rows = (JArray)data["rows"];
            IStoredProcedureBuilder builder;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑备件出库", () =>
                {
                    builder = dbContext.StoredProcedure("SP_StockOutHead_Edit")
                    //.Parameter("StockOutID", Convert.ToInt32(head["StockOutID"]))
                    .Parameter("StockOutCode", head["StockOutCode"].ToString())
                    .Parameter("TradeType", head["TradeType"].ToString())
                    .Parameter("OriType", "销售订单")
                    .Parameter("OriCode", head["OriCode"].ToString())
                    .Parameter("OutTime", Convert.ToDateTime(head["OutTime"].ToString()))
                    //.Parameter("CustomerID", Convert.ToInt32(head["CustomerID"]))
                    .Parameter("CustomerName", head["CustomerName"].ToString())
                    .Parameter("MobileTel", head["MobileTel"].ToString())
                    .Parameter("Address", head["Address"].ToString())
                    .Parameter("StockID", Convert.ToInt32(head["StockID"]))
                    .Parameter("CorpManager", head["CorpManager"] == null ? "" : head["CorpManager"].ToString())
                    .Parameter("TotalAmount", head["TotalAmount"].ToString() == "" ? 0 : Convert.ToDecimal(head["TotalAmount"]))
                    .Parameter("InputName", user.UserName)
                    .Parameter("Remark", head["Remark"] == null ? "" : head["Remark"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok")
                    {
                        dbContext.Rollback();
                        return;
                    }

                    int orderid = 0;
                    foreach (var jToken in rows)
                    {
                        var item = (JObject)jToken;
                        orderid++;
                        builder = db.StoredProcedure("SP_StockAccountBook_Edit")
                            .Parameter("PKID", Convert.ToInt32(item["PKID"]))
                            .Parameter("CorpID", user.CorpID)
                            // .Parameter("StockOutID", Convert.ToInt32(item["StockOutID"]))
                            .Parameter("OutTime", Convert.ToDateTime(head["OutTime"].ToString()))
                            .Parameter("BillType", head["TradeType"].ToString())
                            .Parameter("BillCode", head["StockOutCode"].ToString())
                            .Parameter("SparePartCode", item["SparePartCode"].ToString())
                            .Parameter("Price", Convert.ToDecimal(item["Price"].ToString()))
                            .Parameter("Quantity", Convert.ToInt32(item["Quantity"].ToString()))
                            .Parameter("Amount", Convert.ToDecimal(item["Amount"].ToString()))
                            .Parameter("OrderID", orderid)
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
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
            }

            return result;
        }

        /// <summary>
        /// 审核出库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string AuditOrder(JObject data)
        {
            string result = "ok";
            var user = FormsAuth.UserData;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("备件出库审核", () =>
                {
                    var builder = dbContext.StoredProcedure("SP_StockOutHead_Audit")
                        .Parameter("StockOutCode", data["code"].Value<string>())
                        .Parameter("UpdatePerson", user.UserName)
                        .Parameter("BillState", Convert.ToInt32(data["state"]))
                        .Parameter("CorpID", user.CorpID)
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
            }

            return result;
        }

        public string DeleteStockOut(string outCode)
        {
            string result = "ok";
            var user = FormsAuth.UserData;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("备件出库单删除", () =>
                {
                    var builder = dbContext.StoredProcedure("SP_StockOutHead_Delete")
                        .Parameter("StockOutCode", outCode)
                        .Parameter("CorpID", user.CorpID)
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
            }

            return result;
        }

        public string DeleteStockOutParts(string pkid)
        {
            try
            {
                db.Sql("DELETE FROM P_StockAccountBook WHERE PKID=@0", pkid).Execute();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private dynamic Extend(object form1, object form2)
        {
            var expando = (IDictionary<string, object>)new ExpandoObject();
            EachHelper.EachObjectProperty(form1, (i, name, value) =>
            {
                expando.Add(name, value);
            });

            if (form2 != null)
            {
                EachHelper.EachObjectProperty(form2, (i, name, value) =>
                {
                    if (expando.ContainsKey(name))
                    { expando[name] = value; }
                    else
                    {
                        expando.Add(name, value);
                    }
                });
            }


            return expando;
        }
    }
}