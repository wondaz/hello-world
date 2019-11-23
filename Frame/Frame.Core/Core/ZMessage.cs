using System.Runtime.Serialization;

namespace Frame.Core
{
    [DataContract]
    public class AjaxMessge
    {
        [DataMember]
        public string code { get; set; }

        [DataMember]
        public string text { get; set; }

        public AjaxMessge()
        {
            Set(MsgType.Success, string.Empty);
        }

        public AjaxMessge Set(MsgType msgtype, string message)
        {
            code = msgtype.ToString().ToLower();
            text = message;
            return this;
        }

        public AjaxMessge Set(string txtcode, string message)
        {
            code = txtcode;
            text = message;
            return this;
        }
    }
}
