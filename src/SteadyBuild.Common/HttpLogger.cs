using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public class ProjectLogger : ILogger
    {
        private readonly IProjectRepository _repo;
        private readonly MessageSeverity _maxSeverity;
        private readonly Guid _buildIdentifier;
        private int messageNumber = 0;

        public ProjectLogger(IProjectRepository repository, Guid buildIdentifier, MessageSeverity maxSeverity)
        {
            _repo = repository;
            _buildIdentifier = buildIdentifier;
            _maxSeverity = maxSeverity;
        }

        public bool IsEnabled(MessageSeverity severity)
        {
            return severity <= _maxSeverity;
        }

        public async Task LogMessageAsync(MessageSeverity severity, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (this.IsEnabled(severity))
            {
                messageNumber++;

                await _repo.WriteLogMessageAsync(_buildIdentifier, severity, messageNumber, message);
            }
        }
    }

}
