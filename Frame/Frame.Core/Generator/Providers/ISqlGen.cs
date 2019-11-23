namespace Frame.Core.Generator
{
    //接口
    public interface ISqlGen
    {
        string SqlGetTableNames();                          //取得表名
        string SqlGetTableSchemas(string tableName);        //取得表结构
        string SqlGetTableKeys(string tableName);           //sql server 调用sp_pkeys取得主key
    }
}
