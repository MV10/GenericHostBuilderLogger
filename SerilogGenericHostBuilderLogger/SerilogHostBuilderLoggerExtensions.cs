using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Microsoft.Extensions.Logging
{
    public static class SerilogHostBuilderLoggerExtensions
    {
        public static IHostBuilder UseSerilogWithHostBuilderLogger(this IHostBuilder host)
        {
            HostBuilderLogger.LoggerConfigurationDelegate = (builder) => builder.UseSerilog();
            host.AddHostBuilderLogger();
            host.UseSerilog();
            return host;
        }

        public static IHostBuilder UseSerilogWithHostBuilderLogger(this IHostBuilder host, Action<HostBuilderContext, LoggerConfiguration> configureLogger, bool preserveStaticLogger = false, bool writeToProviders = false)
        {
            HostBuilderLogger.LoggerConfigurationDelegate = (builder) => builder.UseSerilog(configureLogger, preserveStaticLogger, writeToProviders);
            host.AddHostBuilderLogger();
            host.UseSerilog(configureLogger, preserveStaticLogger, writeToProviders);
            return host;
        }
    }
}
