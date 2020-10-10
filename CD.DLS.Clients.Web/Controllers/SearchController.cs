using CD.DLS.Clients.Web.Models;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class SearchController : BaseController
    {
        public ActionResult Default()
        {
            var guid = Guid.NewGuid().ToString();
            return PartialView("Default", guid);
        }

        public ActionResult Query(string argument1, string argument2)
        {
            var query = argument1;
            var tileId = Guid.Parse(argument2);
            var sm = new SearchManager(NetBridge);
            var childTypes = sm.GetParentChildTypeMapping().Select(x => x.ChildType).Distinct().ToList();
            var searchResults = sm.FindFulltext(ProjectConfig.ProjectConfigId, query, "", childTypes);
            
            return PartialView("Results", new FulltextSearchResults
            {
                Results = searchResults,
                TileId = tileId
            });
        }
    }
}