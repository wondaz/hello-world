using System;
using System.Collections.Generic;
using Frame.Core;
using Newtonsoft.Json.Linq;
using FluentData;
using Web.FourS.Areas.Sys4S.Models;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class SettlementService : ServiceBase<S_Balance>
    {
        public dynamic GetWorkOrderHead(string id)
        {
            if (id == "0" || id == "")
            {
                return GetNewModel(new { });
            }

            if (id.StartsWith("B", StringComparison.CurrentCultureIgnoreCase))
            {
                return db.Sql(string.Format("select d.DispatchCode,b.* from S_Balance b inner join S_Dispatch d on b.DispatchID=d.DispatchID WHERE b.BalanceID={0} AND b.CorpID={1}", id.Substring(1, id.Length - 1), FormsAuth.UserData.CorpID)).QuerySingle<dynamic>();
            }

            //从维修派工单转为“结算单”
            var headInfo = GetDispatchDetail(id, 0);
            if (headInfo != null && headInfo.Count == 1)
            {
                return Extend(headInfo[0], new
                {
                    BalanceCode = NewKey.DateFlowCode(db, "维修结算", "WXJS"),
                    InputName = FormsAuth.UserData.UserName,
                    InputTime = DateTime.Now.ToString(),
                    BillState = "0",
                    AuditName = "",
                    AuditTime = ""
                });
            }

            return null;
        }

        public List<dynamic> GetDispatchDetail(string id, int index)
        {
            if (id == null) return null;
            var list = db.StoredProcedure("SP_Dispatch_Query")
                .Parameter("DispatchID", id)
                .Parameter("Index", index).Parameter("CorpID", FormsAuth.UserData.CorpID)
                .QueryMany<dynamic>();
            return list;
        }

        public dynamic GetMoneySum(string dispatchid)
        {
            return db.Sql(string.Format(@"
select max(cash) as AccountReceivable,max(insurance) as InsuranceSum,max(claim) as CounterclaimSum,max(loss) as BosomSum
from (select case when t.type='cash' then SUM(isnull(fee,0)) else 0 END AS cash,
		case when t.type='insurance' then SUM(isnull(fee,0)) else 0 end as  insurance,
		case when t.type='claim' then SUM(isnull(fee,0)) else 0 end as  claim,
		case when t.type='loss' then SUM(isnull(fee,0)) else 0 end as loss
	 from (SELECT AccountType type,ManHourFee as fee FROM [dbo].[S_DisItemDetail] WHERE DispatchID={0} AND CorpID={1}
		UNION ALL SELECT AccountType,PartFee FROM [dbo].[S_DisPartDetail] WHERE DispatchID={0} AND CorpID={1}
		UNION ALL SELECT AccountType,AppendFee FROM [dbo].[S_DisAppendDetail] WHERE DispatchID={0} AND CorpID={1}
) t group by  t.type) m
", dispatchid, FormsAuth.UserData.CorpID)).QuerySingle<dynamic>();
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

        /// <summary>
        /// 保存维修结算单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic SaveBalance(JObject data)
        {
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            var balanceid = 0;
            Logger("编辑维修派工单", () =>
            {
                //主表
                var user = FormsAuth.UserData;
                var head = data["head"];
                balanceid = Convert.ToInt32(head["BalanceID"]);
                int dispatchID = Convert.ToInt32(head["DispatchID"]);
                var builder = dbContext.StoredProcedure("SP_Balance_Save")
                    .Parameter("BalanceID", balanceid)
                    .Parameter("BalanceCode", head["BalanceCode"] == null ? "" : head["BalanceCode"].ToString())
                    .Parameter("DispatchID", dispatchID)
                    .Parameter("EarningSum", head["EarningSum"] == null ? 0 : Convert.ToDecimal(head["EarningSum"]))
                    .Parameter("InsureCorp", head["InsureCorp"] == null ? "" : head["InsureCorp"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("UpdatePerson", user.UserName)
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                var returnVal = builder.ParameterValue<string>("Result").Split('#');
                balanceid = Convert.ToInt32(returnVal[0]);
                result = returnVal[1];
                if (result.ToLower() != "ok" || dispatchID < 0) return;

                //子表1-维修项目
                foreach (JObject row in (JArray)data["rows1"])
                {
                    builder = dbContext.StoredProcedure("SP_Balance_Detail_Edit")
                        .Parameter("BalanceID", balanceid)
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("SerialID", Convert.ToInt32(row["SerialID"]))
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("Agio", (row["Agio"] == null || row["Agio"].ToString() == "") ? 0 : Convert.ToDecimal(row["Agio"]))
                        .Parameter("UpdatePerson", user.UserName)
                        .Parameter("Flag", 1)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表2-维修备件
                foreach (JObject row in (JArray)data["rows2"])
                {
                    builder = dbContext.StoredProcedure("SP_Balance_Detail_Edit")
                        .Parameter("BalanceID", balanceid)
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("SerialID", Convert.ToInt32(row["SerialID"]))
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("Agio", (row["Agio"] == null || row["Agio"].ToString() == "") ? 0 : Convert.ToDecimal(row["Agio"]))
                        .Parameter("UpdatePerson", user.UserName)
                        .Parameter("Flag", 2)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表3-附加项目
                foreach (JObject row in (JArray)data["rows3"])
                {
                    builder = dbContext.StoredProcedure("SP_Balance_Detail_Edit")
                        .Parameter("BalanceID", balanceid)
                        .Parameter("CorpID", user.CorpID)
                        .Parameter("DispatchID", dispatchID)
                        .Parameter("SerialID", Convert.ToInt32(row["SerialID"]))
                        .Parameter("AccountType", row["AccountType"] == null ? "" : row["AccountType"].ToString())
                        .Parameter("Agio", 0)
                        .Parameter("UpdatePerson", user.UserName)
                        .Parameter("Flag", 3)
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //金额汇总               
                builder = dbContext.StoredProcedure("SP_Balance_Detail_Edit")
                    .Parameter("BalanceID", balanceid)
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("DispatchID", dispatchID)
                    .Parameter("SerialID", 0)
                    .Parameter("AccountType", "")
                    .Parameter("Agio", 0)
                    .Parameter("UpdatePerson", user.UserName)
                    .Parameter("Flag", 4)
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");

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

            return new { result, id = balanceid };
        }


        public string AuditSettlement(JObject data)
        {
            var result = "ok";
            var userInfo = FormsAuth.UserData;
            Logger("（反）审核维修结算单", () =>
           {
               var isPay = db.Sql(string.Format("select IsPay FROM S_Balance WHERE BalanceID={0} AND CorpID={1}", Convert.ToInt32(data["id"]), userInfo.CorpID)).QuerySingle<bool>();
               if (isPay)
               {
                   result = "已付款，不允许操作";
                   return;
               }

               var state = Convert.ToInt32(data["state"]);
               if (state == 1)//审核
               {
                   db.Sql(string.Format("update S_Balance set BillState=1,AuditName='{0}',AuditTime=getdate(),UpdateDate=getdate() where BalanceID={1} AND CorpID={2}", userInfo.UserName, Convert.ToInt32(data["id"]), userInfo.CorpID)).Execute();
               }
               else//反审核
               {
                   db.Sql(string.Format("update S_Balance set BillState=0,AuditName=null,AuditTime=null,UpdateDate=getdate(),updatePerson='{0}',Remark='反审核' where BalanceID={1} AND CorpID={2}", userInfo.UserName, Convert.ToInt32(data["id"]), userInfo.CorpID)).Execute();
               }

           }, ex =>
           {
               result = ex.Message;
           });
            return result;
        }

        public List<textValue> GetDispatchCodes()
        {
            var result = db.Sql("select DispatchID as value,DispatchCode as text from s_dispatch where BillState=1 AND CorpID=@0", FormsAuth.UserData.CorpID).QueryMany<textValue>();
            return result;
        }
    }

    public class S_Balance : ModelBase
    {
        public int BalanceID { get; set; }
        public string BalanceCode { get; set; }
        public string DispatchCode { get; set; }
        public int? DispatchID { get; set; }
        public int CorpID { get; set; }
        public string MeetAutoCode { get; set; }
        public string SignCode { get; set; }
        public string VIN { get; set; }
        public string EmpName { get; set; }
        public DateTime? EndTime { get; set; }
        public string BalanceName { get; set; }
        public DateTime? MeetAutoTime { get; set; }
        public string CustomerID { get; set; }
        public string ManHourFee { get; set; }
        public decimal? ManHourAgio { get; set; }
        public decimal? AgioManHourFee { get; set; }
        public decimal? CounterclaimSum { get; set; }
        public decimal? SparepartFee { get; set; }
        public decimal? SparepartAgio { get; set; }
        public decimal? AgioSparepartFee { get; set; }
        public decimal? BosomSum { get; set; }
        public decimal? AddonsFee { get; set; }
        public decimal? TaxForMerSum { get; set; }
        public decimal? Cess { get; set; }
        public decimal? InsuranceSum { get; set; }
        public decimal? CessSum { get; set; }
        public decimal? AccountReceivable { get; set; }
        public decimal? EarningSum { get; set; }
        public decimal? Amount { get; set; }
        public bool IsPay { get; set; }
        public string InputName { get; set; }
        public DateTime? InputTime { get; set; }
        public string AuditName { get; set; }
        public DateTime? AuditTime { get; set; }
        public string Remark { get; set; }
        public string InsureCorp { get; set; }
        public string BillState { get; set; }
        public int State { get; set; }
        public string TransmissionState { get; set; }
        public bool IsCounterclaim { get; set; }
        public string IsCounterclaimName { get; set; }
        public DateTime? IsCounterclaimDate { get; set; }
        public bool IsBosom { get; set; }
        public string IsBosomName { get; set; }
        public DateTime? IsBosomDate { get; set; }
        public bool IsInsurance { get; set; }
        public string IsInsuranceName { get; set; }
        public DateTime? IsInsuranceDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string UpdatePerson { get; set; }

        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public string BrandName { get; set; }
        public string SeriesName { get; set; }
        public string ModelName { get; set; }
        public string RepairName { get; set; }
        public string RepairTel { get; set; }
        public DateTime? HandTime { get; set; }
    }
}