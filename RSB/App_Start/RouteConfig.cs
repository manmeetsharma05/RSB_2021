using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Rajya_Sanik_Board
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional });

            routes.MapRoute(
               name: "Default1",
                      url: "{controller}/{action}/{id}",
                     defaults: new { controller = "Home", action = "RSB_Registration", id = UrlParameter.Optional });

            routes.MapRoute(
              name: "Default2",
                     url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "RSBGeneralInfrm", id = UrlParameter.Optional });

            routes.MapRoute(
              name: "Default3",
                     url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "Schemes", id = UrlParameter.Optional });

            routes.MapRoute(
              name: "Default4",
                     url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "Edit", id = UrlParameter.Optional });

            routes.MapRoute(
             name: "Default5",
                    url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Home", action = "Editmed", id = UrlParameter.Optional });

            routes.MapRoute(
            name: "Default6",
                   url: "{controller}/{action}/{id}",
                  defaults: new { controller = "Home", action = "Medical_Status", id = UrlParameter.Optional });

            routes.MapRoute(
            name: "Default7",
                   url: "{controller}/{action}/{id}",
                  defaults: new { controller = "Home", action = "Award", id = UrlParameter.Optional });

            routes.MapRoute(
            name: "Default8",
                   url: "{controller}/{action}/{id}",
                  defaults: new { controller = "Home", action = "Editawd", id = UrlParameter.Optional });

            routes.MapRoute(
           name: "Default9",
                  url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Home", action = "Editrank", id = UrlParameter.Optional });

            routes.MapRoute(
           name: "Default10",
                  url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Home", action = "Rank", id = UrlParameter.Optional });

            routes.MapRoute(
          name: "Default11",
                 url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Editreg", id = UrlParameter.Optional });

            routes.MapRoute(
          name: "Default12",
                 url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Regcorps", id = UrlParameter.Optional });

            routes.MapRoute(
          name: "Default13",
                 url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Pension", id = UrlParameter.Optional });

            routes.MapRoute(
          name: "Default14",
                 url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Editpen", id = UrlParameter.Optional });
            routes.MapRoute(
                name: "Default15",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Editdepen", id = UrlParameter.Optional });
           
             routes.MapRoute(
                name: "Default19",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional });
             routes.MapRoute(
                name: "Default23",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Report", action = "DistviseReport", id = UrlParameter.Optional });
             routes.MapRoute(
                 name: "Default24",
                 url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Report", action = "CallReport", id = UrlParameter.Optional });
             routes.MapRoute(
             name: "Default25",
                    url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Home", action = "Report", id = UrlParameter.Optional });
             routes.MapRoute(
             name: "Default26",
                    url: "{controller}/{action}/{id}",
                   defaults: new { controller = "Home", action = "martyr", id = UrlParameter.Optional });
             routes.MapRoute(
              name: "Default27",
                     url: "{controller}/{action}/{id}",
                    defaults: new { controller = "Home", action = "RSBGeneralInfrm_citizen", id = UrlParameter.Optional });
             routes.MapRoute(
               name: "Default28",
                      url: "{controller}/{action}/{id}",
                     defaults: new { controller = "Home", action = "shortDistrictwiseReport", id = UrlParameter.Optional });


        }
    }
}