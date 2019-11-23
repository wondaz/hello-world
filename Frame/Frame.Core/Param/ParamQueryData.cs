using System.Collections.Generic;

namespace Frame.Core
{
    public class ParamQueryData
    {
        public int PagingCurrentPage { get; set; }
        public int PagingItemsPerPage { get; set; }
        
        public string Having { get; set; }
        public string GroupBy { get; set; }
        public string OrderBy { get; set; }
        public string From { get; set; }
        public string Select { get; set; }
        public List<ParamWhere> Where { get; set; }
        public string WhereSql {get { return ParamUtils.GetWhereSql(Where); }}

        public ParamQueryData()
        {
            Having = "";
            GroupBy = "";
            OrderBy = "";
            From = "";
            Select = "";
            Where = new List<ParamWhere>();
            PagingCurrentPage = 1;
            PagingItemsPerPage = 0;
        }
    }
}
