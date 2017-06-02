using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public class StreamLogger : SteadyBuild.Abstractions.ILogger, IDisposable
    {
        private readonly System.IO.TextWriter _writer;
        private readonly MessageSeverity _maxSeverity;

        public StreamLogger(System.IO.TextWriter writer, MessageSeverity maxSeverity)
        {
            _writer = writer;
            _maxSeverity = maxSeverity;
        }

        public StreamLogger(System.IO.Stream stream): this(new System.IO.StreamWriter(stream, Encoding.UTF8), MessageSeverity.Debug)
        {
        }

        public StreamLogger(string fileName) : this(System.IO.File.Open(fileName, System.IO.FileMode.Append, System.IO.FileAccess.Write))
        {
            
        }

        public bool IsEnabled(MessageSeverity severity)
        {
            return severity <= _maxSeverity;
        }

        public async Task LogMessageAsync(MessageSeverity severity, string message)
        {
            if (this.IsEnabled(severity))
            {
                await _writer.WriteLineAsync($"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")} {severity.ToString().ToUpper()} {message}");

                // I'm intentioanlly not awaiting this
                _writer.FlushAsync();
            }            
        }        

        public void Dispose()
        {
            _writer.Flush();

            if (_writer != null)
            {
                _writer.Dispose();
            }
        }
    }
}
