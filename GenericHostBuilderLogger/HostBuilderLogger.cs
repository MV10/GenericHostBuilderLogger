using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging
{
    public class HostBuilderLogger
    {
        public static HostBuilderLogger Logger { get; private set; }

        internal static Action<ILoggingBuilder> PreferredLoggerConfigDelegate = null;

        internal static Action<HostBuilderContext, ILoggingBuilder> PreferredLoggerConfigDelegateWithContext = null;

        private ConcurrentQueue<HostBuilderLoggerMessage> cache = new ConcurrentQueue<HostBuilderLoggerMessage>();

        public HostBuilderLogger()
        {
            Logger = this;
        }

        public void EmitCachedMessages(ILogger logger)
        {
            while(!cache.IsEmpty)
            {
                if(cache.TryDequeue(out var entry))
                {
                    logger.Log(entry.LogLevel, entry.Exception, entry.Message, entry.Args);
                }
            }
        }

        public async Task TerminalEmitCachedMessages(string[] args)
        {
            try
            {
                await EmitWithPreferredLoggerConfig(args);
            }
            catch
            {
                await EmitWithDefaultLoggerConfig(args);
            }
        }

        private async Task EmitWithPreferredLoggerConfig(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            if (PreferredLoggerConfigDelegate == null && PreferredLoggerConfigDelegateWithContext == null)
                throw new Exception();

            if (PreferredLoggerConfigDelegateWithContext != null)
                builder.ConfigureLogging(PreferredLoggerConfigDelegateWithContext);

            if (PreferredLoggerConfigDelegate != null)
                builder.ConfigureLogging(PreferredLoggerConfigDelegate);

            await builder
                .ConfigureLogging(log =>
                {
                    log.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices(svc =>
                {
                    svc.AddHostedService<HostBuilderLoggerTerminatorService>();
                })
                .RunConsoleAsync();
        }

        private async Task EmitWithDefaultLoggerConfig(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureLogging(log =>
                {
                    log.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices(svc =>
                {
                    svc.AddHostedService<HostBuilderLoggerTerminatorService>();
                })
                .RunConsoleAsync();
        }

        public void Log(LogLevel logLevel, string message, params object[] args)
            => cache.Enqueue(new HostBuilderLoggerMessage(logLevel, message, null, args));

        public void Log(LogLevel logLevel, string message, Exception exception, params object[] args)
            => cache.Enqueue(new HostBuilderLoggerMessage(logLevel, message, exception, args));

        public void LogCritical(string message, params object[] args)
            => Log(LogLevel.Critical, message, args);

        public void LogCritical(string message, Exception exception, params object[] args)
            => Log(LogLevel.Critical, message, exception, args);

        public void LogDebug(string message, params object[] args)
            => Log(LogLevel.Debug, message, args);

        public void LogDebug(string message, Exception exception, params object[] args)
            => Log(LogLevel.Debug, message, exception, args);

        public void LogError(string message, params object[] args)
            => Log(LogLevel.Error, message, args);

        public void LogError(string message, Exception exception, params object[] args)
            => Log(LogLevel.Error, message, exception, args);

        public void LogInformation(string message, params object[] args)
            => Log(LogLevel.Information, message, args);

        public void LogInformation(string message, Exception exception, params object[] args)
            => Log(LogLevel.Information, message, exception, args);

        public void LogTrace(string message, params object[] args)
            => Log(LogLevel.Trace, message, args);

        public void LogTrace(string message, Exception exception, params object[] args)
            => Log(LogLevel.Trace, message, exception, args);

        public void LogWarning(string message, params object[] args)
            => Log(LogLevel.Warning, message, args);

        public void LogWarning(string message, Exception exception, params object[] args)
            => Log(LogLevel.Warning, message, exception, args);
    }
}
