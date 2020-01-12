using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging
{
    public class HostBuilderLoggerTerminatorService : BackgroundService
    {
        public HostBuilderLoggerTerminatorService(IHostApplicationLifetime appLife, ILoggerFactory logFactory)
        {
            var logger = logFactory.CreateLogger(nameof(HostBuilderLogger));
            appLife.ApplicationStarted.Register(() => 
            {
                logger.LogWarning(HostBuilderLoggerMessage.FormattedMessage("Abnormal application shutdown.\nEmitting messages cached during HostBuilder configuration."));
                HostBuilderLogger.Logger.EmitCachedMessages(logger);
                appLife.StopApplication();
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}
