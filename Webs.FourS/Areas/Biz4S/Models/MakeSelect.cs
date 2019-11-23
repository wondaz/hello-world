using FluentData;
using Frame.Core;
using Web.FourS.Areas.Sys4S.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using Newtonsoft.Json.Linq;
namespace Web.FourS.Areas.Biz4S.Models
{
    [Module("DMS_4S")]
    public class MakeSelect : ServiceBase<MakeSeleModel>
    {
       
        public dynamic GetSaleOrder(string id)
        {
            StringBuilder sqls = new StringBuilder();
            sqls.Append("select A.[BookingID],A.[BookingCode],A.[BookingName],A.[BookingInTime],A.[BillTypeID],A.[PRI],A.[RunDistance],A.[RepairDescribe]");
            sqls.Append(",A.[SignCode],A.[CustomerID],A.[RepairName],A.[RepairTel],A.[Address],A.[InputName],");
            sqls.Append("A.[InputTime],A.[BillState],A.[StayTime],A.[StayExplain],A.[BookingFashion],A.[UpdatePerson],A.[UpdateDate] ");
            sqls.Append(",B.[CustomerName],B.[MobileTel],B.[Address] as CustomerAddress");
            sqls.AppendFormat(" from S_Booking A LEFT JOIN B_Customer B ON B.CustomerID=A.CustomerID where A.BookingID='{0}'", id);
            var projectInfo = db.Sql(sqls.ToString()).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return projectInfo;
            }
            
            return GetNewModel(new
            {

                //BookingInTime = DateTime.Now,
                BookingID = Guid.NewGuid().ToString(),
                BookingCode =NewKey.DateFlowCode(db, "维修预约单", "WXYYD"),
                InputName = FormsAuth.UserData.UserName,
                InputTime = DateTime.Now.ToShortDateString()
            });
        }

        public dynamic PostMake(Object data1)
        {
            JObject data = JObject.Parse(data1.ToString());
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_Booking_Update")
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("CustomerName", data["CustomerName"] == null ? "" : data["CustomerName"].ToString())
                    .Parameter("MobileTel", data["MobileTel"] == null ? "" : data["MobileTel"].ToString())
                    .Parameter("BookingID", data["BookingID"] == null ? "" : data["BookingID"].ToString())
                    .Parameter("BookingCode", data["BookingCode"] == null ? "" : data["BookingCode"].ToString())
                    .Parameter("BookingName", data["BookingName"] == null ? "" : data["BookingName"].ToString())
                    .Parameter("BookingInTime", data["BookingInTime"].ToString())
                    .Parameter("BillTypeID", data["BillTypeID"] == null ? "" : data["BillTypeID"].ToString())
                    .Parameter("PRI", data["PRI"] == null ? "" : data["PRI"].ToString())
                    .Parameter("RunDistance", data["RunDistance"] == null ? "" : data["RunDistance"].ToString())
                    .Parameter("RepairDescribe", data["RepairDescribe"] == null ? "" : data["RepairDescribe"].ToString())
                    .Parameter("SignCode", data["SignCode"] == null ? "" : data["SignCode"].ToString())
                    .Parameter("CustomerID", data["CustomerID"] == null ? "" : data["CustomerID"].ToString())
                    .Parameter("RepairName", data["RepairName"] == null ? "" : data["RepairName"].ToString())
                    .Parameter("RepairTel", data["RepairTel"] == null ? "" : data["RepairTel"].ToString())
                    .Parameter("CustomerAddress", data["CustomerAddress"] == null ? "" : data["CustomerAddress"].ToString())
                    .Parameter("Address", data["Address"] == null ? "" : data["Address"].ToString())
                    .Parameter("InputName", data["InputName"] == null ? "" : data["InputName"].ToString())
                    .Parameter("InputTime", data["InputTime"] == null ? "" : data["InputTime"].ToString())
                    .Parameter("BillState", data["BillState"] == null ? "" : data["BillState"].ToString())
                    .Parameter("StayTime", data["StayTime"] == null ? "" : data["StayTime"].ToString())
                    .Parameter("StayExplain", data["StayExplain"] == null ? "" : data["StayExplain"].ToString())
                    .Parameter("BookingFashion", data["BookingFashion"] == null ? "" : data["BookingFashion"].ToString())
                    .ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
                var result = builder.ParameterValue<string>("Result");
                return new { result = true };
            }
            catch (Exception ex)
            {
                return new { result = false, msg = ex.Message };
            }
        }

        public dynamic DeleteMake(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除预约订单", () =>
            {
                dbContext.Sql("DELETE FROM S_Booking WHERE BookingID = @0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });

            return result;
        }

        public List<dynamic> GetListSignCode(string Code) {
            StringBuilder sqls = new StringBuilder();
            sqls.Append("SELECT top(20) [SignCode]");
            sqls.AppendFormat(" FROM B_AutoArchives WHERE State=0 AND SignCode LIKE '%{0}%'", Code);
            return db.Sql(sqls.ToString()).QueryMany<dynamic>();
        }

        public dynamic GetArchiveInfo(string id)
        {
            string sqls = "select b.RunDistance,b.LinkTel,b.LinkMan from  B_AutoArchives AS A ";
            sqls = sqls + string.Format("LEFT JOIN B_AutoServiceInfo AS B ON A.ID=B.PKID where a.SignCode='{0}'",id);
            var resuel = db.Sql(sqls).QuerySingle<dynamic>();
            return resuel;
        }
    }
    public class MakeSeleModel : ModelBase 
    {
        [PrimaryKey]
        public string BookingID { get; set; }//预约ID
        public string BookingCode { get; set; }
        public string BookingName { get; set; }
        public DateTime? BookingInTime { get; set; }//预定时间
        public string BillTypeID { get; set; }
        public string BillTypeName { get; set; }
        public string PRI { get; set; }
        public decimal RunDistance { get; set; }
        public string BookingFashion { get; set; }
        public string RepairDescribe { get; set; }
        public DateTime? ArriveTime { get; set; }
        public string StayExplain { get; set; }
        public DateTime? StayTime { get; set; }
        public string SignCode { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string MobileTel { get; set; }
        public string RepairName { get; set; }
        public string RepairTel { get; set; }
        public string Address { get; set; }//报修人地址
        public string InputName { get; set; }
        public DateTime? InputTime { get; set; }
        public string BillState { get; set; }
        /// <summary>
        /// 联系地址
        /// </summary>
        public string CustomerAddress { get; set; }
    }
}