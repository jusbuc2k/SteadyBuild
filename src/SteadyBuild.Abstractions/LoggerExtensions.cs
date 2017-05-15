using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public static class LoggerExtensions
    {
        public async static Task LogDebugAsync(this ILogger logger, string message)
        {
            await logger.LogMessageAsync(MessageSeverity.Debug, message);
        }

        public async static Task LogInfoAsync(this ILogger logger, string message)
        {
            await logger.LogMessageAsync(MessageSeverity.Info, message);
        }

        public async static Task LogWarnAsync(this ILogger logger, string message)
        {
            await logger.LogMessageAsync(MessageSeverity.Warn, message);
        }

        public async static Task LogErrorAsync(this ILogger logger, string message)
        {
            await logger.LogMessageAsync(MessageSeverity.Error, message);
        }
    }
}
