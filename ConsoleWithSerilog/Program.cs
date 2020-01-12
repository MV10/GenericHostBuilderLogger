using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace ConsoleWithSerilog
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Demonstrates various scenarios. Only run one at a time.
            // The examples with Exceptions will terminate the app.

            await SuccessfulRun(args);
            //await BasicException(args);
            //await SuccessfulRunWithConfig(args);
            //await BasicExceptionWithConfig(args);
            
            // Note that this specific example doesn't use Serilog since
            // the scenario is that logger configuration has failed.
            //await ExceptionDuringConfig(args);
        }

        private static async Task SuccessfulRun(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.UseSerilogWithHostBuilderLogger();
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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var host = Host.CreateDefaultBuilder(args);
                host.UseSerilogWithHostBuilderLogger();
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
                host.UseSerilogWithHostBuilderLogger((ctx, log) =>
                {
                    log
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
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
                host.UseSerilogWithHostBuilderLogger((ctx, log) =>
                {
                    log
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
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
                host.UseSerilogWithHostBuilderLogger((ctx, log) =>
                {
                    HostBuilderLogger.Logger.LogInformation("Preparing to configure Serilog.");
                    log
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .WriteTo.Console();
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
