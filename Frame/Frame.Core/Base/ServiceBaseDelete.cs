/*************************************************************************
 * 文件名称 ：ServiceBaseDelete.cs                          
 * 描述说明 ：定义数据服务基类中的删除处理
 **************************************************************************/

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        protected virtual bool OnBeforeDelete(DeleteEventArgs arg)
        {
            return true;
        }

        protected virtual void OnAfterDelete(DeleteEventArgs arg)
        {

        }

        public int Delete(ParamDelete param)
        {
            var result = 0;
            Logger("删除记录", () =>
            {
                using (var dbContext = db.UseTransaction(true))
                {
                    var rtnBefore = this.OnBeforeDelete(new DeleteEventArgs { db = dbContext, data = param.GetData() });
                    if (!rtnBefore) return;
                    //result = BuilderParse(param).Execute();
                    result = db.Sql(BuilderDelSql(param)).Execute();
                    Msg.Set(MsgType.Success, APP.MSG_DELETE_SUCCESS);
                    OnAfterDelete(new DeleteEventArgs { db = dbContext, data = param.GetData(), executeValue = result });
                    db.Commit();
                }
            });
            return result;
        }
    }
}
