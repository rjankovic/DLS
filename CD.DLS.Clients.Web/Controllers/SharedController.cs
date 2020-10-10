using CD.DLS.Clients.Web.Models;
using CD.DLS.DAL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    [Authorize]
    public class SharedController : BaseController
    {
        public ActionResult MainMenu()
        {
            if (UserData == null)
            {
                return PartialView("_MainMenuPartial", new MainMenu() { Items = new List<MenuItem>() });
            }
            var menu = new MainMenu()
            {
                Items = new List<MenuItem>()
                {
                    new MenuItem() { Name = ProjectConfig == null ? "Project" : ProjectConfig.Name, Action = "project" }
                }
            };
            
            if (ProjectConfig != null)
            {
                menu.Items.AddRange(new List<MenuItem>()
                    {
                    new MenuItem() { Name = "Overview", Action = "overview" },
                    new MenuItem() { Name = "ST Flow", Action = "stflow" },
                    new MenuItem() { Name = "Search", Action = "search" },
                    new MenuItem() { Name = "Warnings", Action = "warnings" }
                });
            }

            /*
            menu.Message = string.Format(
                "User {0} | AID {1} | DBID {2} | GRP {3}",
                UserData.DisplayName, UserData.Identity, UserData.UserId, string.Join(", ", UserData.Groups.Select(x => x.Name)));
            */

            return PartialView("_MainMenuPartial", menu);

        }

        public ActionResult Errors()
        {
            if (UserData == null)
            {
                return PartialView("_ErrorsPartial", "The user does not have a vaild license. Please log in with another account.");
            }
            
            return PartialView("_ErrorsPartial", null);

        }
    }
}