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
    /// <summary>
    /// 车辆档案查询的实体属性
    /// </summary>
    [Module("DMS_4S")]
    public class AutoArchivesSeleEntry : ServiceBase<ModelBase>
    {
        public dynamic GetAutoArchivesModel(string id)
        {
            string sqls = string.Format(@"select A.ID,A.SignCode,
            A.VIN,A.BrandName,A.SeriesName,A.ModelName,A.OutsideColor,
            A.InsideColor ,A.EngineCode,A.MeasureCode,A.ProduceArea,A.ProduceDate,A.ProduceCorp,A.CostPrice
            ,A.CarType,A.AssembleMemo,C.CustomerName,C.MobileTel,B.CorpID,D.UserName AS SaleManName,
            B.SaleDate,A.SignDate,A.YearAudit,B.UseType,B.MaintainDate,
			F.InsureCorpID,
			F.StartDate,
			F.EndDate,
			F.InsOrderCode,
			F.InsuFee,
			A.UpdateName
            from B_AutoArchives A
            left join [dbo].[A_SaleOrders] B ON A.VIN=B.VIN
            left join [dbo].[B_Customer] C ON C.CustomerID=B.CustomerID
            left join [dbo].[SYS_USER] D ON D.UserCode=B.SaleMan
			left join [dbo].[A_InsSale] F ON A.VIN=F.VIN where A.ID='{0}'", id);
            var projectInfo = db.Sql(sqls).QuerySingle<dynamic>();
            string sql1 = string.Format(@"SELECT top(1)
		    E.EmpName AS ServiceMan,
		    E.NextMaintainDate,
		    E.NextMaintainDistance,
		    E.MeetAutoTime,
		    E.RepairName LinkMan,
		    E.RepairTel LinkTel,E.RunDistance
		    from B_AutoArchives A
		    left join [dbo].[S_Dispatch] E ON E.VIN=A.VIN where A.ID='{0}' order by E.DispatchCode desc", id);
            var form1 = db.Sql(sql1).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return Extend(GetNewModel(new { }), projectInfo);
            }
            
            return Extend(projectInfo, form1);
        }

        public dynamic PostAutoArchives(JObject data) 
        {
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("修改车辆档案销售信息", () =>
            {
                string upves = string.Format(@"update B_AutoArchives set SignCode='{0}',
                               SignDate='{1}',YearAudit='{2}' where ID='{3}'", data["SignCode"].ToString()
                                                                             , data["SignDate"].ToString()
                                                                             , data["YearAudit"].ToString()
                                                                             ,data["ID"].ToString());
                string upordes = string.Format(@"UPDATE A_SaleOrders set UseType='{0}',MaintainDate='{1}' 
                                 where VIN='{2}'", data["UseType"].ToString()
                                                 ,data["MaintainDate"].ToString()
                                                 ,data["VIN"].ToString());
                dbContext.Sql(upves).Execute();
                dbContext.Sql(upordes).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }
    }
    public class AutoArchivesDetail : ModelBase 
    {
        public int ID { get; set; }
        public string VIN { get; set; }
        public string BrandName { get; set; }
        public string SeriesName { get; set; }
        public string ModelName { get; set; }
        public string OutsideColor { get; set; }
        public string InsideColor { get; set; }
        public string EngineCode { get; set; }
        public string MeasureCode { get; set; }
        public string ProduceArea { get; set; }
        public DateTime? ProduceDate { get; set; }
        public string ProduceCorp { get; set; }
        public decimal CostPrice { get; set; }
        public string CarType { get; set; }
        public string AssembleMemo { get; set; }
        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public string CorpID { get; set; }
        public string SaleManName { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime? SignDate { get; set; }
        public string YearAudit { get; set; }
        public string UseType { get; set; }
        public string ServiceMan { get; set; }
        public int RunDistance { get; set; }
        public DateTime? MaintainDate { get; set; }
        public DateTime? NextMaintainDate { get; set; }
        public string NextMaintainDistance { get; set; }
        public DateTime? MeetAutoTime { get; set; }
        public string LinkMan { get; set; }
        public string LinkTel { get; set; }
        public string UpdateName { get; set; }
        public string SignCode { get; set; }
        public string InsureCorpID { get; set; }//保险公司
        public string StartDate { get; set; }//保险开始日
        public string EndDate { get; set; }//保险截止日
        public string InsOrderCode { get; set; }//保单号
        public decimal InsuFee { get; set; }//投保金额
    }
}