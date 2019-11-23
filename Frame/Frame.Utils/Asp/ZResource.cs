using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Resources;

namespace Frame.Utils
{
    public static class ZResource
    {

        private static ResourceManager m_resourceManager;

        static ZResource()
        {
            m_resourceManager = new ResourceManager("ResourceNamespace.myResources", typeof(ZResource).Assembly);
        }

        public static string GetMessage(string messageCode)
        {
            string FieldName = "";
            if (messageCode == null || messageCode.Trim().Equals(""))
            {
                return "";
            }

            try { FieldName = HttpContext.GetGlobalResourceObject("Message", messageCode).ToString(); }
            catch { FieldName = messageCode; }
            return FieldName;
        }

        public static string GetMessage(string messageCode, params object[] Parms)
        {
            string msg = GetMessage(messageCode);
            if (Parms != null) msg = String.Format(msg, Parms);
            return msg;
        }

        public static string GetResource(string Resource, string Field)
        {
            string FieldName = "";
            try { FieldName = HttpContext.GetGlobalResourceObject(Resource, Field).ToString(); }
            catch { FieldName = Field; }
            return FieldName;
        }

    }
}
