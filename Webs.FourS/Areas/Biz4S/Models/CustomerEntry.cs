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
    public class CustomerEntry : ServiceBase<CustomerEntryModel>
    {
        public dynamic GetCustomerModel(string id) 
        {
            string sqls = string.Format(@"select A.CustomerID,A.CustomerCode,A.CustomerName,A.TypeID,A.LevelID,A.IntentLevelID,A.Salesman,
                            A.InfoSourseID,A.Sex,FixTel,A.MobileTel,A.Fax,A.Email,A.City,A.Province,A.County,A.Postalcode,
                            A.Address,A.LinkMan,A.LinkManTel,A.Remark,A.UpdateDate,A.UpdateName,A.ISPart,A.ISLatency,
                            A.ISServer,A.State,A.TransmissionState,B.Marry,B.WedDay,B.Occupation,B.Principalship,B.YealIncome,
                            B.FamilyIncome,B.TopEducation,B.GraduateAcademy,B.ContactWeek,B.ContactTime,B.ContactMode,
                            B.IsAllot,B.allotInfor,B.BirthdayDate,B.NativePlace,B.Nationality,B.CredentialType,
                            B.CredentialNo,B.EnglishName,B.HomePage,C.DrivingLicenceType,C.AuditDate,C.DriveAge,
                            C.DiffCharacter,C.Faith,C.JoinLeague,C.LeagueDuty,C.FirstBuy,C.FirstMode,
                            C.Character,C.Interest,C.ImpReason from [B_Customer] A left join [B_CustomerProperty] B on A.CustomerID=B.CustomerID
                             left join [B_CustomerAppertain] C on A.CustomerID=C.CustomerID where A.CustomerID='{0}'", id);
            var projectInfo = db.Sql(sqls).QuerySingle<dynamic>();
            if (projectInfo != null)
            {
                return Extend(GetNewModel(new { }), projectInfo);
            }

            return GetNewModel(new
            {
                UpdatePerson = FormsAuth.UserData.UserName,
                CorpID = FormsAuth.UserData.CorpID
            });
        }
        public dynamic PostCustomerMgr(JObject data) 
        {
            try
            {
                var user = FormsAuth.UserData;
                var builder = db.StoredProcedure("SP_Customer_Update")
                    .Parameter("CustomerID", data["CustomerID"] == null ? "" : data["CustomerID"].ToString())
                    .Parameter("CorpID", user.CorpID)
                    .Parameter("CustomerName", data["CustomerName"] == null ? "" : data["CustomerName"].ToString())
                    .Parameter("TypeID", data["TypeID"] == null ? "" : data["TypeID"].ToString())
                    .Parameter("LevelID", data["LevelID"] == null ? "" : data["LevelID"].ToString())
                    .Parameter("IntentLevelID", data["IntentLevelID"] == null ? "" : data["IntentLevelID"].ToString())
                    .Parameter("Salesman", data["Salesman"] == null ? "" : data["Salesman"].ToString())
                    .Parameter("InfoSourseID", data["InfoSourseID"].ToString())
                    .Parameter("BirthdayDate", data["BirthdayDate"] == null ? "" : data["BirthdayDate"].ToString())
                    .Parameter("Sex", data["Sex"] == null ? "" : data["Sex"].ToString())
                    .Parameter("NativePlace", data["NativePlace"] == null ? "" : data["NativePlace"].ToString())
                    .Parameter("Nationality", data["Nationality"] == null ? "" : data["Nationality"].ToString())
                    .Parameter("CredentialType", data["CredentialType"] == null ? "" : data["CredentialType"].ToString())
                    .Parameter("CredentialNo", data["CredentialNo"] == null ? "" : data["CredentialNo"].ToString())
                    .Parameter("EnglishName", data["EnglishName"] == null ? "" : data["EnglishName"].ToString())
                    .Parameter("HomePage", data["HomePage"] == null ? "" : data["HomePage"].ToString())
                    .Parameter("FixTel", data["FixTel"] == null ? "" : data["FixTel"].ToString())
                    .Parameter("MobileTel", data["MobileTel"] == null ? "" : data["MobileTel"].ToString())
                    .Parameter("Fax", data["Fax"] == null ? "" : data["Fax"].ToString())
                    .Parameter("Email", data["Email"] == null ? "" : data["Email"].ToString())
                    .Parameter("Marry", data["Marry"] == null ? "" : data["Marry"].ToString())
                    .Parameter("WedDay", data["WedDay"] == null ? "" : data["WedDay"].ToString())
                    .Parameter("Occupation", data["Occupation"] == null ? "" : data["Occupation"].ToString())
                    .Parameter("Principalship", data["Principalship"] == null ? "" : data["Principalship"].ToString())
                    .Parameter("YealIncome", data["YealIncome"] == null ? "" : data["YealIncome"].ToString())
                    .Parameter("FamilyIncome", data["FamilyIncome"] == null ? "" : data["FamilyIncome"].ToString())
                    .Parameter("TopEducation", data["TopEducation"] == null ? "" : data["TopEducation"].ToString())
                    .Parameter("GraduateAcademy", data["GraduateAcademy"] == null ? "" : data["GraduateAcademy"].ToString())
                    .Parameter("Province", data["Province"] == null ? "" : data["Province"].ToString())
                    .Parameter("City", data["City"] == null ? "" : data["City"].ToString())
                    .Parameter("County", data["County"] == null ? "" : data["County"].ToString())
                    .Parameter("Postalcode", data["Postalcode"] == null ? "" : data["Postalcode"].ToString())
                    .Parameter("Address", data["Address"] == null ? "" : data["Address"].ToString())
                    .Parameter("LinkMan", data["LinkMan"] == null ? "" : data["LinkMan"].ToString())
                    .Parameter("LinkManTel", data["LinkManTel"] == null ? "" : data["LinkManTel"].ToString())
                    .Parameter("ContactWeek", data["ContactWeek"] == null ? "" : data["ContactWeek"].ToString())
                    .Parameter("ContactTime", data["ContactTime"] == null ? "" : data["ContactTime"].ToString())
                    .Parameter("ContactMode", data["ContactMode"] == null ? "" : data["ContactMode"].ToString())
                    .Parameter("IsAllot", data["IsAllot"] == null ? "" : data["IsAllot"].ToString())
                    .Parameter("allotInfor", data["allotInfor"] == null ? "" : data["allotInfor"].ToString())
                    .Parameter("DrivingLicenceType", data["DrivingLicenceType"] == null ? "" : data["DrivingLicenceType"].ToString())
                    .Parameter("AuditDate", data["AuditDate"] == null ? "" : data["AuditDate"].ToString())
                    .Parameter("DriveAge", data["DriveAge"] == null ? "" : data["DriveAge"].ToString())
                    .Parameter("DiffCharacter", data["DiffCharacter"] == null ? "" : data["DiffCharacter"].ToString())
                    .Parameter("Faith", data["Faith"] == null ? "" : data["Faith"].ToString())
                    .Parameter("JoinLeague", data["JoinLeague"] == null ? "" : data["JoinLeague"].ToString())
                    .Parameter("LeagueDuty", data["LeagueDuty"] == null ? "" : data["LeagueDuty"].ToString())
                    .Parameter("FirstBuy", data["FirstBuy"] == null ? "" : data["FirstBuy"].ToString())
                    .Parameter("FirstMode", data["FirstMode"] == null ? "" : data["FirstMode"].ToString())
                    .Parameter("Character", data["Character"] == null ? "" : data["Character"].ToString())
                    .Parameter("Interest", data["Interest"] == null ? "" : data["Interest"].ToString())
                    .Parameter("ImpReason", data["ImpReason"] == null ? "" : data["ImpReason"].ToString())
                    .Parameter("Remark", data["Remark"] == null ? "" : data["Remark"].ToString())
                    .Parameter("UpdateName", user.UserName)
                    .Parameter("ISServer", data["ISServer"] == null ? "" : data["ISServer"].ToString())
                    .Parameter("ISLatency", data["ISLatency"] == null ? "" : data["ISLatency"].ToString());
                    //.ParameterOut("Result", DataTypes.String, 200);
                builder.Execute();
               // var result = builder.ParameterValue<string>("Result");
                return new { result = true };
            }
            catch (Exception ex)
            {
                return new { result = false, msg = ex.Message };
            }
        }

        public dynamic DeleteCustomer(JObject data) 
        {
            string id = data["id"].ToString();
            var result = "ok";
            var dbContext = db.UseTransaction(true);
            Logger("删除客户档案信息", () =>
            {
                dbContext.Sql("delete from [dbo].[B_Customer] where CustomerID=@0", id).Execute();
                db.Commit();
            }, e =>
            {
                dbContext.Rollback();
                result = e.Message;
            });
            return result;
        }
    }

    public class CustomerEntryModel : ModelBase 
    {
        public string CustomerID { get; set; }//客户id
        public int CorpID { get; set; }
        public string CustomerName { get; set; }//客户名称
        public string TypeID { get; set; }//客户类别
        public string TypeName { get; set;}//客户类别名称
        public string LevelID { get; set; }//客户等级
        public string LevelName { get; set; }//客户等级名称
        public string IntentLevelID { get; set; }//意向等级
        public string IntentLevelName { get; set; }//意向等级名称
        public string Salesman { get; set; }//销售顾问
        public string InfoSourseID { get; set; }//信息来源
        public string InfoSourseName { get; set; }//信息来源显示名称
        public string Sex { get; set; }//性别
        public string SexName { get; set; }//性别名称
        public string FixTel { get; set; }//客户电话
        public string MobileTel { get; set; }//移动电话
        public string Fax { get; set; }//传真
        public string Email { get; set; }//邮箱地址
        public string City { get; set; }//城市
        public string Province { get; set; }//省份
        public string County { get; set; }//区县
        public string Postalcode { get; set; }//邮编
        public string Address { get; set; }//客户地址
        public string LinkMan { get; set; }//联系人
        public string LinkManTel { get; set; }//联系人电话
        public string Remark { get; set; }//备注
        public string ISLatency { get; set; }//是否潜在客户
        public string ISLatencyName { get; set; }//潜在客户名称
        public string ISServer { get; set; }//是否为保有客户
        public string ISServerName { get; set; }//是否为保有客户名称
        public int State { get; set; }//状态
        public string TransmissionState { get; set; }//传输状态
        public DateTime? UpdateDate { get; set; }
        public string UpdatePerson { get; set; }
        public string InputDate { get; set; }//建档日期
        public string InputName { get; set; }//建档人
        public string SaleMan { get; set; }
        /// <summary>
        /// 客户属性信息
        /// </summary>
        public string SerialID { get; set; }//流水号
        public string Marry { get; set; }//婚否
        public DateTime? WedDay { get; set; }//结婚纪念日
        public string Occupation { get; set; }//职业
        public string Principalship { get; set; }//职务
        public string YealIncome { get; set; }//年收入(个人收入)
        public string FamilyIncome { get; set; }//家庭收入
        public string TopEducation { get; set; }//学历
        public string GraduateAcademy { get; set; }//毕业院校
        public string ContactWeek { get; set; }//方便联系周
        public string ContactTime { get; set; }//方便时段
        public string ContactMode { get; set; }//希望联系方式
        public string IsAllot { get; set; }//是否总部下发
        public string allotInfor { get; set; }//总部下发信息
        public DateTime? BirthdayDate { get; set; }//出生日期
        public string NativePlace { get; set; }//籍贯
        public string Nationality { get; set; }//民族
        public string CredentialType { get; set; }//证件类型
        public string CredentialNo { get; set; }//证件号码
        public string EnglishName { get; set; }//英文名
        public string HomePage { get; set; }//个人主页（个人网页）
        /// <summary>
        /// 客户附属信息
        /// </summary>
        public string CustomerAppertain_SerialID { get; set; }//流水号客户属信
        public string DrivingLicenceType { get; set; }//驾照类别
        public DateTime? AuditDate { get; set; }//年审时间
        public string DriveAge { get; set; }//驾龄
        public string DiffCharacter { get; set; }//特殊特征
        public string Faith { get; set; }//宗教信仰
        public string JoinLeague { get; set; }//参加社团
        public string LeagueDuty { get; set; }//社团职务
        public string FirstBuy { get; set; }//首次购车
        public string FirstMode { get; set; }//首次购车车型
        public string Character { get; set; }//性格特点
        public string Interest { get; set; }//兴趣爱好
        public string ImpReason { get; set; }//购车重要因素
    }
}