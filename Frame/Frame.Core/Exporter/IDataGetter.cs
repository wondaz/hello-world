/*************************************************************************
 * 文件名称 ：IDataGetter.cs                          
 * 描述说明 ：取得数据接口
 * 
 * 创建信息 :   on 2012-11-10
 * 修订信息 : modify by (person) on (date) for (reason)
 * 
 * 版权信息 : Copyright (c) 2013 
**************************************************************************/

using System.Web;

namespace Frame.Core
{
    public interface IDataGetter
    {
        object GetData(HttpContext context);
    }
}
