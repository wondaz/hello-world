/*****************************************
 * 文件名称 ：RequestWrapperMethods.cs                          
 * 描述说明 ：请求包装 方法
*****************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace Frame.Core
{
    public partial class RequestWrapper
    {
        private IEnumerable<string> Except(List<string> keys = null)
        {
            keys = keys ?? new List<string>();
            var result = request.AllKeys
                       .Except(ignores, StringComparer.CurrentCultureIgnoreCase)
                       .Except(keys, StringComparer.CurrentCultureIgnoreCase)
                       .Where(x => !x.StartsWith(ignoreStartWith)).ToList();
            return result;
        }

        public RequestWrapper LoadSettingXmlString(string xml, params object[] param)
        {
            settingXml = param.Length > 0 ? string.Format(xml, param) : xml;
            _alias = null;
            _fields = null;
            _variable = null;
            return this;
        }

        //public RequestWrapper LoadSettingXml(string url, params object[] param)
        //{
        //    var xml = XElement.Load(HttpContext.Current.Server.MapPath(url)).ToString();
        //    settingXml = param.Length > 0 ? string.Format(xml, param) : xml;
        //    return this;
        //}

        public RequestWrapper LoadXmlByUrl(string url, string nodeName = "", params object[] param)
        {
            var root = XElement.Load(HttpContext.Current.Server.MapPath(url));
            XElement xmlNode = null;
            if (!string.IsNullOrEmpty(nodeName))
            {
                xmlNode = root.Elements("settings").FirstOrDefault(p => p.Attribute("name").Value.ToUpper() == nodeName.ToUpper());
            }

            if (xmlNode != null)
            {
                settingXml = xmlNode.ToString();
                if (param.Length > 0)
                {
                    settingXml = string.Format(settingXml, param);
                }
            }
            else
            {
                settingXml = root.ToString();
            }

            return this;
        }

        public RequestWrapper LoadXmlByPath(string path, string nodeName, params object[] param)
        {
            var root = XElement.Load(path);
            XElement xmlNode = root.Elements("settings").FirstOrDefault(p => p.Attribute("name").Value.ToUpper() == nodeName.ToUpper());
            if (xmlNode != null)
            {
                settingXml = xmlNode.ToString();
                if (param.Length > 0)
                {
                    settingXml = string.Format(settingXml, param);
                }
            }
            else
            {
                settingXml = "";
            }

            return this;
        }

        public RequestWrapper LoadXmlByPath(string path, params object[] param)
        {
            settingXml = XElement.Load(path).ToString();
            if (param.Length > 0)
            {
                settingXml = string.Format(settingXml, param);
            }

            return this;
        }

        public void UpdateSettingXml(params object[] param)
        {
            settingXml = string.Format(settingXml, param);
        }

        private RequestWrapper SetRequestData(NameValueCollection values)
        {
            request = values;
            return this;
        }

        public RequestWrapper SetRequestData(JToken values)
        {
            if (values == null) return this;

            foreach (var jToken in values.Children())
            {
                var item = (JProperty)jToken;
                if (item == null) continue;

                this[item.Name] = ((JValue)item.Value).Value == null ? null : item.Value.ToString();
            }

            return this;
        }

        public RequestWrapper SetRequestData(string name, string value)
        {
            this[name] = value;
            return this;
        }

        public string GetXmlNodeValue(string name)
        {
            var settings = XElement.Parse(settingXml);
            return getXmlElementValue(settings, name);
        }

        #region 私有方法
        private string getXmlElementAttr(XElement element, string attri, string defaultStr = "")
        {
            return element.Attribute(attri) == null ? defaultStr : element.Attribute(attri).Value;
        }

        private string getXmlElementValue(XElement element, string name)
        {
            return element.Element(name) == null ? string.Empty : element.Element(name).Value;
        }

        private NameValueCollection _alias;
        private NameValueCollection _fields;
        private NameValueCollection _variable;
        public string GetAliasName(string field)
        {
            if (_alias == null)
                InitFieldAliasVariable("alias");

            return _alias[field] ?? field;
        }
        public string GetFieldName(string alias, bool withTable = false)
        {
            if (string.IsNullOrEmpty(alias))
                return string.Empty;

            if (_fields == null)
                InitFieldAliasVariable("field");

            var prefix = string.Empty;
            if (alias.IndexOf(".") >= 0)
            {
                var arr = alias.Split('.');
                if (withTable) prefix = arr[0] + ".";
                alias = arr[1];
            }
            return prefix + (_fields[alias] ?? alias);
        }

        public string GetVariableName(string field)
        {
            if (_variable == null)
                InitFieldAliasVariable("variable");

            return _variable[field] ?? field;
        }
        private void InitFieldAliasVariable(string initType)
        {
            switch (initType)
            {
                case "alias":
                case "field":
                    _alias = new NameValueCollection();
                    _fields = new NameValueCollection();
                    if (string.IsNullOrEmpty(this.settingXml)) return;
                    var select = this.GetXmlNodeValue("select") ?? string.Empty;    //处理类型 projectName as text;   text = 'xxx'作用条件时的情况
                    select.Replace("\r", "").Replace("\n", "").Split(',').ToList().Where(x => x.ToLower().IndexOf(" as ") >= 0).ToList().ForEach(x =>
                    {
                        var array = Regex.Replace(x, @"\s+", " ").Trim().Split(' ');
                        string field = array[0], alias = array[2];
                        _alias.Add(field, alias);
                        _fields.Add(alias, field);
                    });
                    break;
                case "variable":
                    _variable = new NameValueCollection();
                    if (string.IsNullOrEmpty(this.settingXml)) return;
                    var wheres = XElement.Parse(settingXml).Element("where");
                    if (wheres != null)
                    {
                        foreach (var item in wheres.Elements("field"))
                        {
                            var name = getXmlElementAttr(item, "name");
                            var variable = getXmlElementAttr(item, "variable", name);
                            if (name != variable) _variable[name] = variable;
                        }
                    }
                    break;
            }
        }
        #endregion
    }
}