using FluentData;
using Frame.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class FreightService : ServiceBase<F_C_Cost>
    {
        public string DeleteFreight(string id, int flag)
        {
            var builder = db.StoredProcedure("SP_Freight_Delete")
                .Parameter("F_PKId", id)
                .Parameter("B_CorpID", FormsAuth.UserData.CorpID)
                .Parameter("Flag", flag)
                .ParameterOut("Result", DataTypes.String, 200);
            builder.Execute();
            return builder.ParameterValue<string>("Result");
        }

        public dynamic GetFreightHead(string id)
        {
            if (id == "0" || id == "")
            {
                return GetNewModel(new
                {
                    B_CorpTel = FormsAuth.UserData.LinkPhone,
                    F_CostCode = NewKey.DateFlowCode(db, "运费索赔", "YFSP"),
                    F_InputName = FormsAuth.UserData.UserName,
                    F_InputTime = DateTime.Now.ToShortDateString(),
                    F_Status = 0,
                    StatusName = "新单据"
                });
            }

            return db.Sql(string.Format("SELECT dbo.f_Getcodename('运费索赔单状态',t.F_Status) StatusName,t.* FROM F_C_COST t WHERE t.F_PKId='{0}'", id)).QuerySingle<dynamic>();
        }

        public List<dynamic> GetFreightList(string id, int index)
        {
            if (id == "0" || id == "")
            {
                id = Guid.NewGuid().ToString();
            }
            var list = db.StoredProcedure("SP_Freight_Query")
                .Parameter("F_PKId", id)
                .Parameter("Index", index)
                .Parameter("CorpID", FormsAuth.UserData.CorpID)
                .QueryMany<dynamic>();
            return list;
        }

        public dynamic SaveFreight(JObject data)
        {
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            var pkid = "";
            Logger("编辑运费索赔", () =>
            {
                //主表                
                var user = FormsAuth.UserData;
                var head = data["head"];
                pkid = head["F_PKId"].ToString();
                if (string.IsNullOrEmpty(pkid))
                {
                    pkid = Guid.NewGuid().ToString();
                }

                var builder = dbContext.StoredProcedure("SP_Freight_Edit")
                    .Parameter("F_PKId", pkid)
                    .Parameter("F_Status", head["F_Status"].ToString())
                    .Parameter("F_CostCode", head["F_CostCode"].ToString())
                    .Parameter("B_CorpID", user.CorpID)
                    .Parameter("B_CorpTel", head["B_CorpTel"] == null ? "" : head["B_CorpTel"].ToString())
                    .Parameter("F_InputName", user.UserName)
                    .Parameter("F_SumMoney", Convert.ToDecimal(head["F_SumMoney"]))
                    .Parameter("F_Remark", head["F_Remark"] == null ? "" : head["F_Remark"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");

                if (result.ToLower() != "ok") return;

                //子表1-旧件退回单
                foreach (JObject row in (JArray)data["rows1"])
                {
                    var detailid = row["F_PKId"].ToString();
                    if (detailid == "")
                    {
                        detailid = Guid.NewGuid().ToString();
                    }
                    builder = dbContext.StoredProcedure("SP_FreightList_Edit")
                        .Parameter("F_PKId", detailid)
                        .Parameter("F_MId", pkid)
                        .Parameter("F_OldId", row["F_OldId"].ToString())
                        .Parameter("F_OldCode", row["F_OldCode"].ToString())
                        .Parameter("F_OldTime", Convert.ToDateTime(row["F_OldTime"]))
                        .Parameter("F_CarryName", row["F_CarryName"] == null ? "" : row["F_CarryName"].ToString())
                        .Parameter("F_CarryNumber", row["F_CarryNumber"] == null ? "" : row["F_CarryNumber"].ToString())
                        .Parameter("F_Money", Convert.ToDecimal(row["F_Money"]))
                        .ParameterOut("Result", DataTypes.String, 200);
                    builder.Execute();
                    result = builder.ParameterValue<string>("Result");
                    if (result != "ok") return;
                }

                //子表2-附件
                foreach (JObject row in (JArray)data["rows2"])
                {
                    var attachid = row["F_PKId"].ToString();
                    if (attachid == "")
                    {
                        attachid = Guid.NewGuid().ToString();
                    }

                    builder = dbContext.StoredProcedure("SP_FreightAttach_Edit")
                        .Parameter("F_PKId", attachid)
                        .Parameter("F_MId", pkid)
                        .Parameter("F_AttaUrl", row["F_AttaUrl"] == null ? "" : row["F_AttaUrl"].ToString())
                        .Parameter("F_AttaCusUrl", row["F_AttaCusUrl"] == null ? "" : row["F_AttaCusUrl"].ToString())
                        .Parameter("F_AttaOriginName", row["F_AttaOriginName"] == null ? "" : row["F_AttaOriginName"].ToString())
                        .Parameter("F_AttaType", row["F_AttaType"] == null ? "" : row["F_AttaType"].ToString())
                        .Parameter("F_AttaSize",row["F_AttaSize"].ToString())
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

            return new { result, id = pkid };
        }
    }

    public class F_C_Cost : ModelBase
    {
        public string F_PKId { get; set; }
        public string F_CostCode { get; set; }
        public string F_CostName { get; set; }
        public string B_CorpID { get; set; }
        public string B_CorpCode { get; set; }
        public string B_CorpName { get; set; }
        public string B_CorpTel { get; set; }
        public DateTime? F_CostTime { get; set; }
        public int F_Status { get; set; }
        public int F_Transmission { get; set; }
        public int F_IsDelete { get; set; }
        public DateTime F_InputTime { get; set; }
        public string F_InputId { get; set; }
        public string F_InputName { get; set; }
        public string F_AuditRemark { get; set; }
        public DateTime? F_AuditTime { get; set; }
        public string F_AuditId { get; set; }
        public string F_AuditName { get; set; }
        public decimal? F_SumMoney { get; set; }
        public string F_Remark { get; set; }
        public string F_CostId { get; set; }
        public int F_FaultCode { get; set; }
        public string F_FaultName { get; set; }
        public DateTime? F_EndInputTime { get; set; }
        public DateTime? F_FirstInputTime { get; set; }
    }
}