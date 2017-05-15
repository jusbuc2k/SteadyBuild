using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface ILogger
    {
        Task LogMessageAsync(MessageSeverity severity, string message);

        bool IsEnabled(MessageSeverity severity);
    }
}
