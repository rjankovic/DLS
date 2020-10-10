using System.Web;
using System.Web.Optimization;

namespace CD.DLS.Clients.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/waitMe.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/d3").Include(
                        "~/Scripts/d3.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/contextmenu").Include(
                        "~/Scripts/ContextMenu/jquery.contextMenu.min.js",
                        "~/Scripts/ContextMenu/jquery.ui.position.js",
                        "~/Scripts/jstree.min.js",
                        "~/Scripts/jquery.splitter.js",
                        "~/Scripts/datatables.min.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                      "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/App/commonUi.js",
                      "~/Scripts/App/navigationActions.js",
                      "~/Scripts/App/projectActions.js",
                      "~/Scripts/App/diagramRenderer.js",
                      "~/Scripts/App/treeView.js",
                      "~/Scripts/App/elementView.js",
                      "~/Scripts/App/stFlowActions.js"
                      ));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css",
                      "~/Content/waitMe.min.css",
                      "~/Content/ContextMenu/jquery.contextMenu.min.css",
                      "~/Content/jquery-ui.min.css",
                      "~/Content/jquery-ui.structure.min.css",
                      "~/Content/jquery-ui.min.css",
                      "~/Content/jstree.css",
                      "~/Content/jquery.splitter.css",
                      "~/Content/datatables.min.css"));
        }
    }
}
