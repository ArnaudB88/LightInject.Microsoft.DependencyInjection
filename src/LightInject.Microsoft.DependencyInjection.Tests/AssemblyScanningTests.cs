using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LightInject.Microsoft.DependencyInjection.Tests
{
    public class AssemblyScanningTests
    {
        [Fact]
        public void ShouldResolveFromAssemblyScanning()
        {
            //Arrange
            //Options: like an ASP.NET Core application
            var options = ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings();
            var container = new ServiceContainer(options);
            container.ConstructorDependencySelector = new AnnotatedConstructorDependencySelector();
            container.ConstructorSelector = new AnnotatedConstructorSelector(container.CanGetInstance);

            //register test class
            //container.Register<TestClass>();//registers unnamed
            container.RegisterAssembly(typeof(TestClass).Assembly);

            //Act
            var resolveNamed = container.TryGetInstance<TestClass>("LightInject.Microsoft.DependencyInjection.Tests.TestClass");
            var resolveUnnamed = container.TryGetInstance<TestClass>();

            var serviceProvider = container.CreateServiceProvider(new ServiceCollection());
            var resolveSpNamed = serviceProvider.GetKeyedService<TestClass>("LightInject.Microsoft.DependencyInjection.Tests.TestClass");
            var resolveSpUnnamed = serviceProvider.GetService<TestClass>();


            //Assert
            Assert.NotNull(resolveNamed);//LightInject.Microsoft.DependencyInjection v3.6.3 -> succeeds
            Assert.NotNull(resolveUnnamed);//LightInject.Microsoft.DependencyInjection v3.6.3 -> succeeds
            Assert.NotNull(resolveSpNamed);//LightInject.Microsoft.DependencyInjection v3.6.3 -> not supported (throws exception)
            Assert.NotNull(resolveSpUnnamed);//LightInject.Microsoft.DependencyInjection v3.6.3 -> succeeds
        }
    }

    public class TestClass
    {
    }

    /// <summary>
    /// Extends the <see cref="ContainerOptions"/> class.
    /// </summary>
    public static class ContainerOptionsExtensions
    {
        /// <summary>
        /// Sets up the <see cref="ContainerOptions"/> to be compliant with the conventions used in Microsoft.Extensions.DependencyInjection.
        /// </summary>
        /// <param name="options">The target <see cref="ContainerOptions"/>.</param>
        /// <returns><see cref="ContainerOptions"/>.</returns>
        public static ContainerOptions WithAspNetCoreSettings(this ContainerOptions options)
        {
            options.EnableVariance = false;
            return options;
        }
    }
}
