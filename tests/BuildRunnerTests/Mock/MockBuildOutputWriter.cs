using SteadyBuild;
using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;
using System.IO;
using System.Threading.Tasks;

namespace BuildRunnerTests.Mock
{
    public class MockBuildOutputWriter : ILogger
    {
        public MockBuildOutputWriter()
        {
            this.StringWriter = new StringWriter();
        }

        public bool PrefixSeverity { get; set; } = true;

        public StringWriter StringWriter { get; set; }

        public TextWriter TextWriter => this.StringWriter;

        public bool IsEnabled(MessageSeverity severity)
        {
            return true;
        }

        public async Task LogMessageAsync(MessageSeverity severity, string message)
        {
            if (this.PrefixSeverity)
            {
                await StringWriter.WriteLineAsync($"{severity.ToString()}: {message}");
            }
            else
            {
                await StringWriter.WriteLineAsync(message);
            }
        }
    }
}
