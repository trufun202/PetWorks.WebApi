using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Dispatcher;
using Autofac;
using Autofac.Integration.WebApi;
using PetWorks.Shared.Services.Interfaces;

namespace PetWorks.WebApi
{
    public class PetWorksAssemblyResolver : DefaultAssembliesResolver
    {

        private IList<string> _assemblyPathList;

        public PetWorksAssemblyResolver(Type type)
        {
            this._assemblyPathList = new List<string>
            {
                type.Assembly.Location
            };
        }

        public PetWorksAssemblyResolver(IList<Type> typeList)
        {
            this._assemblyPathList = new List<string>();
            typeList.ToList().ForEach(x =>
            {
                if (!this._assemblyPathList.Contains(x.Assembly.Location))
                {
                    this._assemblyPathList.Add(x.Assembly.Location);
                }
            });
        }

        public PetWorksAssemblyResolver(string assembiesPath)
        {
            this._assemblyPathList = new List<string> { assembiesPath };
        }

        public PetWorksAssemblyResolver(IList<string> assemblyPathList)
        {
            this._assemblyPathList = assemblyPathList;
        }

        public override ICollection<Assembly> GetAssemblies()
        {   
            ICollection<Assembly> baseAssemblies = base.GetAssemblies();
            List<Assembly> assemblies = new List<Assembly>(baseAssemblies);
            if (this._assemblyPathList != null)
            {
                this._assemblyPathList.ToList().ForEach(path =>
                {
                    var directoryInfo = new DirectoryInfo(Path.Combine(path, "Assemblies"));

                    foreach (var file in directoryInfo.GetFiles("*.pet"))
                    {
                        var assembly = Assembly.LoadFrom(file.FullName);
                        assemblies.Add(assembly);

                        WebApiConfig.RegisterAssemblyTypes(assembly);
                        WebApiConfig.Builder.RegisterApiControllers(assembly);
                    }
                });
            }
            WebApiConfig.FinalizeConfig();

            return assemblies;
        }
    }
}