using System;
using System.Collections.Generic;
using Frame.Core;
using Newtonsoft.Json.Linq;
using FluentData;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class WorkOrderService : ServiceBase<Dispatch>
    {
        public dynamic GetWorkOrderHead(string id)
        {
            var headInfo = GetDispatchDetail(id, 0);
            //var headInfo = db.Sql(@"SELECT s.* FROM S_Dispatch s WHERE s.[DispatchID]=@0 AND s.CorpID=@1", id, FormsAuth.UserData.CorpID).QuerySingle<dynamic>();
            if (headInfo != null && headInfo.Count == 1)
            {
                return Extend(GetNewModel(new
                {
                    CustomerName = "",
                    MobileTel = "",
                    SaleDate = "",
                    BrandName = "",
                    SeriesName = "",
                    ModelName = ""
                }), headInfo[0]);
            }

            var model = GetNewModel(new
            {
                EmpName = FormsAuth.UserData.UserCode,
                MeetAutoTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),//接车时间
                EstimateTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm"),//预计交车
                NextMaintainDate = DateTime.Now.AddMonths(6),//下次保养日期
                DispatchCode = NewKey.DateFlowCode(db, "维修工单", "WXGD"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString()
            });

            return model;
        }

        public List<dynamic> GetDispatchDetail(string id, int index)
        {
            var list = db.StoredProcedure("SP_Dispatch_Query")
                .Parameter("DispatchID", id)
                .Parameter("Index", index).Parameter("CorpID", FormsAuth.UserData.CorpID)
                .QueryMany<dynamic>();
            return list;
        }

        public dynamic GetAutoArchive(string signCode)
        {
            return db.Sql(string.Format(@"
SELECT A.SignCode, 
       A.VIN, 
       A.BrandName, 
       A.SeriesName, 
       A.ModelName, 
       C.CustomerName, 
       C.MobileTel, 
       CONVERT(VARCHAR(10),S.SaleDate,120) SaleDate
FROM B_AutoArchives A
     LEFT JOIN [A_SaleOrders] S ON A.VIN = S.VIN
     LEFT JOIN B_Customer C ON A.VIN = C.VIN
WHERE a.signCode LIKE '%{0}%';", signCode)).QuerySingle<dynamic>();
        }

        public List<dynamic> GetManhour(string manhourCode)
        {
            if (string.IsNullOrEmpty(manhourCode)) return null;
            manhourCode = manhourCode.Replace("'", "");
            var data = db.Sql(string.Format(@"
SELECT TOP (20) CONCAT([ManhourCode],'/',[ManhourDescribe],'/',Manhour,'/',[DispatchManhour]) as ManhourCode FROM [dbo].[S_Manhour]
WHERE (ManhourCode LIKE '%{0}%' OR [ManhourDescribe] LIKE '%{0}%') AND CorpID = {1};", manhourCode, FormsAuth.UserData.CorpID)).QueryMany<dynamic>();
            return data;
        }

        public dynamic SaveDispatch(JObject data)
        {
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            int dispatchID = 0;
            Logger("编辑维修派工单", () =>
            {
                //主表
                var user = FormsAuth.UserData;
                var head = data["head"];
                var builder = dbContext.StoredProcedure("SP_Dispatch_Edit")
                    .Parameter("DispatchID", Convert.ToInt32(head["DispatchID"]))
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("DispatchCode", head["DispatchCode"] == null ? "" : head["DispatchCode"].ToString())
                    .Parameter("EmpName", head["EmpName"] == null ? "" : head["EmpName"].ToString())
                    .Parameter("MeetAutoCode", head["MeetAutoCode"] == null ? "" : head["MeetAutoCode"].ToString())
                    .Parameter("MeetAutoTime", Convert.ToDateTime(head["MeetAutoTime"]))
                    .Parameter("EstimateTime", Convert.ToDateTime(head["EstimateTime"]))
                    .Parameter("RunDistance", head["RunDistance"] == null ? 0 : Convert.ToInt32(head["RunDistance"]))
                    .Parameter("RepairDescribe", head["RepairDescribe"] == null ? "" : head["RepairDescribe"].ToString())
                    .Parameter("VIN", head["VIN"] == null ? "" : head["VIN"].ToString())
                    .Parameter("SignCode", head["SignCode"] == null ? "" : head["SignCode"].ToString())
                    .Parameter("MobileTel", head["MobileTel"] == null ? "" : head["MobileTel"].ToString())
                    .Parameter("CustomerName", head["CustomerName"] == null ? "" : head["CustomerName"].ToString())
                    .Parameter("RepairName", head["RepairName"] == null ? "" : head["RepairName"].ToString())
                    .Parameter("RepairTel", head["RepairTel"] == null ? "" : head["RepairTel"].ToString())
                    .Parameter("NextMaintainDate", Convert.ToDateTime(head["NextMaintainDate"]))
                    .Parameter("NextMaintainDistance", head["NextMaintainDistance"] == null ? 0 : Convert.ToInt32(head["NextMaintainDistance"].ToString()))
                    .Parameter("Demo", head["Demo"] == null ? "" : head["Demo"].ToString())
                    .Parameter("UpdatePerson", user.UserName)
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                var returnVal = builder.ParameterValue<string>("Result").Split('#');
                dispatchID = Convert.ToInt32(returnVal[0]);
                result = returnVal[1];
                if (result.ToLower() != "ok" || dispatchID < 0) return;

                //子表1-维修项目
                foreach (JObject row in (JArray)data["rows1"])
                {
                    builder = dbContext.StoredProcedure("SP_Dispatch_Item_Edit")
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("ManhourCode", row["ManhourCode"] == null ? "" : row["ManhourCode"].ToString())
                        .Parameter("ManhourDescribe", row["ManhourDescribe"] == null ? "" : row["ManhourDescribe"].ToString())
                        .Parameter("ItemTypeID", row["ItemTypeID"] == null ? "" : row["ItemTypeID"].ToString())
                        .Parameter("BillTypeID", row["BillTypeID"] == null ? "" : row["BillTypeID"].ToString())
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("DispatchManhour", row["DispatchManhour"] == null ? 0 : Convert.ToDecimal(row["DispatchManhour"]))
                        .Parameter("ManHourFee", row["ManHourFee"] == null ? 0 : Convert.ToDecimal(row["ManHourFee"]))
                        .Parameter("ClassID", row["ClassID"] == null ? "" : row["ClassID"].ToString())
                        .Parameter("WorkShopID", row["WorkShopID"] == null ? "" : row["WorkShopID"].ToString())
                        .Parameter("Remark", row["Remark"] == null ? "" : row["Remark"].ToString())
                        .Parameter("UpdateName", user.UserName)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表2-维修备件
                foreach (JObject row in (JArray)data["rows2"])
                {
                    builder = dbContext.StoredProcedure("SP_Dispatch_Parts_Edit")
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("SparePartCode", row["SparePartCode"] == null ? "" : row["SparePartCode"].ToString())
                        .Parameter("Quantity", Convert.ToDecimal(row["Quantity"]))
                        .Parameter("Price", Convert.ToDecimal(row["Price"]))
                        .Parameter("ClassID", row["ClassID"] == null ? "" : row["ClassID"].ToString())
                        .Parameter("WorkShopID", row["WorkShopID"] == null ? "" : row["WorkShopID"].ToString())
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("UpdatePerson", user.UserName)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表3-附加项目
                foreach (JObject row in (JArray)data["rows3"])
                {
                    builder = dbContext.StoredProcedure("SP_Dispatch_Append_Edit")
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("AppendName", row["AppendName"] == null ? "" : row["AppendName"].ToString())
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("AppendFee", Convert.ToDecimal(row["AppendFee"]))
                        .Parameter("Remark", row["Remark"] == null ? "" : row["Remark"].ToString())
                        .Parameter("UpdatePerson", user.UserName)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表4-总检信息
                dbContext.Sql("DELETE FROM S_DisInspectDetail WHERE DispatchID = @0 AND CorpID = @1", dispatchID, user.CorpID).Execute();
                foreach (JObject row in (JArray)data["rows4"])
                {
                    builder = dbContext.StoredProcedure("SP_Dispatch_Inspect_Edit")
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("FinishTime", Convert.ToDateTime(row["FinishTime"]))
                        .Parameter("JerqueTime", Convert.ToDateTime(row["JerqueTime"]))
                        .Parameter("CensorName", row["CensorName"] == null ? "" : row["CensorName"].ToString())
                        .Parameter("JerqueVerdict", row["JerqueVerdict"] == null ? "" : row["JerqueVerdict"].ToString())
                        .Parameter("Remark", row["Remark"] == null ? "" : row["Remark"].ToString())
                        .Parameter("TestDriveName", row["TestDriveName"] == null ? "" : row["TestDriveName"].ToString())
                        .Parameter("UpdatePerson", user.UserName)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }
                if (result == "ok")
                {
                    dbContext.Commit();
                }
                else
                {
                    dbContext.Rollback();
                }

            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });

            return new { result, id = dispatchID };
        }

        public string DeleteDispatch(string id, int index)
        {
            var builder = db.StoredProcedure("SP_Dispatch_Delete")
                .Parameter("ID", Convert.ToInt32(id))
                .Parameter("Index", index)
                .Parameter("CorpID", FormsAuth.UserData.CorpID)
                .ParameterOut("Result", DataTypes.String, 200);
            builder.Execute();
            return builder.ParameterValue<string>("Result");
        }

        public string AuditDispatch(JObject data)
        {
            var result = "ok";
            var userInfo = FormsAuth.UserData;
            Logger("（反）审核维修派工单", () =>
           {
               var builder = db.StoredProcedure("SP_Dispatch_Audit")
                .Parameter("DispatchID", Convert.ToInt32(data["id"]))
                .Parameter("BillState", Convert.ToInt32(data["state"]))
                .Parameter("CorpID", userInfo.CorpID)
                .Parameter("UpdatePerson", userInfo.UserName)
                .ParameterOut("Result", DataTypes.String, 200);
               builder.Execute();
               result = builder.ParameterValue<string>("Result");
               //db.Sql("UPDATE S_Dispatch SET BillState = @0,AuditName=@1,AuditTime=GETDATE(),UpdateDate=GETDATE() WHERE DispatchID = @2", data["state"].Value<int>(), FormsAuth.UserData.UserName, data["id"].Value<int>()).Execute();
           }, ex =>
           {
               result = ex.Message;
           });
            return result;
        }

        public List<textValue> GetDispatchCodes()
        {
            var result = db.Sql("select DispatchID as value,DispatchCode as text from s_dispatch where BillState=1").QueryMany<textValue>();
            return result;
        }
    }

    public class Dispatch : ModelBase
    {
        public int DispatchID { get; set; }
        public int CorpID { get; set; }
        public string DispatchCode { get; set; }
        public string QuotationID { get; set; }
        public string EmpName { get; set; }
        public string MeetAutoCode { get; set; }
        public DateTime MeetAutoTime { get; set; }
        public DateTime EstimateTime { get; set; }
        public string Hand { get; set; }
        public DateTime? HandTime { get; set; }
        public decimal? RunDistance { get; set; }
        public string RepairDescribe { get; set; }
        public string VIN { get; set; }
        public string SignCode { get; set; }
        public string CustomerID { get; set; }
        public string RepairName { get; set; }
        public string RepairTel { get; set; }
        public string Address { get; set; }
        public decimal ManHourFee { get; set; }
        public decimal SparepartFee { get; set; }
        public decimal AddonsFee { get; set; }
        public decimal Amount { get; set; }
        public string Remark { get; set; }
        public string InputName { get; set; }
        public DateTime InputTime { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public DateTime NextMaintainDate { get; set; }
        public string NextMaintainDistance { get; set; }
        public string BillState { get; set; }
        public int State { get; set; }
        public string Demo { get; set; }
        public string TransmissionState { get; set; }
        public bool IsBalance { get; set; }
        public bool IsPay { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdatePerson { get; set; }
        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public DateTime? SaleDate { get; set; }
        public string BrandName { get; set; }
        public string SeriesName { get; set; }
        public string ModelName { get; set; }
    }
}