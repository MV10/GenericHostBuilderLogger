using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Logging
{
    public class HostBuilderLoggerEmitterService : BackgroundService
    {
        public HostBuilderLoggerEmitterService(IHostApplicationLifetime appLife, ILoggerFactory logFactory)
        {
            var logger = logFactory.CreateLogger(nameof(HostBuilderLogger));
            appLife.ApplicationStarted.Register(() =>
            {
                logger.LogInformation(HostBuilderLoggerMessage.FormattedMessage("Application started normally.\nEmitting messages cached during HostBuilder configuration."));
                HostBuilderLogger.Logger.EmitCachedMessages(logger);
            });
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}
