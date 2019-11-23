using System;
using System.Collections.Generic;
using Frame.Core;

namespace Web.FourS.Areas.Sys4S.Models
{
    [Module("DMS_4S")]
    public class CodeTypeService : ServiceBase<sys_codeType>
    {
        public List<dynamic> GetCodeType()
        {
            if (FormsAuth.UserData.UserID == 2)//系统维护
            {
                return db.Sql("SELECT * FROM [sys_codeType] ORDER BY Grade,SEQ").QueryMany<dynamic>();
            }
            //4s店
            return db.Sql("SELECT * FROM [sys_codeType] WHERE Grade=1 ORDER BY SEQ").QueryMany<dynamic>();
        }

        public List<dynamic> GetDataDict(string codeType)
        {
            var sql = "";
            if (FormsAuth.UserData.UserID == 2)//系统维护
            {
                sql = "SELECT * FROM sys_code WHERE codeType=@0 ORDER BY CorpID,Seq";
            }
            else
            {
                sql = "SELECT * FROM sys_code WHERE codeType=@0 AND CorpID IN (1,@1) ORDER BY CorpID,Seq";
            }
            return db.Sql(sql, codeType, FormsAuth.UserData.CorpID).QueryMany<dynamic>();
        }

        /// <summary>
        /// 数据字典
        /// </summary>
        /// <param name="codeType"></param>
        /// <returns></returns>
        public List<textValue> GetDictItemsByType(string codeType)
        {
            var sql = string.Format(
                "SELECT value,text FROM sys_code WHERE CodeType='{0}' AND IsEnable=1 AND CorpID IN (1,'{1}') ORDER BY Seq", codeType.Replace("'", ""), FormsAuth.UserData.CorpID);

            if (codeType.ToLower() == "warehousecode")
            {
                sql = "select storehcode as value,storename as text from t_ga_storehouse";
            }
            else if (codeType.ToLower() == "dept")
            {
                sql = "Select deptCode as value,DeptName as text FROM t_base_dept t Where delFlag='0'";
            }

            return db.Sql(sql).QueryMany<textValue>();
        }

        protected override bool OnBeforEditDetail(EditEventArgs arg)
        {
            var variable = arg.wrapper.GetVariableName("CodeType");
            var oldCodeType = arg.row[variable].ToString();
            switch (arg.type)
            {
                case OptType.Mod:
                    var newCodeTypeName = arg.row["CodeTypeName"].ToString();
                    arg.db.Sql(string.Format("update sys_code set CodeTypeName='{0}' where CodeType='{1}'", newCodeTypeName, oldCodeType)).Execute();
                    break;
                case OptType.Del:
                    arg.db.Delete("sys_code").Where("CodeType", oldCodeType).Execute();
                    break;
            }

            return true;
        }
    }

    public class sys_codeType : ModelBase
    {
        public string CodeType { get; set; }
        public string CorpID { get; set; }

        public string CodeTypeName { get; set; }

        public string Description { get; set; }

        public int Seq { get; set; }

        public string UpdatePerson { get; set; }

        public DateTime UpdateDate { get; set; }
        public int Grade { get; set; }

    }
}
