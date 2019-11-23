using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using FluentData;

namespace Frame.Core
{
    public partial class ServiceBase<T> where T : ModelBase, new()
    {
        protected virtual bool OnBeforEdit(EditEventArgs arg)
        {
            return true;
        }

        protected virtual bool OnBeforEditMaster(EditEventArgs arg)
        {
            return true;
        }

        protected virtual void OnAfterEditMaster(EditEventArgs arg)
        {
        }

        protected virtual bool OnBeforEditDetail(EditEventArgs arg)
        {
            return true;
        }

        protected virtual void OnAfterEditDetail(EditEventArgs arg)
        {
        }

        protected virtual void OnAfterEdit(EditEventArgs arg)
        {
        }


        public int Edit(RequestWrapper formWrapper, RequestWrapper listWrapper, JObject data)
        {
            var rowsAffected = 0;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑记录", () =>
                {
                    var editArgs = new EditEventArgs { db = dbContext, form = data["form"], list = data["list"] };
                    editArgs.IdentityVal = -1;
                    if (formWrapper != null && data["form"] != null)
                    {
                        editArgs.wrapper = formWrapper;
                        //var pUpdate = formWrapper.SetRequestData(editArgs.form).ToParamUpdate();
                        //if (pUpdate.GetData().Columns.Count > 0)//更新主表
                        //{
                        var rtnBefore = this.OnBeforEditMaster(editArgs);
                        if (rtnBefore)
                        {
                            editArgs.type = OptType.Mod;
                            var pUpdate = formWrapper.SetRequestData(editArgs.form).ToParamUpdate(); //在before事件中更改了form中的值，刷新更新时的值
                            rowsAffected = dbContext.Sql(BuilderParse(pUpdate)).Execute();
                            if (rowsAffected == 0)
                            {
                                editArgs.type = OptType.Add;
                                rowsAffected = dbContext.Sql(BuilderParse(formWrapper.ToParamInsert())).Execute();
                                if (APP.DbProvider == DbProviderEnum.SqlServer)
                                {
                                    editArgs.IdentityVal = dbContext.Sql("SELECT @@IDENTITY;").QuerySingle<int>();
                                }
                            }

                            editArgs.executeValue = rowsAffected;
                            this.OnAfterEditMaster(editArgs);
                        }
                        // }
                    }

                    if (listWrapper != null && data["list"] != null)
                    {
                        #region 定义变量

                        var types = new Dictionary<string, OptType>
                        {
                            {"deleted", OptType.Del},
                            {"updated", OptType.Mod},
                            {"inserted", OptType.Add}
                        };

                        var handles = new Dictionary<string, Func<RequestWrapper, int>>
                        {
                            {"deleted", x => dbContext.Sql(BuilderDelSql(x.ToParamDelete())).Execute()},
                            {"updated", x => dbContext.Sql(BuilderParse(x.ToParamUpdate())).Execute()},
                            {"inserted", x => dbContext.Sql(BuilderParse(x.ToParamInsert())).Execute()}
                        };
                        #endregion

                        editArgs.wrapper = listWrapper;

                        foreach (JProperty item in data["list"].Children())
                        {
                            if (!handles.ContainsKey(item.Name)) continue;

                            foreach (var row in item.Value.Children())
                            {
                                editArgs.row = row;
                                editArgs.type = types[item.Name];
                                var rtnBefore = this.OnBeforEditDetail(editArgs);
                                if (!rtnBefore) continue;

                                editArgs.executeValue = handles[item.Name](listWrapper.SetRequestData(row));
                                rowsAffected += editArgs.executeValue;
                                this.OnAfterEditDetail(editArgs);
                            }
                        }
                    }

                    editArgs.executeValue = rowsAffected;
                    //editArgs.wrapper = formWrapper;
                    //this.OnAfterEdit(editArgs);
                    if (rowsAffected > 0)
                    {
                        dbContext.Commit();
                        if (editArgs.IdentityVal > 0)
                        {
                            rowsAffected = editArgs.IdentityVal;
                        }

                        //Msg.Set(MsgType.Success, APP.MSG_SAVE_SUCCESS);
                    }
                }, e => dbContext.Rollback());
            }

            return rowsAffected;
        }

        public bool SaveExchangedData(RequestWrapper requestWrapper, JArray jdata, ServiceBase service, string dateColumn)
        {
            if (requestWrapper == null || jdata == null) return false;

            var rowsAffected = 0;
            var sqlTest = "";
            var dbContext = service.db.UseTransaction(true);
            try
            {
                using (dbContext)//开启事务
                {
                    //Logger("数据交换", () =>
                    //{
                    foreach (JToken row in jdata)
                    {
                        var pUpdate = requestWrapper.SetRequestData(row).ToParamUpdate();
                        var editArgs = new EditEventArgs { form = row, db = dbContext, TableName = pUpdate.GetData().Update };
                        var editBefore = OnBeforEditMaster(editArgs);
                        if (!editBefore) continue;

                        var sqlData = pUpdate.GetData();
                        var selSql = string.Format("SELECT {0} FROM {1} WHERE {2}", dateColumn, sqlData.Update,
                            sqlData.WhereSql);
                        var updateDate = dbContext.Sql(selSql).QuerySingle<string>();
                        if (updateDate != null)//更新
                        {
                            //日期大于数据库中的日期
                            if (Convert.ToDateTime(row[dateColumn]) > Convert.ToDateTime(updateDate))
                            {
                                sqlTest = BuilderParse(pUpdate);
                                rowsAffected += dbContext.Sql(BuilderParse(pUpdate)).Execute();
                            }
                        }
                        else//新增
                        {
                            rowsAffected += dbContext.Sql(BuilderParse(requestWrapper.ToParamInsert())).Execute();
                        }

                        OnAfterEditMaster(editArgs);
                    }

                    dbContext.Commit();
                }
            }
            catch(Exception ex)
            {
                rowsAffected = -1;
                dbContext.Rollback();
                throw new Exception(ex.StackTrace);
            }

            return rowsAffected >= 0;
        }



        private Dictionary<string, OptType> types = new Dictionary<string, OptType>
                        {
                            {"deleted", OptType.Del},
                            {"updated", OptType.Mod},
                            {"inserted", OptType.Add}
                        };

        public int EditFormList(RequestWrapper formWrapper, RequestWrapper listWrapper, JObject data)
        {
            var rowsAffected = 0;
            var dbContext = db.UseTransaction(true);
            using (dbContext)//开启事务
            {
                Logger("编辑记录", () =>
                {
                    var result1 = EditForm(formWrapper, data, dbContext);
                    var result2 = EditList(listWrapper, data, dbContext);
                    if (result1 > 0 || result2 > 0)
                    {
                        rowsAffected = result1 + result2;
                        dbContext.Commit();
                    }
                }, e => dbContext.Rollback());
            }

            return rowsAffected;
        }

        private int EditForm(RequestWrapper formWrapper, JObject data, IDbContext dbContext)
        {
            if (formWrapper == null || data["form"] == null) return 0;

            var rowsAffected = 0;
            var editArgs = new EditEventArgs { db = dbContext, form = data["form"] };
            editArgs.wrapper = formWrapper;
            var rtnBefore = this.OnBeforEditMaster(editArgs);
            if (rtnBefore)
            {
                editArgs.type = OptType.Mod;
                var pUpdate = formWrapper.SetRequestData(editArgs.form).ToParamUpdate(); //在before事件中更改了form中的值，刷新更新时的值
                rowsAffected = dbContext.Sql(BuilderParse(pUpdate)).Execute();
                if (rowsAffected == 0)
                {
                    editArgs.type = OptType.Add;
                    rowsAffected = dbContext.Sql(BuilderParse(formWrapper.ToParamInsert())).Execute();
                }

                editArgs.executeValue = rowsAffected;
                this.OnAfterEditMaster(editArgs);
            }

            return rowsAffected;
        }

        private int EditList(RequestWrapper listWrapper, JObject data, IDbContext dbContext)
        {
            if (listWrapper == null || data["list"] == null) return 0;

            var rowsAffected = 0;
            var editArgs = new EditEventArgs
            {
                db = dbContext,
                list = data["list"],
                wrapper = listWrapper
            };
            foreach (JProperty item in data["list"].Children())
            {
                if (!types.ContainsKey(item.Name)) continue;

                foreach (var row in item.Value.Children())
                {
                    editArgs.row = row;
                    editArgs.type = types[item.Name];
                    var rtnBefore = this.OnBeforEditDetail(editArgs);
                    if (!rtnBefore) continue;

                    var wrapper = listWrapper.SetRequestData(row);
                    if (types[item.Name] == OptType.Del)
                    {
                        editArgs.type = OptType.Del;
                        editArgs.executeValue = dbContext.Sql(BuilderDelSql(wrapper.ToParamDelete())).Execute();
                    }
                    else
                    {
                        //update
                        editArgs.type = OptType.Mod;
                        editArgs.executeValue = dbContext.Sql(BuilderParse(wrapper.ToParamUpdate())).Execute();
                        if (editArgs.executeValue < 1)
                        {
                            // insert
                            editArgs.type = OptType.Add;
                            editArgs.executeValue = dbContext.Sql(BuilderParse(wrapper.ToParamInsert())).Execute();
                        }
                    }
                    rowsAffected += editArgs.executeValue;
                    this.OnAfterEditDetail(editArgs);
                }
            }

            return rowsAffected;
        }
    }
}
