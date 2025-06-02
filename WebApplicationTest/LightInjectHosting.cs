//Copied from the LightInject hosting repository
namespace Microsoft.Extensions.Hosting
{
    using LightInject;
    using LightInject.Microsoft.DependencyInjection;
    using System;

    /// <summary>
    /// Extends the <see cref="IHostBuilder"/> to enable LightInject to be used as the service container.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Configures the <paramref name="builder"/> to use LightInject as the service container.
        /// </summary>
        /// <param name="builder">The target <see cref="IHostBuilder"/>.</param>
        /// <param name="configureServices">A delegate passing the <see cref="IServiceRegistry"/> used to configure services.</param>
        /// <param name="configureOptions">A delegate passing the </param>
        /// <returns>The <see cref="IHostBuilder"/> configured to use LightInject.</returns>
        public static IHostBuilder UseLightInject(this IHostBuilder builder, Action<IServiceRegistry>? configureServices = null, Action<ContainerOptions> configureOptions = null)
        {
            var options = ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings();
            configureOptions?.Invoke(options);
            var container = new ServiceContainer(options);
            container.ConstructorDependencySelector = new AnnotatedConstructorDependencySelector();
            container.ConstructorSelector = new AnnotatedConstructorSelector(container.CanGetInstance);
            configureServices?.Invoke(container);
            return builder.UseServiceProviderFactory(new LightInjectServiceProviderFactory(container));
        }

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