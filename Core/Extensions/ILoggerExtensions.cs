using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MQContract.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "This is a simple extensions class used to wrap all logging calls to check for enablement as per recommended code")]
    internal static class ILoggerExtensions
    {
        public static void LogWarningChecked(this ILogger logger, string message, params object?[] args)
        {
            if (logger.IsEnabled(LogLevel.Warning))
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogWarning(message, args);
#pragma warning restore CA2254 // Template should be a static expression
        }

        public static void LogInformationChecked(this ILogger logger,string message, params object?[] args)
        {
            if (logger.IsEnabled(LogLevel.Information))
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogInformation(message, args);
#pragma warning restore CA2254 // Template should be a static expression
        }

        public static void LogDebugChecked(this ILogger logger, string message, params object?[] args)
        {
            if (logger.IsEnabled(LogLevel.Debug))
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogDebug(message, args);
#pragma warning restore CA2254 // Template should be a static expression
        }

        public static void LogErrorChecked(this ILogger logger, string message, params object?[] args)
        {
            if (logger.IsEnabled(LogLevel.Error))
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogError(message, args);
#pragma warning restore CA2254 // Template should be a static expression
        }

        public static void LogErrorChecked(this ILogger logger, Exception e, string message, params object?[] args)
        {
            if (logger.IsEnabled(LogLevel.Error))
#pragma warning disable CA2254 // Template should be a static expression
                logger.LogError(e, message, args);
#pragma warning restore CA2254 // Template should be a static expression
        }
    }
}
