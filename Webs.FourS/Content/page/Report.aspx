<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="GuoLong.Web.Report" %>

<%@ Register  TagPrefix="cc1" Namespace="Stimulsoft.Report.Web" Assembly="Stimulsoft.Report.Web, Version=2018.2.3.0, Culture=neutral, PublicKeyToken=ebe6666cba19647a"%>

<!doctype html>
<html>
    <head runat="server">
        <title></title>
    </head>
    <body style="background-color: #e8e8e8">
        <form id="form1" runat="server">
        <div style="width: 960px;margin: 0 auto;">
           
            <cc1:StiWebViewer ID="StiWebViewer1" runat="server"  GlobalizationFile="/Content/page/reports/Localization/zh-CHS.xml" ShowDesignButton="False"  
              Theme="Office2007Blue"  BackColor="#e8e8e8"/>         
        </div>
        </form>
    </body>
</html>
