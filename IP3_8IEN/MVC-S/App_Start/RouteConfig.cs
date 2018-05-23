﻿using System.Web.Mvc;
using System.Web.Routing;

namespace IP_8IEN.UI.MVC_S
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
                //defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
