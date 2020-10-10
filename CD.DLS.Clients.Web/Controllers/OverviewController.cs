using CD.DLS.Clients.Web.Models.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class OverviewController : BaseController
    {
        public ActionResult Default()
        {
            var guid = Guid.NewGuid().ToString();
            return PartialView("Default", guid);
        }

        public ActionResult GetDiagram()
        {
            DiagramLoader loader = new DiagramLoader(NetBridge);
            var diagram = loader.LoadDiagram(ProjectConfig.ProjectConfigId, Common.Structures.DependencyGraphKind.DFHigh);
            return Json(diagram, JsonRequestBehavior.AllowGet);
        }
    }
}