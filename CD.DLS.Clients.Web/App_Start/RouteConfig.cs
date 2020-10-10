using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CD.DLS.Clients.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{argument1}/{argument2}",
                defaults: new { controller = "Home", action = "Index", argument1 = UrlParameter.Optional, argument2 = UrlParameter.Optional }
            );
        }
    }
}
