using System.Web.Optimization;

namespace Web.FourS
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            ResetIgnorePatterns(bundles.IgnoreList);
            bundles.Add(new ScriptBundle("~/Scripts/library").Include(
                "~/Scripts/jquery/jquery-3.4.1.min.js",
                "~/Scripts/jquery/jquery-browser.js",
                "~/Scripts/jquery/jquery.cookie.js",
                "~/Scripts/core/utils.js",
                "~/Scripts/core/common.js",
                "~/Scripts/core/knockout-{version}.js", 
                "~/Scripts/core/knockout.mapping.js", 
                "~/Scripts/core/knockout.bindings.js",
                "~/Scripts/jquery-plugin/showloading/jquery.showLoading.min.js",
                "~/Scripts/core/jquery.easyui.fix.js"
                //"~/Scripts/chartjs/echarts.js",
                //"~/Scripts/chartjs/china.js"
                ));

            bundles.Add(new ScriptBundle("~/Scripts/index").Include(
                "~/Scripts/jquery/jquery-3.4.1.min.js",
                "~/Scripts/jquery/jquery-browser.js",
                "~/Scripts/jquery/jquery.cookie.js",
                "~/Scripts/core/utils.js",
                "~/Scripts/core/common.js",
                "~/Scripts/core/jquery.easyui.fix.js",
                "~/Scripts/jquery-plugin/jnotify/jquery.jnotify.js",
                "~/Scripts/jquery-plugin/showloading/jquery.showLoading.min.js",
                "~/Scripts/jquery-plugin/ztree/jquery.ztree.all-3.2.min.js",
                "~/Scripts/viewModel/index.js"
                
                ));
        }

        public static void ResetIgnorePatterns(IgnoreList ignoreList)
        {
            ignoreList.Clear();
            ignoreList.Ignore("*.intellisense.js");
            ignoreList.Ignore("*-vsdoc.js");
            ignoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
            //ignoreList.Ignore("*.min.js", OptimizationMode.WhenDisabled);
            ignoreList.Ignore("*.min.css", OptimizationMode.WhenDisabled);
        }

    }
}