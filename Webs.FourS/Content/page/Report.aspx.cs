using System;
using System.Configuration;
using System.Web.UI;
using Stimulsoft.Report;
using Stimulsoft.Report.Web;

namespace GuoLong.Web
{
    public partial class Report : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            StiReport report = GetReport();
            StiWebViewer1.Report = report;
        }

        private StiReport GetReport()
        {
            var report = new StiReport();
            report.Load(GetReportPath());
            ChangeConnectString(report);
            report.Compile();
            SetReportParamaters(report);
            return report;
        }

        private void SetReportParamaters(StiReport report)
        {
            var dataSource = report.CompiledReport.DataSources;
            foreach (Stimulsoft.Report.Dictionary.StiDataSource ds in dataSource)
            {
                var param = Request.QueryString;
                foreach (string key in param.Keys)
                {
                    if (!ds.Parameters.Contains(key)) continue;
                    var p = ds.Parameters[key];
                    var v = param[key];
                    p.ParameterValue = v;
                }
            }
        }

        private string GetReportPath()
        {
            var path = String.Format("~/Areas/hbbuy/Reports/{0}.mrt", Request["rpt"]);
            path = Server.MapPath(path);
            if (!System.IO.File.Exists(path))
                path = Server.MapPath("~/Content/page/reports/helloworld.mrt");
            return path;
        }

        private void ChangeConnectString(StiReport report)
        {
            foreach (Stimulsoft.Report.Dictionary.StiSqlDatabase item in report.Dictionary.Databases)
            {
                var prefix = item.Name.Split('_')[0];
                item.ConnectionString = ConfigurationManager.ConnectionStrings[prefix].ConnectionString;
            }
        }

        private string GetRouteValue(string name)
        {
            var oParam = Page.RouteData.Values[name];
            var value = (oParam == null || oParam.ToString() == string.Empty) ? string.Empty : oParam.ToString();
            return value;
        }

    }
}