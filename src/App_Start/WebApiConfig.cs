using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Autofac;
using Autofac.Integration.WebApi;
using PetWorks.Shared.Services.Interfaces;

namespace PetWorks.WebApi
{
    public static class WebApiConfig
    {
        public static ContainerBuilder Builder { get; private set; }
        public static IContainer Container { get; set; }

        public static HttpConfiguration Config { get; set; }

        public static void Register(HttpConfiguration config)
        {
            Config = config;

            //Autofac
            Builder = new ContainerBuilder();
            Builder.RegisterWebApiFilterProvider(config);
            WebApiConfig.RegisterAssemblyTypes(typeof(ILoggingService).Assembly);

            // Web API configuration and services
            GlobalConfiguration
                .Configuration
                .Services
                .Replace
                (
                    typeof(IAssembliesResolver),
                    new PetWorksAssemblyResolver(HttpRuntime.AppDomainAppPath)
                );
            
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        public static void RegisterAssemblyTypes(Assembly assembly)
        {
            Builder
                .RegisterAssemblyTypes(assembly)
                .Where(x => x.Name.EndsWith("Service"))
                .AsImplementedInterfaces()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies)
                .SingleInstance();
        }

        public static void FinalizeConfig()
        {
            Container = Builder.Build();
            Config.DependencyResolver = new AutofacWebApiDependencyResolver(Container);
        }
    }
}
