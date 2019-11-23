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
    public class Insurance : ServiceBase<InsuranceModele>
    {
        public dynamic GetSaleOrder(string id)
        {
            StringBuilder sqls = new StringBuilder();
            //sqls.AppendFormat("select InsSaleCode,InsureCorpID,InsOrderCode,IsContinueIns,IsContinueIns,SaleMan,TranMan,TranDate,TranAdress,StartDate,EndDate,ServiceFee,InsuFee,FeeSum,VIN from [A_InsSale] where InsSaleID='{0}' order by InsSaleID", id);
            sqls.AppendFormat("select  * from [A_InsSale] where InsSaleID='{0}' order by InsSaleID", id);
            var projectInfo = db.Sql(sqls.ToString()).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return Extend(GetNewModel(new { }), projectInfo);
                //return projectInfo;
            }

            return GetNewModel(new
            {
                //BookingInTime = DateTime.Now,
                InsSaleID = Guid.NewGuid().ToString(),
                InsSaleCode = NewKey.DateFlowCode(db, "销售单号", "XSDH"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString()
            });
        }
        public List<textValue> B_InsureCorp_GetList() 
        {
            var result = db.Sql("select InsureCorpID as value,InsureCorpName as text from [B_InsureCorp]").QueryMany<textValue>();
            return result;
        }


        public List<textValue> B_AutoBrand_GetList()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT BrandID as value,BrandName as text FROM B_AutoBrand WHERE State = 0 ORDER BY BrandName ASC");
            return db.Sql(sql.ToString()).QueryMany<textValue>();
        }
        public List<textValue> B_AutoSeries_GetList()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT SeriesID as value,SeriesName as text FROM B_AutoSeries WHERE State = 0 ORDER BY SeriesName asc");
            return db.Sql(sql.ToString()).QueryMany<textValue>();
        }
         public List<textValue> B_AutoModel_GetList()
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT ModelID as value,ModelName as text FROM B_AutoModel WHERE State = 0 ORDER BY ModelName ASC");
            return db.Sql(sql.ToString()).QueryMany<textValue>();
        }
        
        public List<textValue> B_InsureType_GetList()
         {
             StringBuilder sql = new StringBuilder();
             sql.Append(" SELECT InsureTypeID as value, InsureTypeName as text FROM B_InsureType WHERE State = 0");
             return db.Sql(sql.ToString()).QueryMany<textValue>();
         }
        public dynamic PostInsurance(Object data1)
        {
            JObject data = JObject.Parse(data1.ToString());
            var dbContext = db.UseTransaction(true);
            string result = "ok";
            int TradeOrderID = 0;
            Logger("编辑保险销售单主表", () =>
            {
                //主表
                var user = FormsAuth.UserData;
                var head = data["head"];
                var builder = dbContext.StoredProcedure("SP_InsSale_Update")
                    .Parameter("InsSaleID", head["InsSaleID"] == null ? "" : head["InsSaleID"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("InsSaleCode", head["InsSaleCode"] == null ? "" : head["InsSaleCode"].ToString())
                    .Parameter("SaleOrderID", head["SaleOrderID"] == null ? "" : head["SaleOrderID"].ToString())
                    .Parameter("Saleman", head["SaleMan"] == null ? "" : head["SaleMan"].ToString())
                    .Parameter("CustomerID", head["CustomerID"] == null ? "" : head["CustomerID"].ToString())
                    .Parameter("SignCode", head["SignCode"] == null ? "" : head["SignCode"].ToString())
                    .Parameter("VIN", head["VIN"] == null ? "" : head["VIN"].ToString())
                    .Parameter("InsOrderCode", head["InsOrderCode"] == null ? "" : head["InsOrderCode"].ToString())
                    .Parameter("TranDate", Convert.ToDateTime(head["TranDate"]))
                    .Parameter("TranAdress", head["TranAdress"] == null ? "" : head["TranAdress"].ToString())
                    .Parameter("TranMan", head["TranMan"] == null ? "" : head["TranMan"].ToString())
                    .Parameter("StartDate", Convert.ToDateTime(head["StartDate"]))
                    .Parameter("EndDate", Convert.ToDateTime(head["EndDate"]))
                    .Parameter("InsuFee", Convert.ToDecimal(head["InsuFee"]))
                    .Parameter("ServiceFee", Convert.ToDecimal(head["ServiceFee"]))
                    .Parameter("FeeSum", Convert.ToDecimal(head["FeeSum"]))
                    .Parameter("PayMode", head["PayMode"] == null ? "" : head["PayMode"].ToString())
                    .Parameter("Remark", head["Remark"] == null ? "" : head["Remark"].ToString())
                    .Parameter("BillState", head["BillState"].ToString() == "" ? "0" : head["BillState"].ToString())
                    .Parameter("InputName", head["InputName"] == null ? "" : head["InputName"].ToString())
                    .Parameter("InputTime", Convert.ToDateTime(head["InputTime"]))
                    .Parameter("InsureCorpID", head["InsureCorpID"] == null ? "" : head["InsureCorpID"].ToString())
                    .Parameter("Beneficiary", head["Beneficiary"] == null ? "" : head["Beneficiary"].ToString())
                    .Parameter("UpdatePerson", user.UserName)
                    .Parameter("IsContinueIns", head["IsContinueIns"] == null ? "" : head["IsContinueIns"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                result = builder.ParameterValue<string>("Result");
                if (result == "ok")
                {
                    //子表updated
                    int orderid = 0;
                    #region
                    foreach (JObject row in (JArray)data["rows"]["inserted"])
                    {
                        if (row.ToString() == "{\r\n  \"_isnew\": true,\r\n  \"InsureTypeID\": \"\",\r\n  \"InsureMoney\": \"\",\r\n  \"InsureFee\": \"\",\r\n  \"FactAmount\": \"\",\r\n  \"Remark_Detail\": \"\"\r\n}")
                        {
                            continue;
                        }
                        orderid++;
                        builder = db.StoredProcedure("SP_InsSaleDetail_Update")
                            .Parameter("SerialID", row["SerialID"] == null ? Guid.NewGuid().ToString() : row["SerialID"].ToString())
                            .Parameter("InsSaleID", head["InsSaleID"].ToString())
                            .Parameter("InsureTypeID", row["InsureTypeID"].ToString())
                            .Parameter("InsureMoney",Convert.ToDecimal(row["InsureMoney"].ToString()))
                            .Parameter("InsureFee", Convert.ToDecimal(row["InsureFee"].ToString()))
                            .Parameter("Rebate", 1.00)
                            .Parameter("FactAmount", Convert.ToDecimal(row["FactAmount"].ToString()))
                            .Parameter("Remark", row["Remark_Detail"].ToString())
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("OrderID", orderid)
                            .Parameter("UpdatePerson", user.UserName)
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
                    }
                    #endregion
                    #region
                    foreach (JObject row in (JArray)data["rows"]["updated"])
                    {
                        if (row.ToString() == "{\r\n  \"_isnew\": true,\r\n  \"InsureTypeID\": \"\",\r\n  \"InsureMoney\": \"\",\r\n  \"InsureFee\": \"\",\r\n  \"FactAmount\": \"\",\r\n  \"Remark_Detail\": \"\"\r\n}")
                        {
                            continue;
                        }
                        orderid++;
                        builder = db.StoredProcedure("A_InsSaleDetail_Update")
                            .Parameter("SerialID", row["SerialID"] == null ? Guid.NewGuid().ToString() : row["SerialID"].ToString())
                            .Parameter("InsSaleID", head["InsSaleID"].ToString())
                            .Parameter("InsureTypeID", row["InsureTypeID"].ToString())
                            .Parameter("InsureMoney", Convert.ToDecimal(row["InsureMoney"].ToString()))
                            .Parameter("InsureFee", Convert.ToDecimal(row["InsureFee"].ToString()))
                            .Parameter("Rebate", 1.00)
                            .Parameter("FactAmount", Convert.ToDecimal(row["FactAmount"].ToString()))
                            .Parameter("Remark", row["Remark_Detail"].ToString())
                            .Parameter("CorpID", user.CorpID)
                            .Parameter("OrderID", orderid)
                            .Parameter("UpdatePerson", user.UserName)
                            .ParameterOut("Result", DataTypes.String, 200);
                        builder.Execute();
                        result = builder.ParameterValue<string>("Result");
                        if (result != "ok") break;
                    }
                    #endregion
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

        
        public List<dynamic> GetVin(string q) 
        {
            string sql = string.Format("SELECT TOP 30 VIN FROM B_AutoArchives where state=0 and corpid={0} AND VIN LIKE '%{1}%'", FormsAuth.UserData.CorpID, q);
            return db.Sql(sql).QueryMany<dynamic>();
        }
        public dynamic GetArchiveInfo(string id) 
        {
            string sql = string.Format(@"SELECT a.VIN,a.SignCode,c.CustomerName,c.FixTel,c.MobileTel,
                           c.Postalcode,
                           c.Address,d.CredentialNo,b.IsInstalment,a.BrandID,a.SeriesID,a.ModelID
                           ,a.EngineCode,a.MeasureCode,a.OutsideColor,a.InsideColor 
                           FROM B_AutoArchives a left join A_SaleOrders b on a.id=b.SaleOrderID and b.VIN=a.VIN And b.State=0
                           left join B_Customer c on c.CustomerID=b.CustomerID
                           left join B_CustomerProperty d on d.CustomerID=b.CustomerID
                           WHERE a.VIN='{0}'", id);
            var result = db.Sql(sql).QuerySingle<dynamic>();
            return result;
        }
        /// <summary>
        /// grid列表查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public dynamic GetGridLs(string id) 
        {
            string sqls = "select SerialID,InsureTypeID,InsureMoney,InsureFee,FactAmount,Remark as Remark_Detail from A_InsSaleDetail";
            sqls = sqls + string.Format(" where InsSaleID='{0}'",id);
            var result = db.Sql(sqls).QuerySingle<dynamic>();
            return result;
        }
        public dynamic DeleteBxMx(JObject id) 
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除保险单", () =>
            {
                dbContext.Sql("delete from [dbo].[A_InsSaleDetail] where SerialID=@0", id["SerialID"].ToString()).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }
        public dynamic DeleteInsurance(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除保险单", () =>
            {
                dbContext.Sql("delete from [dbo].[A_InsSale] where InsSaleID=@0", id).Execute();
                dbContext.Sql("delete from [dbo].[A_InsSaleDetail] where InsSaleID=@0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
        public int PostInsuranceOrder(JObject data) 
        {
            var result = db.Sql("UPDATE [A_InsSale] SET BillState = @0,AuditName=@1,AuditTime=GETDATE(),UpdateDate=GETDATE() WHERE InsSaleID = @2", data["status"].Value<string>(), FormsAuth.UserData.UserName, data["id"].Value<string>()).Execute();

            return result;
        }
    }

    public class InsuranceModele : ModelBase
    {
        /// <summary>
        /// 主键key
        /// </summary>
        [PrimaryKey]
        public string InsSaleID { get; set; }
        /// <summary>
        /// 保险销售单号
        /// </summary>
        public string InsSaleCode { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string SaleOrderID { get; set; }
        /// <summary>
        /// 销售顾问(推荐人)
        /// </summary>
        public string SaleMan { get; set; }
        /// <summary>
        /// 客户ID
        /// </summary>
        public string CustomerID { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string SignCode { get; set; }
        /// <summary>
        /// VIN码
        /// </summary>
        public string VIN { get; set; }
        /// <summary>
        /// 代办时间
        /// </summary>
        public DateTime? TranDate { get; set; }
        /// <summary>
        /// 代办地点
        /// </summary>
        public string TranAdress { get; set; }
        /// <summary>
        /// 代办人员(被保险销售员)
        /// </summary>
        public string TranMan { get; set; }
        /// <summary>
        /// 保险公司
        /// </summary>
        public string InsureCorpID { get; set; }
        /// <summary>
        /// 生效日期
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// 保险费
        /// </summary>
        public Decimal InsuFee { get; set; }
        /// <summary>
        /// 服务费(代办费)
        /// </summary>
        public Decimal ServiceFee { get; set; }
        /// <summary>
        /// 付款方式
        /// </summary>
        public string PayMode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 费用合计
        /// </summary>
        public Decimal FeeSum { get; set; }
        /// <summary>
        /// 保单号
        /// </summary> 
        public string InsOrderCode { get; set; }
        /// <summary>
        /// 制单日期
        /// </summary> 
        public DateTime? InputTime { get; set; }
        /// <summary>
        /// 制单人姓名
        /// </summary> 
        public string InputName { get; set; }
        /// <summary>
        /// 审核人姓名
        /// </summary> 
        public string AuditName { get; set; }
        /// <summary>
        /// 审核日期
        /// </summary> 
        public DateTime? AuditTime { get; set; }
        /// <summary>
        /// 单据状态
        /// </summary>
        public string BillState { get; set; }
        public string BillStateName { get; set; }
        /// <summary>
        /// 传输状态
        /// </summary>
        public string TransmissionState { get; set; }
        /// <summary>
        /// 是否续保
        /// </summary>
        public bool IsContinueIns { get; set; }
        /// <summary>
        /// 被保险人
        /// </summary>
        public string Beneficiary { get; set; }


        /// <summary>
        /// A_InsSaleDetail(Key)
        /// </summary>
        public string SerialID { get; set; }
        /// <summary>
        /// 保险销售单号详细表
        /// </summary>
        public string InsSaleID_Detail { get; set; }
        /// <summary>
        /// 保险种类
        /// </summary>
        public string InsureTypeID { get; set; }
        /// <summary>
        /// 保险金额
        /// </summary>
        public string InsureMoney { get; set; }
        /// <summary>
        /// 保险费
        /// </summary>
        public string InsureFee { get; set; }
        /// <summary>
        /// 折扣
        /// </summary>
        public decimal Rebate { get; set; }
        /// <summary>
        /// 实收金额
        /// </summary>
        public decimal FactAmount { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public decimal Remark_Detail { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int State_Detail { get; set; }

        /// <summary>
        /// 传输状态
        /// </summary>
        public string TransmissionState_Detail { get; set; }


        //----------客户信息
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustomerName { get; set; }
        /// <summary>
        /// 固定电话
        /// </summary>
        public string FixTel { get; set; }
        /// <summary>
        /// 移动电话
        /// </summary>
        public string MobileTel { get; set; }
        /// <summary>
        /// 邮编
        /// </summary>
        public string Postalcode { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string CredentialNo { get; set; }


        //----车辆信息
        /// <summary>
        /// 品牌
        /// </summary>
        public string BrandID { get; set; }
        /// <summary>
        /// 车系
        /// </summary>
        public string SeriesID { get; set; }
        /// <summary>
        /// 车型
        /// </summary>
        public string ModelID { get; set; }
        /// <summary>
        /// 外观色
        /// </summary>
        public string OutsideColor { get; set; }
        /// <summary>
        /// 内饰色
        /// </summary>
        public string InsideColor { get; set; }
        /// <summary>
        /// 发动机号
        /// </summary>
        public string EngineCode { get; set; }
        /// <summary>
        /// 合格证号
        /// </summary>
        public string MeasureCode { get; set; }
        /// <summary>
        /// 车牌号
        /// </summary>
        public bool IsLocal { get; set; }
        /// <summary>
        /// 是否分期付款
        /// </summary>
        public bool IsInstalment { get; set; }
    }
}
