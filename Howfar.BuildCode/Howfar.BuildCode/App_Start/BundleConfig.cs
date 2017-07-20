using System.Web;
using System.Web.Optimization;

namespace Howfar.BuildCode
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/JavaScript").Include(
                        "~/Scripts/*.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            ///Ace 代码编辑器
            bundles.Add(new ScriptBundle("~/Ace").Include(
                "~/Scripts/ace/ace.js",
                "~/Scripts/ace/ext-language_tools.js",
                "~/Scripts/ace/c_cpp.js",
                "~/Scripts/ace/text.js",
                "~/Scripts/ace/mode-c_cpp.js",
                "~/Scripts/ace/theme-clouds.js"));



            bundles.Add(new StyleBundle("~/Content").Include(
                      "~/Content/*.css"));
        }
    }
}
