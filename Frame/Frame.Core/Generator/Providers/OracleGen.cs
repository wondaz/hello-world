namespace Frame.Core.Generator
{
    //取得表的方法
    public class OracleGen : ISqlGen
    {
        //取得表名，返回字段TableName即可
        public string SqlGetTableNames()
        {
            return "SELECT table_name as TableName FROM user_tables order by table_name";
        }

        public string SqlGetTableSchemas(string tableName)
        {
            var sql = string.Format(@"
SELECT utc.TABLE_NAME AS TableName,utc.column_name AS ColumnName
        ,utc.data_type AS SqlTypeName
        ,utc.DATA_LENGTH AS MaxLength
        ,(case when utc.NULLABLE = 'Y' then 1 else 0 end) AS IsNullable
        ,(case when exists(select 1 
           from user_constraints au, user_cons_columns ucc 
           where ucc.table_name = au.table_name 
           and ucc.constraint_name = au.constraint_name 
           and au.constraint_type = 'P' 
           and utc.TABLE_NAME = ucc.table_name 
           AND utc.COLUMN_NAME = ucc.column_name(+)) then 1 else 0 end) AS IsPrimaryKey
FROM user_tab_columns utc
WHERE utc.table_name = '{0}'", tableName.ToUpper());
            return sql;
        }

        public string SqlGetTableKeys(string tableName)
        {
            var sql = string.Format(@"
select b.column_name
  from dba_constraints a, user_cons_columns b
 where a.table_name = '{0}'
   AND a.CONSTRAINT_TYPE = 'P'
   and a.constraint_name = b.constraint_name
   AND a.owner = b.owner", tableName.ToUpper()); 
            return sql;
        }

    }
}
