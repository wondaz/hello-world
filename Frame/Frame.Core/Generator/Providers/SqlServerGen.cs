using System;

namespace Frame.Core.Generator
{
    //取得表的方法
    public class SqlServerGen : ISqlGen
    {
        //取得表名，返回字段TableName
        public string SqlGetTableNames()
        {
            return "SELECT Name as TableName FROM sys.tables order by Name";
        }

        public string SqlGetTableKeys(string tableName)
        {
            return string.Format("sp_pkeys '{0}'", tableName);
        }

        //取得表结构
        public string SqlGetTableSchemas(string tableName)
        {
            return string.Format(@"
SELECT '{0}' as TableName,sys.columns.name AS ColumnName, 
		type_name(sys.columns.system_type_id) AS SqlTypeName,
		sys.columns.max_length AS MaxLength,
		sys.columns.is_nullable AS IsNullable,
		sys.columns.is_identity AS IsIdentity,
		(case when exists(select 1  
						 from   syscolumns 
						 join   sysindexkeys on syscolumns.id  =sysindexkeys.id and syscolumns.colid=sysindexkeys.colid and syscolumns.id=sys.columns.object_id 
						 join   sysindexes   on syscolumns.id  =sysindexes.id   and sysindexkeys.indid=sysindexes.indid   
						 join   sysobjects   on sysindexes.name=sysobjects.name and sysobjects.xtype= 'PK '
						 where syscolumns.name = sys.columns.name) then 1 else 0 end) AS IsPrimaryKey
FROM sys.columns    
WHERE sys.columns.object_id = object_id('{0}')", tableName);
        }
    }
}
