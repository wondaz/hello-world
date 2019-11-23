/*************************************************
 * 文件名称 ：RequestWrapperConvert.cs                          
 * 描述说明 ：请求包装 参数转换处理
**************************************************/
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Frame.Core
{
    public partial class RequestWrapper
    {
        #region 私有方法
        private void parseColumns(XElement settings, Action<string, object> Column)
        {
            var columns = settings.Element("columns");
            var ignoreCols = columns != null ? getXmlElementAttr(columns, "ignore").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            var includeCols = columns != null ? getXmlElementAttr(columns, "include").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries) : new string[0];
            ignores.AddRange(ignoreCols);

            foreach (var key in includeCols)
            {
                Column(key, this[key]);
            }

            foreach (var key in this.Except(ignores))
            {
                Column(key, this[key]);                    
            }
        }

        private void ParseWhere(XElement settings, Action<string, string, Func<WhereData, string>, string, object[]> andWhere)
        {
            var where = settings.Element("where");
            if (where == null) return;

            var defaultIgnoreEmpty = string.Equals(getXmlElementAttr(where, "defaultIgnoreEmpty"), "true", StringComparison.OrdinalIgnoreCase);
            var handledList = new List<string>();
            foreach (var item in where.Elements("field"))
            {
                var cp = CpHelper.Parse(getXmlElementAttr(item, "cp"));
                var ignoreEmpty = getXmlElementAttr(item, "ignoreEmpty");
                var isIgnoreEmpty = ignoreEmpty == string.Empty ? defaultIgnoreEmpty : string.Equals(ignoreEmpty, "true", StringComparison.OrdinalIgnoreCase);
                var name = getXmlElementAttr(item, "name");
                var variable = getXmlElementAttr(item, "variable", name);
                var value = getXmlElementAttr(item, "value");
                if (string.IsNullOrEmpty(value))
                {
                    value = this[variable];
                }

                var extend = getXmlElementAttr(item, "extend").Split(',');
                if (!string.IsNullOrEmpty(value) || !isIgnoreEmpty)
                {
                    andWhere(GetFieldName(name, true), value, cp, variable, extend);
                }

                handledList.Add(GetFieldName(variable));
                handledList.Add(GetAliasName(variable));
            }

            var defaultForAll = string.Equals(getXmlElementAttr(where, "defaultForAll"), "true", StringComparison.OrdinalIgnoreCase);
            if (defaultForAll)
            {
                var unHandledKeys = this.Except(handledList);
                foreach (var name in unHandledKeys)
                {
                    //var value = this[name];
                    //if (!string.IsNullOrEmpty(value) || !defaultIgnoreEmpty)
                    //{
                    andWhere(name, this[name], Cp.Equal, name, new object[] { });
                    //}
                }
            }
        }
        #endregion

        #region ToParamQuery
        public ParamQuery ToParamQuery()
        {
            var pQuery = new ParamQuery();
            var settings = XElement.Parse(settingXml);
            var defaultOrderBy = getXmlElementAttr(settings, "defaultOrderBy");
            pQuery.Select(getXmlElementValue(settings, "select"));

            //获取分页及排序信息
            var page = parseInt(request["page"], 1);
            var rows = parseInt(request["rows"]);
            var orderby = string.Join(" ", GetFieldName(this["sort"], true), this["order"]).Trim();
            if (string.IsNullOrEmpty(orderby))
            {
                orderby = defaultOrderBy;
            }

            var sFrom = getXmlElementValue(settings, "from");
            var sGroupby = getXmlElementValue(settings, "groupby");
            pQuery.From(sFrom).Paging(page, rows).OrderBy(orderby).GroupBy(sGroupby);
            ParseWhere(settings, (name, value, compare, variable, extend) =>
            {
                pQuery.AndWhere(name, value, compare, extend);
            });

            return pQuery;
        }

        private int parseInt(string str, int defaults = 0)
        {
            int value;
            if (!int.TryParse(str, out value)) value = defaults;
            return value;
        }
        #endregion

        #region ToParamUpdate
        public ParamUpdate ToParamUpdate()
        {
            var settings = XElement.Parse(settingXml);
            var pUpdate = ParamUpdate.Instance().Update(getXmlElementValue(settings, "table"));

            var list = new List<string>();
            ParseWhere(settings, (name, value, compare, variable, extend) =>
            {
                pUpdate.AndWhere(name, value, compare, extend);
                list.Add(variable);
            });

            parseColumns(settings, (name, value) =>
            {
                if (list.IndexOf(name) < 0)
                    pUpdate.Column(name, value);
            });
            return pUpdate;
        }
        #endregion

        #region ToParamInsert
        public ParamInsert ToParamInsert()
        {
            var settings = XElement.Parse(settingXml);
            var pInsert = ParamInsert.Instance().Insert(getXmlElementValue(settings, "table"));
            parseColumns(settings, (name, value) => pInsert.Column(name, value));
            return pInsert;
        }
        #endregion

        #region ToParamDelete
        public ParamDelete ToParamDelete()
        {
            var settings = XElement.Parse(settingXml);
            var pDelete = ParamDelete.Instance().From(getXmlElementValue(settings, "table"));
            ParseWhere(settings, (name, value, compare, variable, extend) => pDelete.AndWhere(name, value, compare, extend));

            return pDelete;
        }
        #endregion
    }
}