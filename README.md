## GenericHostBuilderLogger

When using the Generic Host with .NET Core 3.x, logging isn't available until the Host Builder is actually built. It is treated like any other service in that regard. This means applications have no reliable, standardized way of reporting progress and problems during the critical phase of application startup. I am of the opinion this is not acceptable. In any serious production-usage scenario, it's important to have that information available for troubleshooting and other support requirements. This package provides a basic level of startup logging support with good configurability and high reliability.

## Quick Start

After adding the `Microsoft.Extensions.Hosting` package, a minimal console-based Generic Host program looks like this:

```csharp
var host = Host.CreateDefaultBuilder(args);
await host.RunConsoleAsync();
```

You don't even need to call `ConfigureLogging` -- it's already done for you. Running this will produce a few lines of `Information`-level log messages about the Generic Host application lifecycle. The problem we're addressing is when something goes wrong between those two lines of code:

```csharp
var host = Host.CreateDefaultBuilder(args);
throw new Exception("Something went wrong...");
await host.RunConsoleAsync();
```

You can `try/catch` this, but what do you do with it? That's where this package comes into play. The simplest possible implementation looks like this:

```csharp
try
{
    var host = Host.CreateDefaultBuilder(args);
    host.AddHostBuilderLogger();
    HostBuilderLogger.Logger.LogInformation("Everything looks OK so far...");
    throw new Exception("Something went wrong...");
    await host.RunConsoleAsync();
}
catch(Exception ex)
{
    HostBuilderLogger.Logger.LogError("Program.Main caught exception", ex);
    await HostBuilderLogger.Logger.TerminalEmitCachedMessages(args);
}
```

Behind the scenes, the call to `TerminalEmitCachedMessages` actually creates a new, minimal Generic Host instance and uses that to write any cached log entries. This uses the default logger configuration, which means you'll see console output if you're using a console program, and the log entries will also be written to the Windows Application Event Log. One exception to the default configuration is that the default level is changed to `LogLevel.Trace` which ensures you'll have maximum details in the log in the event of a problem.

If the application starts normally, as soon as the Generic Host signals application-startup, the cache is also flushed using the now-live logger provided by the Generic Host.

## Logger Configuration

Most applications need more than the default logger configuration. This package is able to capture and attempt to re-use the logger configuration provided to the original Generic Host. Instead of calling `AddHostBuilderLogger`, provide your usual logger configuration lambda to `ConfigureLoggerWithHostBuilderLogger`. Notice we can even log messages from within these builder delegates:

```csharp
try
{
    var host = Host.CreateDefaultBuilder(args);
    host.ConfigureLoggingWithHostBuilderLogger(log =>
    {
        HostBuilderLogger.Logger.LogInformation("Preparing to configure logging.");
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
```

Now in the event of a failure, the separate minimal Generic Host created by the terminal-emit command will also attempt to run the specified logger configuration. In this case, "Microsoft" messages are filtered to the `Information` level. The output will still be defaulted to `LogLevel.Trace` but the filter will apply.

The package can even survive a failure within the logger configuration. In that case, it falls back to the same default behavior shown in the Quick Start section. A trivial example of how to force this behavior would be as follows:

```csharp
host.ConfigureLoggingWithHostBuilderLogger(log =>
{
    HostBuilderLogger.Logger.LogInformation("Preparing to configure logging.");
    log.AddFilter("Microsoft", LogLevel.Information);
    throw new Exception("Logger configuration failure");
});
```

## Available Log Methods

The static `HostBuilderLogger.Logger` property is available immediately after calling `AddHostBuilderLogger` or `ConfigureLoggingWithHostBuilderLogger`. The methods provided by this class are a subset of the full-blown logging system:

* `Log(logLevel, message, args)`
* `Log(logLevel, exception, message, args)`

These are wrapped by methods like `LogInformation` and `LogWarning` allowing you to skip explicit references to the `LogLevel` enumeration.

## Logger Output

This is not a full-blown implementation of the logger stack. Because the messages are cached and written to a real logger at a later time, the message is modified to include a UTC timestamp as well as the AppDomain's short name. The console output from a normal run looks like this, with some blank lines added around the entries generated by this package so they're easier to identify:

```
info: HostBuilderLogger[0]
      [ConsoleTest] at UTC [2020-01-12T18:10:57.3963573+00:00]
Application started normally.
Emitting messages cached during HostBuilder configuration.


info: HostBuilderLogger[0]
      [ConsoleTest] at UTC [2020-01-12T18:10:56.9917662+00:00]
Everything looks OK so far...


info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Source\HostBuilderLogger\ConsoleTest\bin\Debug\netcoreapp3.1
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
```

The output in response to a failure looks similar to this. In this case, we threw an exception inside the logger configuration lambda, so the "Microsoft" filter was not applied, and the output includes debug messages from the generic host. Again, we've added some blank lines to make it easier to pick out the cached entries:

```
dbug: Microsoft.Extensions.Hosting.Internal.Host[1]
      Hosting starting


warn: HostBuilderLogger[0]
      [ConsoleTest] at UTC [2020-01-12T18:13:28.7487257+00:00]
Abnormal application shutdown.
Emitting messages cached during HostBuilder configuration.


info: HostBuilderLogger[0]
      [ConsoleTest] at UTC [2020-01-12T18:13:28.2391438+00:00]
Everything looks OK so far...


fail: HostBuilderLogger[0]
      [ConsoleTest] at UTC [2020-01-12T18:13:28.5262217+00:00]
Program.Main caught exception
System.Exception: Logger configuration failure
   at ConsoleTest.Program.<>c.<Main>b__0_0(ILoggingBuilder log) in C:\Source\HostBuilderLogger\ConsoleTest\Program.cs:line 21
   at Microsoft.Extensions.Hosting.HostingHostBuilderExtensions.<>c__DisplayClass5_0.<ConfigureLogging>b__1(ILoggingBuilder builder)
   at Microsoft.Extensions.DependencyInjection.LoggingServiceCollectionExtensions.AddLogging(IServiceCollection services, Action`1 configure)
   at Microsoft.Extensions.Hosting.HostingHostBuilderExtensions.<>c__DisplayClass5_0.<ConfigureLogging>b__0(HostBuilderContext context, IServiceCollection collection)
   at Microsoft.Extensions.Hosting.HostBuilder.CreateServiceProvider()
   at Microsoft.Extensions.Hosting.HostBuilder.Build()
   at Microsoft.Extensions.Hosting.HostingHostBuilderExtensions.RunConsoleAsync(IHostBuilder hostBuilder, CancellationToken cancellationToken)
   at ConsoleTest.Program.Main(String[] args) in C:\Source\HostBuilderLogger\ConsoleTest\Program.cs:line 29


info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Source\HostBuilderLogger\ConsoleTest\bin\Debug\netcoreapp3.1
dbug: Microsoft.Extensions.Hosting.Internal.Host[2]
      Hosting started
dbug: Microsoft.Extensions.Hosting.Internal.Host[3]
      Hosting stopping
dbug: Microsoft.Extensions.Hosting.Internal.Host[4]
      Hosting stopped
```

You can customize the timestamp format. It is always logged as UTC, but you can provide any valid `DateTimeOffset` [format string](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings) to this static property. It defaults to format "o" which is the standard ISO 8601 [round-trip format](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip):

```csharp
HostBuilderLoggerMessage.TimestampFormat = "o";
```

## How It Works

The basic `AddHostBuilderLogger` call creates an instance of the `HostBuilderLogger` which internally stores log messages in a thread-safe queue collection. Then it registeres a simple Generic Host `BackgroundService` called `HostBuilderLoggerEmitterService`. When the host signals application startup via the `ApplicationStarting` token provided by `IHostApplicationLifetime`, the service triggers the logger to dump the queue to the live logger provided by the host.

In the event of a failure, calling `TerminalEmitCachedMessages` builds a separate, bare-bones Generic Host. If logger configuration was provided, first it attempts to build a host using that configuration, and if that fails, it falls back to the default configuration. This process registers a similar `BackgroundService` called `HostBuilderLoggerTerminatorService` which again triggers the logger to dump the queue in response to application startup, but after the dump is complete it calls `StopApplication`.
