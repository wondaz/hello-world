namespace Frame.Core
{
    public class LoginUser
    {
        public int UserID { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public int CorpID { get; set; }
        public string CorpCode { get; set; }
        public string CorpName { get; set; }
        public string CorpShortName { get; set; }
        public string CorpAddress { get; set; }
        public string CorpPhone { get; set; }
        
        // public int CollaborationID { get; set; }
        //public string CollaborationName { get; set; }
        public int UnionID { get; set; }
        public int CorpLevel { get; set; }
        public int ParentID { get; set; }
        public string RoleCode { get; set; }
        public string TelePhone { get; set; }
        public string LinkPhone { get; set; }
        public string DeptID { get; set; }
        public string DeptCode { get; set; }
        public string DeptName { get; set; }

    }
}
