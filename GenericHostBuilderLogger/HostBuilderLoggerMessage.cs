using System;

namespace Microsoft.Extensions.Logging
{
    public class HostBuilderLoggerMessage
    {
        // O is the ISO 8601 "round-trip" format
        // https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings#Roundtrip
        public static string TimestampFormat { get; set; } = "O";

        public LogLevel LogLevel { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }
        public object[] Args { get; private set; }

        public HostBuilderLoggerMessage(LogLevel level, string message, Exception exception, params object[] args)
        {
            LogLevel = level;
            Message = FormattedMessage(message);
            Exception = exception;
            Args = args;
        }

        public static string FormattedMessage(string message)
            => $"[{AppDomain.CurrentDomain.FriendlyName}] at UTC [{DateTimeOffset.UtcNow.ToString(TimestampFormat)}]\n{message}";
    }
}
