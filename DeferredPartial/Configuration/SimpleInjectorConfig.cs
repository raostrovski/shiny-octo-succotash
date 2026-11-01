using DeferredPartial.CrossCutting.Contracts;
using DeferredPartial.Services.Resolvers;
using DeferredPartial.Services.Stores;
using SimpleInjector;
using System.Reflection;


namespace DeferredPartial.MVC.Configuration
{
    public static class SimpleInjectorConfig
    {
        public static void Register(Container container)
        {
            container.Register<IDeferredPartialStore, DeferredPartialStore>(Lifestyle.Singleton);

            container.Register<DeferredViewResolver>(
                Lifestyle.Singleton
            );

            container.RegisterMvcControllers(
                Assembly.GetExecutingAssembly()
            );
        }
    }
}