using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public class LoggerDemuxer : ILogger
    {
        private readonly IEnumerable<ILogger> _loggers;

        public LoggerDemuxer(params ILogger[] loggers)
        {
            _loggers = loggers;
        }

        public bool IsEnabled(MessageSeverity severity)
        {
            return true;
        }

        public Task LogMessageAsync(MessageSeverity severity, string message)
        {
            var tasks = new List<Task>(_loggers.Count());

            foreach (var logger in _loggers)
            {
                if (logger.IsEnabled(severity))
                {
                    tasks.Add(logger.LogMessageAsync(severity, message));
                }
            }

            return Task.WhenAll(tasks);
        }
    }
}
