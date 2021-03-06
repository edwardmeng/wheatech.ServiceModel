﻿using System;
using System.Web.Mvc;
using System.Web.Routing;
using MassActivation;
using ServiceBridge.Sample.Components;

namespace ServiceBridge.Samples.Mvc
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            ApplicationActivator.UseEnvironment(EnvironmentName.Development).Startup();
            RegisterRoutes(RouteTable.Routes);
            ServiceContainer.GetInstance<ICacheRepository>().SetVale("Sample", "ServiceBridge");
        }

        private void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Wisdom.Platform.Controllers" }
                );
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}