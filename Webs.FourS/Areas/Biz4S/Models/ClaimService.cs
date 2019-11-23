using System;
using System.Threading.Tasks;
using Frame.Core;
using System.Text;
using System.Collections.Generic;
using Web.FourS.Areas.Sys4S.Models;
using FluentData;
using Newtonsoft.Json.Linq;



namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class ClaimService : ServiceBase<Claim>
    {
        public dynamic GetClaimEnty(string id) 
        {
            string sqls = string.Format(@"select A.*, 
                                        B.ModelID,B.EngineCode,C.CustomerName,C.MobileTel,
                                        C.SellerIdentityCard,C.Address,D.SaleDate from [F_C_Claim] A 
                                        left join [dbo].[B_AutoArchives] B on A.B_ChassisCode= B.VIN 
                                        left join [dbo].[B_Customer] C on A.B_ChassisCode=C.VIN 
                                        left join [dbo].[A_SaleOrders] D on D.VIN=A.B_ChassisCode WHERE A.ID='{0}' 
                                        and A.B_CorpID='{1}'"
                                        , id, FormsAuth.UserData.CorpID);
            var parinfos = db.Sql(sqls).QuerySingle<dynamic>();
            if(parinfos !=null)
            {
                return parinfos;
            }
            return GetNewModel(new
            {
                F_Status = 0,
                F_ClaimCode = NewKey.DateFlowCode(db, "索赔单号", "SPDH"),
                F_InputName = FormsAuth.UserData.UserName,
                B_CorpID = FormsAuth.UserData.CorpID
            });
            
        }
        public dynamic DeleteClaim(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            string sqls = string.Format(" delete from [F_C_Claim] where ID='{0}' and B_CorpID='{1}'", id, FormsAuth.UserData.CorpID);
            
            Logger("删除索赔信息", () =>
            {
                var parinfos = db.Sql(string.Format("select F_ClaimCode from F_C_Claim where ID='{0}'", id)).QuerySingle<Claim>();
                if (parinfos != null)
                {
                    string delelist = string.Format(" delete from F_ClaimPartList where F_ClaimCode='{0}'", parinfos.F_ClaimCode);
                    dbContext.Sql(sqls).Execute();
                    dbContext.Sql(delelist).Execute();
                    db.Commit();   
                }
            }, e => 
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
        /// <summary>
        /// 保存索赔信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic PostClaim(JObject data) 
        {
            var head=data["head"];
            var dbContext = db.UseTransaction(true);
            var result = "ok";
            Logger("保存索赔信息主表与子表", () =>
            {
                string F_PKId = Guid.NewGuid().ToString();
                string selesql = string.Format(@"select * from [dbo].[F_C_Claim] where F_ClaimCode='{0}'", head["F_ClaimCode"].ToString());
                var parinfos = dbContext.Sql(selesql).QuerySingle<dynamic>();
                if (parinfos == null)
                {
                    string insertsqlz = string.Format(@"insert into F_C_Claim(
                    F_PKId,B_ChassisCode,F_ClaimCode,
                    B_CorpTel,B_B_CarRunMile,B_LicensePlate,F_RepailName,F_RepailTel,F_FaultDesc,
                    F_FaultReason,F_SolveFault,B_CorpID,F_InputName,F_InputTime,F_Status,F_ClaimTime
                    ,F_otherMoney,F_outMoney,F_hourMoney,F_Total)values('{0}',
                    '{1}','{2}','{3}','{4}','{5}','{6}','7','{8}','{9}','{10}','{11}','{12}',GETDATE(),0,'{13}'
                    ,'{14}','{15}','{16}','{17}')"
                    , F_PKId, head["B_ChassisCode"].ToString(), head["F_ClaimCode"].ToString()
                    , head["B_CorpTel"].ToString(), head["B_B_CarRunMile"].ToString()
                    , head["B_LicensePlate"].ToString(), head["F_RepailName"].ToString()
                    , head["F_RepailTel"].ToString(), head["F_FaultDesc"].ToString()
                    , head["F_FaultReason"].ToString(), head["F_SolveFault"].ToString()
                    , FormsAuth.UserData.CorpID, FormsAuth.UserData.UserName, head["F_ClaimTime"].ToString()
                    , head["F_otherMoney"].ToString(), head["F_outMoney"].ToString()
                    , head["F_hourMoney"].ToString(), head["F_Total"].ToString());
                    dbContext.Sql(insertsqlz).Execute();
                    //保存子表
                    foreach (JObject row in (JArray)data["rows"]["inserted"]) 
                    {
                        string sqllist = string.Format(@"select * from F_ClaimPartList where F_ClaimPartID='{0}'"
                            , row["F_ClaimPartID"] == null ? "0" : row["F_ClaimPartID"].ToString());
                        var ClaimPartinfo = db.Sql(sqllist).QuerySingle<dynamic>();
                        if (ClaimPartinfo !=null) 
                        {
                            continue;
                        }
                        string insertlist = string.Format(@"INSERT INTO [dbo].[F_ClaimPartList]([F_ClaimCode]
                            ,[CorpID],[F_FaultCode],[F_FaultName],[B_DealerName]
                            ,[P_PartCodeOld],[P_PartCode],[P_PartNameOld],[P_PartName]
                            ,[F_ClaimPrice],[F_Number],[B_PartTypeName],[F_Total]
                            ,[F_IngredientCode],[F_IngredientName],[F_HourCode],[F_HourName]
                            ,[InputTime],[InputName])VALUES('{0}',{1},'{2}','{3}'
                            ,'{4}','{5}','{6}','{7}'
                            ,'{8}','{9}','{10}','{11}'
                            ,'{12}','{13}','{14}','{15}'
                            ,'{16}',{17},'{18}')"
                            , head["F_ClaimCode"].ToString(), FormsAuth.UserData.CorpID, row["F_FaultCode"].ToString()
                            , row["F_FaultName"].ToString(), row["B_DealerName"].ToString(), row["P_PartCodeOld"].ToString()
                            , row["P_PartCode"].ToString(), row["P_PartNameOld"].ToString(), row["P_PartName"].ToString()
                            , row["F_ClaimPrice"].ToString(), row["F_Number"].ToString(), row["B_PartTypeName"].ToString()
                            , row["F_Total"].ToString(), row["F_IngredientCode"].ToString(), row["F_IngredientName"].ToString()
                            , row["F_HourCode"].ToString(), row["F_HourName"].ToString()
                            , "GETDATE()", FormsAuth.UserData.UserName);
                        dbContext.Sql(insertlist).Execute();
                    }
                }
                else
                {
                    string sqlzbup = string.Format(@"update F_C_Claim set 
                    B_CorpTel='{1}',B_B_CarRunMile='{2}',B_LicensePlate='{3}',
                    F_RepailName='{4}',F_RepailTel='{5}',F_FaultDesc='{6}',
                    F_FaultReason='{7}',F_SolveFault='{8}',F_InputTime=GETDATE()
                    ,F_InputName='{9}',F_ClaimTime='{10}',F_otherMoney='{11}'
                    ,F_outMoney='{12}',F_hourMoney='{13}',F_Total ='{14}'
                    where F_ClaimCode='{0}'"
                    , head["F_ClaimCode"].ToString()
                    , head["B_CorpTel"].ToString(), head["B_B_CarRunMile"].ToString()
                    , head["B_LicensePlate"].ToString(), head["F_RepailName"].ToString()
                    , head["F_RepailTel"].ToString(), head["F_FaultDesc"].ToString()
                    , head["F_FaultReason"].ToString(), head["F_SolveFault"].ToString()
                    , FormsAuth.UserData.UserName, head["F_ClaimTime"].ToString()
                    , head["F_otherMoney"].ToString(), head["F_outMoney"].ToString()
                    , head["F_hourMoney"].ToString(), head["F_Total"].ToString());
                    dbContext.Sql(sqlzbup).Execute();
                    #region//保存子表
                    foreach (JObject row in (JArray)data["rows"]["updated"])
                    {
                        string sqluplist = string.Format(@"UPDATE [dbo].[F_ClaimPartList] 
                                           SET F_FaultName ='{0}',B_DealerName='{1}',
                                           P_PartCodeOld='{2}',P_PartCode='{3}',
                                           P_PartNameOld='{4}',P_PartName='{5}',
                                           F_ClaimPrice='{6}',F_Number='{7}',
                                           B_PartTypeName='{8}',F_Total='{9}',
                                           F_IngredientCode='{10}',F_IngredientName='{11}',
                                           F_HourCode='{12}',F_HourName='{13}',
                                           InputTime={14},InputName='{15}' where F_ClaimPartID='{16}'
                                           ", row["F_FaultName"].ToString(), row["B_DealerName"].ToString()
                                            , row["P_PartCodeOld"].ToString(), row["P_PartCode"].ToString()
                                            , row["P_PartNameOld"].ToString(), row["P_PartName"].ToString()
                                            , row["F_ClaimPrice"].ToString(), row["F_Number"].ToString()
                                            , row["B_PartTypeName"].ToString(), row["F_Total"].ToString()
                                            , row["F_IngredientCode"].ToString(), row["F_IngredientName"].ToString()
                                            , row["F_HourCode"].ToString(), row["F_HourName"]
                                            , "GETDATE()", FormsAuth.UserData.UserName, row["F_ClaimPartID"].ToString());
                        dbContext.Sql(sqluplist).Execute();
                    }
                    #endregion
                }
                string sqlup = string.Format(@"update B_Customer set MobileTel='{0}',Address='{1}'
                ,InputDate=Getdate(),InputName='{5}' where CustomerName='{2}' and SellerIdentityCard='{3}' 
                and VIN='{4}'", head["MobileTel"].ToString(), head["Address"].ToString()
                , head["CustomerName"].ToString(), head["SellerIdentityCard"].ToString()
                , head["B_ChassisCode"].ToString(), FormsAuth.UserData.UserName);
                dbContext.Sql(sqlup).Execute();
                db.Commit();
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });
            return result;
        }
        /// <summary>
        /// 提交索赔信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic PostSubmitClaim(JObject data) 
        {
            var head = data["head"];
            var dbContext = db.UseTransaction(true);
            var result = "ok";
            Logger("保存索赔信息主表与子表", () =>
            {
                string F_PKId = Guid.NewGuid().ToString();
                string selesql = string.Format(@"select * from [dbo].[F_C_Claim] where F_ClaimCode='{0}'", head["F_ClaimCode"].ToString());
                var parinfos = dbContext.Sql(selesql).QuerySingle<dynamic>();
                if (parinfos == null)
                {
                    string insertsqlz = string.Format(@"insert into F_C_Claim(
                    F_PKId,B_ChassisCode,F_ClaimCode,
                    B_CorpTel,B_B_CarRunMile,B_LicensePlate,F_RepailName,F_RepailTel,F_FaultDesc,
                    F_FaultReason,F_SolveFault,B_CorpID,F_InputName,F_InputTime,F_Status,F_ClaimTime
                    ,F_otherMoney,F_outMoney,F_hourMoney,F_Total)values('{0}',
                    '{1}','{2}','{3}','{4}','{5}','{6}','7','{8}','{9}','{10}','{11}','{12}',GETDATE(),1,'{13}'
                    ,'{14}','{15}','{16}','{17}')"
                    , F_PKId, head["B_ChassisCode"].ToString(), head["F_ClaimCode"].ToString()
                    , head["B_CorpTel"].ToString(), head["B_B_CarRunMile"].ToString()
                    , head["B_LicensePlate"].ToString(), head["F_RepailName"].ToString()
                    , head["F_RepailTel"].ToString(), head["F_FaultDesc"].ToString()
                    , head["F_FaultReason"].ToString(), head["F_SolveFault"].ToString()
                    , FormsAuth.UserData.CorpID, FormsAuth.UserData.UserName, head["F_ClaimTime"].ToString()
                    , head["F_otherMoney"].ToString(), head["F_outMoney"].ToString()
                    , head["F_hourMoney"].ToString(), head["F_Total"].ToString());
                    dbContext.Sql(insertsqlz).Execute();
                    //保存子表
                    foreach (JObject row in (JArray)data["rows"]["inserted"])
                    {
                        string sqllist = string.Format(@"select * from F_ClaimPartList where F_ClaimPartID='{0}'"
                            , row["F_ClaimPartID"] == null ? "0" : row["F_ClaimPartID"].ToString());
                        var ClaimPartinfo = db.Sql(sqllist).QuerySingle<dynamic>();
                        if (ClaimPartinfo != null)
                        {
                            continue;
                        }
                        string insertlist = string.Format(@"INSERT INTO [dbo].[F_ClaimPartList]([F_ClaimCode]
                            ,[CorpID],[F_FaultCode],[F_FaultName],[B_DealerName]
                            ,[P_PartCodeOld],[P_PartCode],[P_PartNameOld],[P_PartName]
                            ,[F_ClaimPrice],[F_Number],[B_PartTypeName],[F_Total]
                            ,[F_IngredientCode],[F_IngredientName],[F_HourCode],[F_HourName]
                            ,[InputTime],[InputName])VALUES('{0}',{1},'{2}','{3}'
                            ,'{4}','{5}','{6}','{7}'
                            ,'{8}','{9}','{10}','{11}'
                            ,'{12}','{13}','{14}','{15}'
                            ,'{16}',{17},'{18}')"
                            , head["F_ClaimCode"].ToString(), FormsAuth.UserData.CorpID, row["F_FaultCode"].ToString()
                            , row["F_FaultName"].ToString(), row["B_DealerName"].ToString(), row["P_PartCodeOld"].ToString()
                            , row["P_PartCode"].ToString(), row["P_PartNameOld"].ToString(), row["P_PartName"].ToString()
                            , row["F_ClaimPrice"].ToString(), row["F_Number"].ToString(), row["B_PartTypeName"].ToString()
                            , row["F_Total"].ToString(), row["F_IngredientCode"].ToString(), row["F_IngredientName"].ToString()
                            , row["F_HourCode"].ToString(), row["F_HourName"].ToString()
                            , "GETDATE()", FormsAuth.UserData.UserName);
                        dbContext.Sql(insertlist).Execute();
                    }
                }
                else
                {
                    string sqlzbup = string.Format(@"update F_C_Claim set 
                    B_CorpTel='{1}',B_B_CarRunMile='{2}',B_LicensePlate='{3}',
                    F_RepailName='{4}',F_RepailTel='{5}',F_FaultDesc='{6}',
                    F_FaultReason='{7}',F_SolveFault='{8}',F_InputTime=GETDATE()
                    ,F_InputName='{9}',F_ClaimTime='{10}',F_Status=1 
                    ,F_otherMoney='{11}',F_outMoney='{12}',F_hourMoney='{13}',F_Total ='{14}' 
                    where F_ClaimCode='{0}'"
                    , head["F_ClaimCode"].ToString()
                    , head["B_CorpTel"].ToString(), head["B_B_CarRunMile"].ToString()
                    , head["B_LicensePlate"].ToString(), head["F_RepailName"].ToString()
                    , head["F_RepailTel"].ToString(), head["F_FaultDesc"].ToString()
                    , head["F_FaultReason"].ToString(), head["F_SolveFault"].ToString()
                    , FormsAuth.UserData.UserName, head["F_ClaimTime"].ToString()
                    , head["F_otherMoney"].ToString(), head["F_outMoney"].ToString()
                    , head["F_hourMoney"].ToString(), head["F_Total"].ToString());
                    dbContext.Sql(sqlzbup).Execute();
                    #region//保存子表
                    foreach (JObject row in (JArray)data["rows"]["updated"])
                    {
                        string sqluplist = string.Format(@"UPDATE [dbo].[F_ClaimPartList] 
                                           SET F_FaultName ='{0}',B_DealerName='{1}',
                                           P_PartCodeOld='{2}',P_PartCode='{3}',
                                           P_PartNameOld='{4}',P_PartName='{5}',
                                           F_ClaimPrice='{6}',F_Number='{7}',
                                           B_PartTypeName='{8}',F_Total='{9}',
                                           F_IngredientCode='{10}',F_IngredientName='{11}',
                                           F_HourCode='{12}',F_HourName='{13}',
                                           InputTime={14},InputName='{15}' where F_ClaimPartID='{16}'
                                           ", row["F_FaultName"].ToString(), row["B_DealerName"].ToString()
                                            , row["P_PartCodeOld"].ToString(), row["P_PartCode"].ToString()
                                            , row["P_PartNameOld"].ToString(), row["P_PartName"].ToString()
                                            , row["F_ClaimPrice"].ToString(), row["F_Number"].ToString()
                                            , row["B_PartTypeName"].ToString(), row["F_Total"].ToString()
                                            , row["F_IngredientCode"].ToString(), row["F_IngredientName"].ToString()
                                            , row["F_HourCode"].ToString(), row["F_HourName"]
                                            , "GETDATE()", FormsAuth.UserData.UserName, row["F_ClaimPartID"].ToString());
                        dbContext.Sql(sqluplist).Execute();
                    }
                    #endregion
                }
                string sqlup = string.Format(@"update B_Customer set MobileTel='{0}',Address='{1}'
                ,InputDate=Getdate(),InputName='{5}' where CustomerName='{2}' and SellerIdentityCard='{3}' 
                and VIN='{4}'", head["MobileTel"].ToString(), head["Address"].ToString()
                , head["CustomerName"].ToString(), head["SellerIdentityCard"].ToString()
                , head["B_ChassisCode"].ToString(), FormsAuth.UserData.UserName);
                dbContext.Sql(sqlup).Execute();
                db.Commit();
            }, ex =>
            {
                dbContext.Rollback();
                result = ex.Message;
            });
            return result;
        }
        public dynamic GetClaimInfo(string id) 
        {
            string sql = string.Format(@"select B.ModelID,B.EngineCode,A.B_B_CarRunMile,A.B_LicensePlate, 
                                        A.F_Status,C.CustomerName,C.MobileTel,C.SellerIdentityCard,C.Address
                                        ,D.SaleDate,CONVERT(money,ISNULL(E.AddonsManHour,0.00)) as F_otherMoney,A.F_RepailName,A.F_RepailTel
                                        ,CONVERT(money,ISNULL(E.DispatchManHour,0.00)) as F_outMoney,'' AS F_partMoney,CONVERT(money,ISNULL(E.ManHour,0.00)) as F_hourMoney,
                                        CONVERT(money,ISNULL(E.ManHour,0.00))+CONVERT(money,ISNULL(E.DispatchManHour,0.00)) AS F_Total,A.F_FaultDesc,A.F_FaultReason,
                                        A.F_SolveFault,B.SignCode AS B_LicensePlate from B_AutoArchives B left join F_C_Claim A
                                        on A.B_ChassisCode=B.VIN left join B_Customer C on B.VIN=C.VIN 
                                        left join A_SaleOrders D on D.VIN=B.VIN 
                                        left join S_ManHour E on B.ModelID=E.ModelID and B.BrandID=E.BrandID
                                        WHERE B.VIN='{0}'", id);
            var result = db.Sql(sql).QuerySingle<dynamic>();
            return result;
        }
        public dynamic DeleteClaimPartList(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除索赔列表单", () =>
            {
                dbContext.Sql("delete from F_ClaimPartList where F_ClaimPartID = @0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
    }


    public class Claim : ModelBase
    {
        #region//索赔表单
        public string F_PKId { get; set; }//索赔主键ID
        public string F_ClaimCode { get; set; }//索赔单编码
        public string F_ClaimName { get; set; }//索赔单名称
        public string F_ClaimType { get; set; }//索赔单类型
        public string B_CorpID { get; set; }//销售公司ID
        public string B_CorpCode { get; set; }//服务站编码
        public string B_CorpName { get; set; }
        public string B_CorpTel { get; set; }//服务站电话
        public string B_LicensePlate { get; set; }//牌照
        public DateTime? F_ClaimTime { get; set; }//索赔日期
        public string F_FaultId { get; set; }
        
        public string F_FaultMeno { get; set; }
        public string B_ChassisCode { get; set; }//VIN码(车架号)
        public string B_B_CarRunMile { get; set; }//行驶里程公里
        public string F_RepailName { get; set; }//送修人
        public string F_RepailTel { get; set; }//送修人电话
        public string F_FaultDesc { get; set; }//故障现象描述
        public string F_FaultReason { get; set; }//故障原因
        public string F_SolveFault { get; set; }//处理意见
        public int F_Status { get; set; }//审核状态ID
        public int F_Transmission { get; set; }
        public int F_IsDelete { get; set; }
        public DateTime? F_InputTime { get; set; }//提交时间
        public string F_InputId { get; set; }
        public string F_InputName { get; set; }
        public string F_AuditRemark { get; set; }//汽车厂家审核意见
        public DateTime? F_AuditTime { get; set; }//审核时间
        public string F_AuditId { get; set; }
        public string F_AuditName { get; set; }//审核人
        public decimal? F_Total { get; set; }//总费用
        public string F_ClaimMeno { get; set; }
        public string F_SettleCode { get; set; }
        public decimal? F_partMoney { get; set; }//配件费用
        
        public decimal? F_otherMoney { get; set; }//其他工时费
        public decimal? F_outMoney { get; set; }//外出服务金额（元）
        public string F_ClaimId { get; set; }
        public string F_MajorCode { get; set; }
        public string F_OutserCode { get; set; }
        public string F_ReportCode { get; set; }
        public string F_MajorId { get; set; }
        public string F_OutserId { get; set; }
        public string F_ReportId { get; set; }
        public int F_IsPrint { get; set; }//是否大印
        public DateTime? F_EndInputTime { get; set; }
        public DateTime? F_FirstInputTime { get; set; }
        #endregion

        public string ModelID { get; set; }//车型ID
        public string EngineCode { get; set; }//发动机号
        public string SellerIdentityCard { get; set; }//身份证号
        public string CustomerName { get; set; }//客户姓名
        public string MobileTel { get; set; }//联系电话
        public string Address { get; set; }//联系地址
        public string SaleDate { get; set; }//销售日期,购车日

        public string F_FaultCode { get; set; }//故障编码
        public string F_FaultName { get; set; }//故障名称
        public string B_DealerName { get; set; }//配件供应商
        public string P_PartCodeOld { get; set; }//旧件编码
        public string P_PartCode { get; set; }//配件编码(新件)
        public string P_PartNameOld { get;set;}//旧件名称
        public string P_PartName { get; set; }//配件名称(新件)
        public string F_ClaimPrice { get; set; }//索赔单价
        public int F_Number { get; set; }//使用数量
        public string B_PartTypeName { get; set; }//配件属性
        public string F_IngredientCode { get; set; }//辅料编码
        public string F_IngredientName { get; set; }//辅料名称
        public string F_InPrice { get; set; }//辅料单价
        public string F_HourType { get; set; }//工时类型
        public string F_HourCode { get; set; }//工时编码
        public string F_HourName { get; set; }//工时名称
        public decimal? F_hourMoney { get; set; }//总工时费
        public dynamic F_HourAuditMoney { get; set; }//工时单价
        
    }
}