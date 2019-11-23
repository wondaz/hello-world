using FluentData;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class PartsStockInService : ServiceBase<ModelBase>
    {
        /// <summary>
        /// 获取采购入库单主表数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetStockInHead(string id)
        {
            //从导航菜单进入
            if (id == "0" || id.Trim() == "")
            {
                var form = new
                {
                    StockInID = 0,
                    OriCode = "",
                    StockInCode = "",
                    TradeType = "1",//采购入库
                    InTime = DateTime.Now.ToShortDateString(),
                    StockID = GetDefaultStock("BJ"),
                    CorpManager = "",
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

            if (id.Length > 4 && id.Substring(0, 4) == "BJCG")//采购单Code
            {
                //从采购单转入库单
                var form1 = db.Sql(string.Format(@"
SELECT S.TradeOrderCode AS OriCode,        
       S.TotalAmount
FROM [dbo].[P_TradeOrderHead] S
WHERE S.TradeOrderCode = '{0}' AND BillState IN (1,2)
      AND S.CorpID = {1}", id, FormsAuth.UserData.CorpID)).QuerySingle<dynamic>();

                if (form1 != null)
                {
                    var form2 = new
                    {
                        StockInID = 0,
                        StockInCode = NewKey.DateFlowCode(db, "备件入库单", "BJRK"),
                        InputName = FormsAuth.UserData.UserName,
                        InputTime = DateTime.Now.ToShortDateString(),
                        InTime = DateTime.Now.ToShortDateString(),
                        TradeType = "1",//采购入库
                        StockID = GetDefaultStock("BJ"),
                        CorpManager = "",
                        Remark = "",
                        AuditName = "",
                        AuditTime = "",
                        BillState = 0,
                        BillStateName = "新单据"
                    };

                    return Extend(form1, form2);
                }
            }

            //修改记录
            string field = "a.StockInID=@0";
            if (id.Length > 4 && id.Substring(0, 4) == "BJCG")
            {
                field = "a.OriCode=@0";
            }
            var headData = db.Sql(@"
SELECT TOP 1 
    a.StockInID,
    a.OriCode,
    a.StockInCode,
    a.TradeType,
    a.InTime,
    a.StockID,
    a.CorpManager,
    a.Remark,
    a.TotalAmount,
    a.BillState,
    CASE WHEN a.[BillState] = 1 THEN '已审' ELSE '未审' END AS BillStateName,
    a.[InputTime], 
    a.[InputName], 
    a.[AuditName], 
    a.[AuditTime]       
FROM P_StockInHead AS a 
WHERE " + field + "  AND a.CorpID = @1", id, FormsAuth.UserData.CorpID).QuerySingle<dynamic>();
            return headData;
        }

        /// <summary>
        /// 获取采购入库单子表数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<dynamic> GetStockInDetail(string id)
        {
            List<dynamic> list;
            if (id.Length > 4 && id.Substring(0, 4) == "BJCG")//采购单Code
            {
                //从采购单转入库单
                list = db.Sql(@"
SELECT  
       A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity,
       A.Quantity TradeQty, 
       A.Amount,
       '' Remark,
       0 AS SerialID,
       0 AS StockInID
FROM P_TradeOrderDetail A INNER JOIN P_TradeOrderHead C ON A.TradeOrderID = C.TradeOrderID 
     INNER JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
WHERE C.TradeOrderCode = @0 AND A.CorpID=@1 AND C.BillState IN (1,2)", id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
                if (list.Count > 0)
                {
                    return list;
                }
            }

            //修改记录
            string field = "A.StockInID=@0";
            if (id.Length > 4 && id.Substring(0, 4) == "BJCG")
            {
                field = "C.OriCode=@0";
            }
            string sql = @"
SELECT A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity, 
       A.TradeQty,
       A.Amount, 
       A.Remark,
       A.SerialID,
       A.StockInID      
FROM P_StockInDetail A INNER JOIN P_StockInHead C ON A.StockInID = C.StockInID
INNER JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
WHERE " + field + " AND A.CorpID=@1";
            list = db.Sql(sql, id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
            return list;
        }

        /// <summary>
        /// 获取其他入库主表信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetStockOtherHead(string id)
        {
            //从导航菜单进入
            if (id == "0" || id.Trim() == "")
            {
                var form = new
                {
                    StockInID = 0,
                    OriCode = "",
                    StockInCode = NewKey.DateFlowCode(db, "备件入库单", "BJRK"),
                    TradeType = "6",//其他入库
                    InTime = DateTime.Now.ToShortDateString(),
                    StockID = GetDefaultStock("BJ"),
                    CorpManager = "",
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
            var headData = db.Sql(@"
SELECT TOP 1 
    a.StockInID,
    a.OriCode,
    a.StockInCode,
    a.TradeType,
    a.InTime,
    a.StockID,
    a.CorpManager,
    a.Remark,
    a.TotalAmount,
    a.BillState,
    CASE WHEN a.[BillState] = 1 THEN '已审' ELSE '未审' END AS BillStateName,
    a.[InputTime], 
    a.[InputName], 
    a.[AuditName], 
    a.[AuditTime]       
FROM P_StockInHead AS a 
WHERE a.StockInID=@0  AND a.CorpID = @1", id, FormsAuth.UserData.CorpID).QuerySingle<dynamic>();
            return headData;
        }


        /// <summary>
        /// 获取其他入库单子表数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<dynamic> GetStockOtherDetail(string id)
        {
            string sql = @"
SELECT A.SparePartCode, 
       B.SparePartName, 
       B.Spec, 
       B.Unit, 
       A.Price, 
       A.Quantity, 
       A.TradeQty,
       A.Amount, 
       A.SerialID,
       A.StockInID,
       A.Remark   
FROM P_StockInDetail A INNER JOIN P_StockInHead C ON A.StockInID = C.StockInID
INNER JOIN dbo.B_SparePart B ON A.SparePartCode = B.SparePartCode
WHERE A.StockInID=@0 AND A.CorpID=@1";
            return db.Sql(sql, id, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
        }


        /// <summary>
        /// 获取未生成入库单的采购单号
        /// </summary>
        /// <returns></returns>
        public List<textValue> GetTradeCodes()
        {
            return db.Sql(
@"SELECT TradeOrderCode AS text, 
       TradeOrderCode AS value
FROM P_TradeOrderHead
WHERE BillState IN(1,2)
      AND CorpID = @0
ORDER BY TradeOrderCode;", FormsAuth.UserData.CorpID).QueryMany<textValue>();
        }

        /// <summary>
        /// 获取仓库保管员
        /// </summary>
        /// <param name="stockid"></param>
        /// <returns></returns>
        public string GetStockKeeper(string stockid)
        {
            return db.Sql("SELECT Keeper FROM base_stock WHERE stockid=@0", stockid).QuerySingle<string>();
        }

        /// <summary>
        /// 获取仓库列表
        /// </summary>
        /// <param name="typeid"></param>
        /// <returns></returns>
        public List<textValue> GetStocks(string typeid)
        {
            return db.Sql("SELECT StockID as value,StockName as text FROM base_Stock WHERE CorpID IN(1, @0) AND StockTypeID = @1", FormsAuth.UserData.CorpID, typeid).QueryMany<textValue>();
        }

        public string GetDefaultStock(string typeid)
        {
         var stockID = db.Sql("SELECT TOP 1 StockID FROM base_Stock WHERE CorpID IN(1,@0) AND StockTypeID = @1 AND IsDefault=1",FormsAuth.UserData.CorpID, typeid).QuerySingle<string>();
            return stockID;
        }
        
        /// <summary>
        /// 保存备件入库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic SaveStockIn(JObject data)
        {
            string result = "";
            int stockInID = 0;
            var user = FormsAuth.UserData;
            var head = (JObject)data["head"];
            var rows = (JArray)data["rows"];
            IStoredProcedureBuilder builder;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑备件入库", () =>
                {
                    builder = dbContext.StoredProcedure("SP_StockInHead_Edit")
                    .Parameter("StockInID", Convert.ToInt32(head["StockInID"]))
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("StockInCode", head["StockInCode"].ToString())
                    .Parameter("TradeType", head["TradeType"].ToString())
                    .Parameter("OriType", "采购订单")
                    .Parameter("OriCode", head["OriCode"].ToString())
                    .Parameter("InTime", Convert.ToDateTime(head["InTime"].ToString()))
                    .Parameter("StockID", Convert.ToInt32(head["StockID"]))
                    .Parameter("CorpManager", head["CorpManager"] == null ? "" : head["CorpManager"].ToString())
                        //.Parameter("TotalAmount", head["TotalAmount"] == null ? 0 : Convert.ToDecimal(head["TotalAmount"]))
                    .Parameter("InputName", user.UserName)
                    .Parameter("Remark", head["Remark"] == null ? "" : head["Remark"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    stockInID = Convert.ToInt32(result.Split('#')[0]);
                    result = result.Split('#')[1];
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
                        builder = db.StoredProcedure("SP_StockInDetail_Edit")
                            //.Parameter("SerialID", Convert.ToInt32(item["SerialID"]))
                            .Parameter("StockInID", stockInID)
                            .Parameter("SparePartCode", item["SparePartCode"].ToString())
                            .Parameter("Price", Convert.ToDecimal(item["Price"].ToString()))
                            .Parameter("TradeQty", Convert.ToInt32(item["TradeQty"].ToString()))
                            .Parameter("Quantity", Convert.ToInt32(item["Quantity"].ToString()))
                            //.Parameter("Amount", Convert.ToDecimal(item["Amount"].ToString()))
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("OrderID", orderid)
                            .Parameter("UpdatePerson", user.UserName)
                            .Parameter("Remark", item["Remark"] == null ? "" : item["Remark"].ToString())
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

            return new { result, stockInID };
        }

        /// <summary>
        /// 审核备件入库单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string AuditStockInOrder(JObject data)
        {
            string result = "ok";
            var user = FormsAuth.UserData;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("备件入库单审核", () =>
                {
                    var builder = dbContext.StoredProcedure("[dbo].[SP_StockIn_Audit]")
                        .Parameter("StockInID", data["id"].Value<int>())
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("UpdatePerson", user.UserName)
                        .Parameter("BillState", Convert.ToInt32(data["state"]))
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

        public string DeleteStockInOrder(int id)
        {
            string result = "ok";
            var user = FormsAuth.UserData;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("备件入库单删除", () =>
                {
                    //todo 修改为入库单删除
                    var builder = dbContext.StoredProcedure("[dbo].[SP_StockInHead_Delete]")
                        .Parameter("StockInID", id)
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

        public string DeleteStockInParts(string id)
        {
            try
            {
                var billState = db.Sql(@"SELECT BillState FROM [P_StockInHead] H
INNER JOIN [P_StockInDetail] D ON H.StockInID = D.StockInID
WHERE D.SerialID = @0 AND D.CorpID=@1", id, FormsAuth.UserData.CorpID).QuerySingle<int>();
                if (billState > 0)
                {
                    return "单据已审核，不能删除。";
                }
                db.Sql("DELETE FROM [P_StockInDetail] WHERE SerialID=@0 AND CorpID=@1 AND BillState=0", id, FormsAuth.UserData.CorpID).Execute();
                return "ok";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}