
using System;

namespace Frame.Core
{
    public class ParamWhere
    {
        public WhereData Data { get; set; }
        public Func<WhereData,string> Compare { get; set; }
    }

    public class WhereData
    {
        public string AndOr { get; set; }
        public string Column { get; set; }
        public object Value { get; set; }
        public object[] Extend { get; set; }
    }
}
