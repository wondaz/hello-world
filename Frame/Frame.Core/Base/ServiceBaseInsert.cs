/*************************************************************************
 * 文件名称 ：ServiceBaseInsert.cs                          
 * 描述说明 ：定义数据服务基类中的插入处理
 **************************************************************************/

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        protected virtual bool OnBeforeInsert(InsertEventArgs arg)
        {
            return true;
        }

        protected virtual void OnAfterInsert(InsertEventArgs arg)
        {

        }

        public int Insert(ParamInsert param)
        {
            var result = 0;
            Logger("增加记录", () =>
            {
                using (var dbContext = db.UseTransaction(true))
                {
                    var rtnBefore = this.OnBeforeInsert(new InsertEventArgs() { db = dbContext, data = param.GetData() });
                    if (!rtnBefore) return;

                    var identity = ModelBase.GetAttributeFields<T, IdentityAttribute>();
                    result = identity.Count > 0 ? db.Sql(BuilderParse(param)).ExecuteReturnLastId<int>() : db.Sql(BuilderParse(param)).Execute();

                    Msg.Set(MsgType.Success, APP.MSG_INSERT_SUCCESS);
                    this.OnAfterInsert(new InsertEventArgs() { db = dbContext, data = param.GetData(), executeValue = result });
                    dbContext.Commit();
                }
               
            });
            return result;
        }
    }
}
