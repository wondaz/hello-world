using System;
using System.Collections.Generic;
using Frame.Core;
using Newtonsoft.Json.Linq;

namespace Web.FourS.Areas.Sys4S.Models
{
    /// <summary>
    /// 暂时不用此文件
    /// </summary>
    [Module("DMS_4S")]
    public class SysCorpService : ServiceBase<Base_corp>
    {
        /// <summary>
        /// 企业树形结构
        /// </summary>
        /// <returns></returns>
        public List<dynamic> GetCorpTree(string corpid)
        {
            string sql = "SELECT CorpShortName AS text,CorpID AS id,ParentID AS pid FROM V_CorpTree";
            var result = db.Sql(sql).QueryMany<dynamic>();
            return result;
        }

        public dynamic GetCorpInfo(string id)
        {
            var masterData = db.Sql(string.Format(@"SELECT TOP 1 A.*,B.UserCode FROM base_corp A 
LEFT JOIN sys_user B ON A.CorpID=b.CorpID AND B.IsAdmin=1 WHERE A.CorpID={0}", id)).QuerySingle<Base_corp>();
            if (masterData != null)
            {
                return masterData;
            }

            return GetNewModel(new { });
        }

        protected override void OnAfterEditMaster(EditEventArgs arg)
        {
            if (arg.type != OptType.Add) return;

            var newCorpID = arg.IdentityVal;
            if (newCorpID == -1) return;
            Logger("新增企业角色", () =>
            {
                JToken form = arg.form;
                arg.db.Sql(
                    string.Format(
                        @"INSERT INTO [sys_user]([UserCode],[CorpID],[UserName],[Description],[Password],[IsEnable],[IsAdmin],[UpdatePerson]) 
                        VALUES('{0}',{1},'{2}','企业管理员','1234',1,1,'{3}')", form["UserCode"], newCorpID, form["CorpShortName"], form["UpdatePerson"])).Execute();

                //系统角色-整车销售顾问
                arg.db.Sql(
                   string.Format(
                       @"INSERT INTO [sys_role]([RoleCode],[CorpID],[RoleName],[Description],[UpdatePerson],[IsSystemRole]) 
                      VALUES('XSGW',{0},'销售顾问','系统角色','{1}',1)", newCorpID, form["UpdatePerson"])).Execute();

                //系统角色-保险销售顾问
                arg.db.Sql(
                   string.Format(
                       @"INSERT INTO [sys_role]([RoleCode],[CorpID],[RoleName],[Description],[UpdatePerson],[IsSystemRole]) 
                      VALUES('BXGW',{0},'保险销售顾问','系统角色','{1}',1)", newCorpID, form["UpdatePerson"])).Execute();

                //系统角色-采购员
                arg.db.Sql(
                   string.Format(
                       @"INSERT INTO [sys_role]([RoleCode],[CorpID],[RoleName],[Description],[UpdatePerson],[IsSystemRole]) 
                      VALUES('CGY',{0},'采购员','系统角色','{1}',1)", newCorpID, form["UpdatePerson"])).Execute();


                var roleExist = arg.db.Sql(@"SELECT COUNT(1) FROM sys_role WHERE RoleCode='XJQX' AND CorpID=" + form["ParentID"])
                    .QuerySingle<int>();
                if (roleExist == 0)
                {
                    arg.db.Sql(
                    string.Format(
                        @"INSERT INTO [sys_role]([RoleCode],[CorpID],[RoleName],[Description],[UpdatePerson],[IsSystemRole]) 
                      VALUES('XJQX',{0},'下级站点权限','系统角色','{1}',1)", form["ParentID"], form["UpdatePerson"])).Execute();
                }

                arg.db.Sql(string.Format(@"INSERT INTO sys_userRoleMap(UserCode,RoleCode) VALUES('{0}','XJQX')",form["UserCode"])).Execute();
            }, e => arg.db.Rollback());

            base.OnAfterEditMaster(arg);
        }

        public List<dynamic> GetCorpNames(string q, string corpType)
        {
            var xmlPath = "~/Areas/comm/xml/corp.xml";
            var request = RequestWrapper.Instance();
            request.LoadXmlByUrl(xmlPath, corpType);
            var pareamQuery = request.ToParamQuery().AndWhere("CorpName", q, Cp.Like);
            return GetDynamicList(pareamQuery);
        }
    }

    public class Base_corp : ModelBase
    {
        [PrimaryKey]
        public int CorpID { get; set; }
        public string CorpName { get; set; }
        public string CorpContact { get; set; }
        public string CorpPhone { get; set; }
        public string CorpAddress { get; set; }
        public string CorpFax { get; set; }
        public string CorpCode { get; set; }
        public int UnionID { get; set; }
        public int CollaborationID { get; set; }
        public int AreaID { get; set; }
        public string CorpShortName { get; set; }

        public string UserCode { get; set; }
        public int Enable { get; set; }
        public int CorpLevel { get; set; }
        public int ParentID { get; set; }
        public string UpdatePerson { get; set; }
        public DateTime? UpdateDate { get; set; }

    }
}