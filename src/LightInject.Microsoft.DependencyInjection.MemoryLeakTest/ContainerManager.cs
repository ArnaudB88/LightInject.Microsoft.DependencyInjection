using AutoMapper;
using System.Reflection;

namespace LightInject.MemoryLeakTest
{
    public static class ContainerManager
    {
        public static IServiceContainer Container { get; private set; }

        public static void Bootstrap(IServiceContainer existingContainer = null)
        {
            InitializeContainer(existingContainer);
            InitializeDtoMapping();
        }

        public static void Teardown()
        {
            lock (ContainerSyncObject)
            {
                if (_containerIsConfigured)
                {
                    Container.Dispose();
                    _containerIsConfigured = false;
                }
            }
        }

        private static bool _containerIsConfigured;
        private static readonly object ContainerSyncObject = new object();
        private static void InitializeContainer(IServiceContainer existingContainer = null)
        {
            lock (ContainerSyncObject)
            {
                if (!_containerIsConfigured)
                {
                    Container = existingContainer ?? new ServiceContainer();

                    //https://www.lightinject.net/#assembly-scanning
                    //issue https://github.com/seesharper/LightInject/issues/358 => use reflection
                    //Container.RegisterAssembly(DependencyResolutionConstants.DllPrefix + "*.dll");
                    var dllFilePaths = Directory.GetFiles(AppContext.BaseDirectory, "LightInject.MemoryLeak*.dll");

                    foreach (var dllFilePath in dllFilePaths)
                    {
                        var assembly = Assembly.LoadFrom(dllFilePath);
                        Container.RegisterAssembly(assembly);
                    }

                    //Named instances : https://www.lightinject.net/#named-services
                    //Annotations and named instances : https://www.lightinject.net/annotation/
                    if (Container is ServiceContainer)
                        ((ServiceContainer)Container).EnableAnnotatedConstructorInjection();

                    _containerIsConfigured = true;
                }
            }
        }

        private static readonly object DtoMappingSyncObject = new object();
        private static void InitializeDtoMapping()
        {
            lock (DtoMappingSyncObject)
            {
                if (Container.TryGetInstance<IMapper>() != null)
                    return;

                //fetch all AutoMapper profiles and initiate them
                var mapperConfiguration = GetMapperConfiguration();

                //register mapper configuration in IoC container - cfr. use in EntityFrameworkGenericRepository
                //this could possibly be refactored via a separate CompositionRoot file in Mapping project
                Container.RegisterSingleton<IConfigurationProvider>(factory => mapperConfiguration);
                Container.RegisterSingleton<IMapper>(factory => new Mapper(factory.GetInstance<IConfigurationProvider>(), factory.GetInstance));
            }
        }

        //Static variable so it is only created once for all containers (eg. unit tests)
        private static MapperConfiguration _mapperConfiguration;
        private static MapperConfiguration GetMapperConfiguration()
        {
            if (_mapperConfiguration == null)
            {
                _mapperConfiguration = new MapperConfiguration(cfg =>
                {
                    //add all Profile classes defined in other assemblies
                    //we only have 1 assembly that contains all Profile classes
                    cfg.AddMaps("LightInject.MemoryLeakTest");

                    //Used for resolving custom automapper typeConvertors/resolvers
                    //If any custom convertors are written, they should be registered in the container
                    //This method passes the resolve method to Automapper so it can resolve those custom convertors
                    //(actually not needed here because we don't have custom resolvers)
                    cfg.ConstructServicesUsing(Container.GetInstance);

                    //Custom code for removed functionality
                    //cfg.AddIgnoreAttributeForDestinationMember();
                });

#if DEBUG
                _mapperConfiguration.AssertConfigurationIsValid();
#endif
            }
            return _mapperConfiguration;
        }
    }
}