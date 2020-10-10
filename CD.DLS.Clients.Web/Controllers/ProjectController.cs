using CD.DLS.Clients.Web.Models;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class ProjectController : BaseController
    {
        // GET: Project
        public ActionResult Default()
        {
            ProjectConfigManager pcm = new ProjectConfigManager(NetBridge);
            List<ProjectConfig> configs = pcm.ListProjectConfigs();
            return PartialView(configs);
        }

        public void Select(string argument1)
        {
            var pcm = new ProjectConfigManager(NetBridge);
            var config = pcm.GetProjectConfig(Guid.Parse(argument1));
            SessionManager sm = new SessionManager(Session);
            sm.ProjectConfig = config;
        }
    }
}