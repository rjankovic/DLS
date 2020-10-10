using CD.DLS.Clients.Web.Models;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class WarningsController : BaseController
    {
        // GET: Warnings
        public ActionResult Default()
        {
            var inspectManager = new InspectManager(NetBridge);
            var warningMessages = inspectManager.GetWarningMessages(ProjectConfig.ProjectConfigId);
            var warningsWrap = new WarningsWrap()
            {
                Warnings = warningMessages,
                TileId = Guid.NewGuid().ToString()
            };

            return PartialView("Default", warningsWrap);
        }
    }
}