using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Microsoft.Extensions.Logging
{
    public static class HostBuilderLoggerExtensions
    {
        public static IHostBuilder AddHostBuilderLogger(this IHostBuilder host)
        {
            if(HostBuilderLogger.Logger == null)
            {
                host.ConfigureServices(svc =>
                {
                    svc.AddHostedService<HostBuilderLoggerEmitterService>();
                });
                _ = new HostBuilderLogger();
            }
            return host;
        }

        public static IHostBuilder ConfigureLoggingWithHostBuilderLogger(this IHostBuilder host, Action<ILoggingBuilder> configureLogging)
        {
            HostBuilderLogger.PreferredLoggerConfigDelegate = configureLogging;
            host.AddHostBuilderLogger();
            host.ConfigureLogging(configureLogging);
            return host;
        }

        public static IHostBuilder ConfigureLoggingWithHostBuilderLogger(this IHostBuilder host, Action<HostBuilderContext, ILoggingBuilder> configureLogging)
        {
            HostBuilderLogger.PreferredLoggerConfigDelegateWithContext = configureLogging;
            host.AddHostBuilderLogger();
            host.ConfigureLogging(configureLogging);
            return host;
        }

        public static IHostBuilder AddTerminalHostBuilderLogger(this IHostBuilder host)
        {
            host.ConfigureServices(svc =>
            {
                svc.AddHostedService<HostBuilderLoggerTerminatorService>();
            });
            return host;
        }
    }
}
