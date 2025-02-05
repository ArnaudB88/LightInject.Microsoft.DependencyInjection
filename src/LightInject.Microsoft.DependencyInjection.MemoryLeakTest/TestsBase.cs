using LightInject.Microsoft.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LightInject.MemoryLeakTest
{
    [TestClass]
    public abstract class TestsBase
    {
        private readonly bool _testRequiresNewContainer = false;

        private Scope _serviceFactory;

        /// <param name="testRequiresNewContainer">Should be true when overriding the <see cref="ConfigureMocks"/> method.</param>
        protected TestsBase(bool testRequiresNewContainer = true)
        {
            _testRequiresNewContainer = testRequiresNewContainer;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            InitializeContainer();

            //This will lock the container. Only container.override() methods can be used to change registrations.
            //Uow = GetInstance<IUnitOfWork>();
            //UserContext = GetInstance<IUserContext>();

            ////wrap test with transaction
            ////await Uow.BeginTransactionAsync(); //this line fucks up the DependencyInjectionTests.DependencyInjection_PerScopeLifetime() test
            //Uow.BeginTransactionAsync().GetAwaiter().GetResult();
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            //if (Uow != null)
            //{
            //    //always rollback the transaction to keep the database clean
            //    Uow.CloseTransactionAsync(new Exception("dummy exception to simulate rollback"));

            //    Uow.Dispose();
            //}

            if (_testRequiresNewContainer)//Cleanup for the next test (which is possibly a test where _testRequiresNewContainer is false)
                CleanupContainer();
            else //clean other instances in the container
            {
                //GetInstance<ISendGridApiClientForTests>()?.SentMails.Clear();
                //GetInstance<ICacheStorage>().Clear(); //Uncomment when available
            }

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();

            //Process currentProc = Process.GetCurrentProcess();
            //var bytesInUse = currentProc.PrivateMemorySize64;

            //Trace.WriteLine("Private bytes: " + bytesInUse);
            //if (bytesInUse > 500_000_000)
            //{
            //    //For simplicity check process is not using more then 500MB of memory
            //    throw new Exception("Tests are using too much memory, is there a memory leak? Current private bytes: " + bytesInUse);
            //}
        }

        [ClassCleanup]
        public void ClassCleanup()
        {
            CleanupContainer();
        }

        private void InitializeContainer()
        {
            if (_testRequiresNewContainer)
                CleanupContainer();

            if (_serviceFactory == null)
            {
                //ConfigureDefaultCultureAndPrincipal();

                //https://www.lightinject.net/#unit-testing
                ContainerManager.Bootstrap();
                _serviceFactory = ContainerManager.Container.BeginScope();

                ConfigureContainer();
                //ConfigureNBuilder();

                //configure modified registrations in our container for current test class
                ConfigureMocks(ContainerManager.Container);
            }
        }
        private void CleanupContainer()
        {
            if (_serviceFactory != null)
            {
                _serviceFactory.Dispose();
                _serviceFactory = null;
            }
            ContainerManager.Teardown();
        }

        protected T GetInstance<T>(string instanceKey = null)
        {
            if (instanceKey != null)
            {
                return _serviceFactory.GetInstance<T>(instanceKey);
            }

            return _serviceFactory.GetInstance<T>();
        }

        /// <summary>
        /// Allows registering mocked instances before the test begins.
        /// <para>Make sure to add a constructor method with 'testRequiresNewContainer' true</para>
        /// <para>Teardown of registered mocks is not needed since a new container is created for each test. See Cleanup() for more info</para>
        /// </summary>
        protected virtual void ConfigureMocks(IServiceContainer container)
        {
        }

        private void ConfigureContainer()
        {
            ContainerManager.Container.Register<IMediator, Mediator>();

            //Needed by MediatR (Not needed in AspNetCore projects since that uses the Host.UseLightInject())
            if (!ContainerManager.Container.AvailableServices.Any(sr => sr.ServiceType == typeof(IServiceProvider)))
                ((ServiceContainer)ContainerManager.Container).CreateServiceProvider(new ServiceCollection());
        }
    }
}
