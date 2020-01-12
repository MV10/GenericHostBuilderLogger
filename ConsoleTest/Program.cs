using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Demonstrates various scenarios. Only run one at a time.
            // The examples with Exceptions will terminate the app.

            //await SuccessfulRun(args);
            //await BasicException(args);
            //await SuccessfulRunWithConfig(args);
            //await BasicExceptionWithConfig(args);
            await ExceptionDuringConfig(args);
        }

        private static async Task SuccessfulRun(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.AddHostBuilderLogger();
                HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
                Console.WriteLine("Press CTRL+C to exit.");
                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
                await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
            }
        }

        private static async Task BasicException(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.AddHostBuilderLogger();
                HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
                throw new Exception("Something went wrong...");
                Console.WriteLine("Press CTRL+C to exit.");
                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
                await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
            }
        }

        private static async Task SuccessfulRunWithConfig(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.ConfigureLoggingWithHostBuilderLogger(log =>
                {
                    log.AddFilter("Microsoft", LogLevel.Information);
                });
                HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
                Console.WriteLine("Press CTRL+C to exit.");
                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
                await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
            }
        }

        private static async Task BasicExceptionWithConfig(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.ConfigureLoggingWithHostBuilderLogger(log =>
                {
                    log.AddFilter("Microsoft", LogLevel.Information);
                });
                HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
                throw new Exception("Something went wrong...");
                Console.WriteLine("Press CTRL+C to exit.");
                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
                await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
            }
        }

        private static async Task ExceptionDuringConfig(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.ConfigureLoggingWithHostBuilderLogger(log =>
                {
                    HostBuilderLogger.Logger.LogInformation("Preparing to configure logging.");
                    log.AddFilter("Microsoft", LogLevel.Information);
                    throw new Exception("Logger configuration failure");
                });
                HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
                Console.WriteLine("Press CTRL+C to exit.");
                await host.RunConsoleAsync();
            }
            catch (Exception ex)
            {
                HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
                await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
            }
        }
    }
}
