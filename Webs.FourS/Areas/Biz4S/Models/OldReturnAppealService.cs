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
    public class OldReturnAppealService : ServiceBase<OldReturnAppeal>
    {
        public dynamic GetOldReturnAppeal(string Id) {

            string sqls = string.Format(@"select F_PKId,F_OldPartsAppealCode,F_OldPartsAppealTime,F_Status,B_CorpTel,
                                        F_AppealReason,B_CorpID,B_CorpCode,F_InputName from [dbo].[F_C_OldPartsAppeal]
                                        where F_PKId='{0}' and B_CorpID='{1}'"
                                        , Id, FormsAuth.UserData.CorpID);
            var parinfos = db.Sql(sqls).QuerySingle<dynamic>();
            if (parinfos != null)
            {
                return parinfos;
            }
            return GetNewModel(new
            {
                F_PKId = Guid.NewGuid().ToString(),
                F_OldPartsAppealCode = NewKey.DateFlowCode(db, "旧件申诉单号", "JJSSDH"),
                F_InputName = FormsAuth.UserData.UserName,
                B_CorpTel = FormsAuth.UserData.LinkPhone,
                B_CorpCode = FormsAuth.UserData.CorpCode,
                B_CorpID = FormsAuth.UserData.CorpID
            });
        }
        public dynamic PostOldReturnAppeal(JObject data) 
        {
            var head = data["head"];
            var dbContext = db.UseTransaction(true);
            var result = "ok";
            Logger("保存旧件申诉信息主表与子表", () =>{
                var builder = dbContext.StoredProcedure("SP_F_C_OldPartsAppeal_Edit")
                    .Parameter("F_PKId",head["F_PKId"].ToString())
                    .Parameter("F_OldPartsAppealCode", head["F_OldPartsAppealCode"].ToString())
                    .Parameter("B_CorpId",FormsAuth.UserData.CorpID.ToString())
                    .Parameter("F_OldPartsAppealTime", head["F_OldPartsAppealTime"].ToString())
                    .Parameter("F_InputName", head["F_InputName"].ToString())
                    .Parameter("F_Status", int.Parse(head["F_Status"].ToString()))
                    .Parameter("F_AppealReason", head["F_AppealReason"].ToString())
                    .Parameter("B_CorpTel", head["B_CorpTel"].ToString())
                    .ParameterOut("Result", DataTypes.String,200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
                if (result == "ok")
                {
                    foreach (JObject row in (JArray)data["rows"]["inserted"]) 
                    {
                        var builderlist = dbContext.StoredProcedure("SP_F_C_OldPartsAppealList_Edit")
                            .Parameter("F_PKId", row["F_PKId"].ToString() == "" ? Guid.NewGuid().ToString() : row["F_PKId"].ToString())
                            .Parameter("F_Mid", head["F_PKId"].ToString())
                            .Parameter("F_ClaimCode", row["F_ClaimCode"].ToString())
                            .Parameter("F_OldPartsCode", row["F_OldPartsCode"].ToString())
                            .Parameter("F_OldReturnTime", row["F_OldReturnTime"] == null ? DateTime.Now : Convert.ToDateTime(row["F_OldReturnTime"].ToString()))
                            .Parameter("P_PartCode", row["P_PartCode"].ToString())
                            .Parameter("P_PartName", row["P_PartName"].ToString())
                            .Parameter("F_ClaimPrice", row["F_ClaimPrice"].ToString())
                            .Parameter("F_Number", row["F_Number"].ToString())
                            .Parameter("F_CheckedName", row["CheckedName"].ToString())
                            .ParameterOut("Result", DataTypes.String, 200);  
                            builderlist.Execute();
                            result = builderlist.ParameterValue<string>("Result");
                        if(result !="ok")
                        {
                            dbContext.Rollback();
                            return;
                        }
                    }
                    dbContext.Commit();
                }
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });
            return result;
        }
        /// <summary>
        /// 删除申诉信息子表数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic DeleteOldReturnAppeal(JObject id)
        {
            string sql = string.Format(@"delete from F_C_OldPartsAppealList where F_PKId='{0}'", id["id"].ToString());
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除旧件申诉子表信息", () =>
            {
                dbContext.Sql(sql).Execute();
                db.Commit();

            }, e => {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
        /// <summary>
        /// 删除旧件申诉主表和子表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic DeleteOldReturnAppeal_List(JObject id) 
        {
            string sql = string.Format(@"delete from F_C_OldPartsAppealList where F_Mid='{0}' 
                                       delete from F_C_OldPartsAppeal where F_PKId='{0}'", id["id"].ToString());
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除旧件申诉主表和子表信息", () =>
            {
                dbContext.Sql(sql).Execute();
                db.Commit();

            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
    }
    public class OldReturnAppeal : ModelBase 
    {
        /// <summary>
        /// 旧件申诉单号
        /// </summary>
        public string F_OldPartsAppealCode { get; set; }
        /// <summary>
        /// 申诉时间
        /// </summary>
        public DateTime? F_OldPartsAppealTime { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public int F_Status { get; set; }
        /// <summary>
        /// 服务站电话
        /// </summary>
        public string B_CorpTel { get; set; }
        /// <summary>
        /// 申诉理由
        /// </summary>
        public string F_AppealReason { get; set; }
        /// <summary>
        /// 服务站ID
        /// </summary>
        public int B_CorpID { get; set; }
        /// <summary>
        /// 服务站编号
        /// </summary>
        public string B_CorpCode { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string F_InputName { get; set; }
    }
}