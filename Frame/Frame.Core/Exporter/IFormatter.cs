/*************************************************************************
 * 文件名称 ：IFormatter.cs                          
 * 描述说明 ：字段格式化接口
 * 
 * 创建信息 :   on 2012-11-10
 * 修订信息 : modify by (person) on (date) for (reason)
 * 
 * 版权信息 : Copyright (c) 2013 
**************************************************************************/

namespace Frame.Core
{
    public interface IFormatter
    {
        string Format(string codeType,string codeValue);
    }
}
