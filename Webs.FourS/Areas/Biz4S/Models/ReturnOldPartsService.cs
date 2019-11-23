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
    public class ReturnOldPartsService : ServiceBase<ReturnOldParts>
    {
        //旧件返厂初始化加载
        public dynamic GetReeturnOldParts(string Id) {
            string Selesql = string.Format(@"select * from [dbo].[F_C_OldPartsReturn] 
                where F_PKId='{0}' and B_CorpID='{1}'", Id, FormsAuth.UserData.CorpID
                );
            var parinfos = db.Sql(Selesql).QuerySingle<dynamic>();
            if(parinfos !=null){
                return parinfos; 
            }
            return GetNewModel(new 
            {
                F_OldPartsCode = NewKey.DateFlowCode(db, "旧件返厂单号", "JJFCDH"),
                F_PKId = Guid.NewGuid().ToString()
            });
        }
        //旧件返厂保存
        public dynamic PostReturnOldParts(JObject data)
        {
            var head = data["head"];
            var dbContext = db.UseTransaction(true);
            var result = "ok";
            int TradeOrderID = 0;
            Logger("编辑旧件返厂主表", () => {
                //主表
                var user = FormsAuth.UserData;
                var builder = dbContext.StoredProcedure("SP_F_C_OldPartsReturn_Edit")
                    .Parameter("F_PKId", head["F_PKId"] == null ? "" : head["F_PKId"].ToString())
                    .Parameter("F_OldPartsCode", head["F_OldPartsCode"] == null ? "" : head["F_OldPartsCode"].ToString())
                    .Parameter("F_DocType", head["F_DocType"] == null ? "" : head["F_DocType"].ToString())
                    .Parameter("F_OldReturnTime", head["F_OldReturnTime"] == null ? "" : head["F_OldReturnTime"].ToString())
                    .Parameter("F_Status", head["F_Status"] == null ? "" : head["F_Status"].ToString())
                    .Parameter("F_CarryName", head["F_CarryName"] == null ? "" : head["F_CarryName"].ToString())
                    .Parameter("F_Money", head["F_Money"] == null ? "" : head["F_Money"].ToString())
                    .Parameter("F_CarryNumber", head["F_CarryNumber"] == null ? "" : head["F_CarryNumber"].ToString())
                    .Parameter("F_AuditName", head["F_AuditName"] == null ? "" : head["F_AuditName"].ToString())
                    .Parameter("B_CorpID", FormsAuth.UserData.CorpID)
                    .Parameter("F_InputName", FormsAuth.UserData.UserName)
                    .Parameter("F_DeliveryNumber", head["F_DeliveryNumber"] == null ? "" : head["F_DeliveryNumber"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
                if (result == "ok")
                {
                    foreach (JObject row in (JArray)data["rows"]["inserted"]) //["inserted"]
                    {
                        var builderlist = dbContext.StoredProcedure("SP_F_C_OldPartsReturnList_Edit")
                            .Parameter("F_PKId", String.IsNullOrEmpty(row["ID"].ToString()) ? Guid.NewGuid().ToString() : row["ID"].ToString())
                            .Parameter("F_Mid", head["F_PKId"] == null ? "" : head["F_PKId"].ToString())
                            .Parameter("SPDCode", row["F_ClaimCode"].ToString())
                            //.Parameter("F_ClaimTime", row["F_ClaimTime"].ToString())
                            .Parameter("CarType", row["ModelName"].ToString())
                            .Parameter("VIN", row["VIN"].ToString())
                            .Parameter("UName", row["CustomerName"].ToString())
                            .Parameter("UTel", row["MobileTel"].ToString())
                            .Parameter("P_PartCode", row["P_PartCodeOld"].ToString())
                            .Parameter("P_PartName", row["P_PartNameOld"].ToString())
                            .Parameter("F_ClaimPrice", row["F_ClaimPrice"].ToString())
                            .Parameter("F_Number", row["F_Number"].ToString())
                            //.Parameter("F_Status", row["F_Status"].ToString())
                           // .Parameter("F_ClaimOrNot", row["F_ClaimOrNot"].ToString())
                            //.Parameter("F_ClaimRemarks", row["F_ClaimRemarks"].ToString())
                            .ParameterOut("Result", DataTypes.String, 200);
                            builderlist.Execute();
                            result = builderlist.ParameterValue<string>("Result");
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
                            .Parameter("F_MId", head["F_PKId"].ToString())
                            .Parameter("F_AttaUrl", row["F_AttaUrl"] == null ? "" : row["F_AttaUrl"].ToString())
                            .Parameter("F_AttaCusUrl", row["F_AttaCusUrl"] == null ? "" : row["F_AttaCusUrl"].ToString())
                            .Parameter("F_AttaOriginName", row["F_AttaOriginName"] == null ? "" : row["F_AttaOriginName"].ToString())
                            .Parameter("F_AttaType", row["F_AttaType"] == null ? "" : row["F_AttaType"].ToString())
                            .Parameter("F_AttaSize", row["F_AttaSize"].ToString())
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") return;
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
            return result;
        }

        public dynamic DeleteReturnOldPartsList(string ID) 
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);

            Logger("删除旧件返厂索赔单信息", () =>
            {
                string delelist = string.Format(" delete from F_C_OldPartsReturnList where F_PKId='{0}'", ID);
                dbContext.Sql(delelist).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }

        public dynamic DeleteReturnOldparts(string ID) 
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除旧件返厂单信息", () =>
                {
                    string deleteRetrnOldPart = string.Format(@" 
                    Delete FROM F_C_OldPartsReturnList where F_Mid 
                    in(SELECT top 1 F_PKId FROM F_C_OldPartsReturn where F_PKId='{0}' )
                    delete from F_C_OldPartsReturn where F_PKId='{0}' 
                    ", ID);
                    dbContext.Sql(deleteRetrnOldPart).Execute();
                    db.Commit();
                }, e =>
                {
                    dbContext.Rollback();
                    result = e.Message;
                });
            return result;
        }

        public dynamic deleteAttach(string id)
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除附件信息", () =>
            {
                string deleteAttach = string.Format(@" 
                    delete from F_C_Atta where F_PKId='{0}'
                    ", id);
                dbContext.Sql(deleteAttach).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
    }

    public class ReturnOldParts : ModelBase
    {
        public string F_PKId { get; set; }//主键ID
        public string F_OldPartsCode { get; set; }//返厂单号
        public string F_DocType { get; set; }//返厂类型
        public DateTime? F_OldReturnTime { get; set; }//退回时间
        public string F_Status { get; set; }//单据状态
        public string F_CarryName { get; set; }//承运商
        public string F_DeliveryNumber { get; set; }//物流单号
        public decimal? F_Money { get; set; }//旧件运费
        public int F_CarryNumber { get; set; }//装箱总件数
    }
}