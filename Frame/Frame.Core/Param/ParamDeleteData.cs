using System.Collections.Generic;

namespace Frame.Core
{
    public class ParamDeleteData
    {
        public string From { get; set; }
        public List<ParamWhere> Where { get; set; }
        public string WhereSql { get { return ParamUtils.GetWhereSql(Where); } }

        
        public object GetValue(string column)
        {
            var first = Where.Find(x => x.Data.Column == column);
            return first == null ? null : first.Data.Value;
        }

        public ParamDeleteData()
        {
            From = "";
            Where = new List<ParamWhere>();
        }
    }
}
