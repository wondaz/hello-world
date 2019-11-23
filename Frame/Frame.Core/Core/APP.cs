using System;
using System.Configuration;
using FluentData;

namespace Frame.Core
{
    public static class APP
    {
        //消息字段
        public static string MSG_SAVE_SUCCESS = "保存成功！";
        public static string MSG_DELETE_SUCCESS = "删除成功！";
        public static string MSG_INSERT_SUCCESS = "新增成功！";
        //配置自动更新的字段
        public static string FIELD_UPDATE_PERSON = "UPDATEPERSON";
        public static string FIELD_UPDATE_DATE = "UPDATEDATE";
        //数据库的一些设置
        public static string DB_DEFAULT_CONN_NAME {get; set; }
        public static string DB_NMPS_CONN_NAME = "NMPS2013";
        //本地部署的API地址
        public static string API_LOCAL = ConfigurationManager.AppSettings["API_LOCAL"];
        //平台部署的API地址
        public static string API_REMOTE = ConfigurationManager.AppSettings["API_REMOTE"];
        public static Action<CommandEventArgs> OnDbExecuting = null;

        public static DbProviderEnum DbProvider;
        //框架初始化函数
        public static void Init() 
        {
            LogHelper.Init();
        }
    }

    public enum DbProviderEnum
    {
        Oracle=0,
        SqlServer
    }
}
