using Microsoft.Extensions.Caching.Memory;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VehicleFitmentAPI.Controllers;
using VehicleFitmentAPI.Services;

namespace VehicleFitmentAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private void ConfigureApi()
        {
            var container = new SimpleInjector.Container();
            GlobalConfiguration.Configuration.DependencyResolver =
                          new SimpleInjectorWebApiDependencyResolver(container);
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var container = new SimpleInjector.Container();

            container.Options.AllowOverridingRegistrations = true;

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["VehicleConnection"].ConnectionString;
            container.Register<IDatabaseService>(() => new DatabaseService(connectionString), Lifestyle.Singleton);
            container.Register<DatabaseService>(() => new DatabaseService(connectionString), Lifestyle.Singleton);
            container.Register(() => new MemoryCache(new MemoryCacheOptions()), Lifestyle.Singleton);
            container.Register<IMemoryCache>(() => container.GetInstance<MemoryCache>(), Lifestyle.Singleton);
            container.Register<PartsController>(Lifestyle.Transient);
            container.Register<VehicleController>(Lifestyle.Transient);
            container.Register<FitmentController>(Lifestyle.Transient);
            container.Register<HomeController>(Lifestyle.Singleton);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);


            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
