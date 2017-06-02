using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SteadyBuild.Abstractions;
using Dapper;
using Dapper.Contrib.Extensions;

namespace SteadyBuild.Manager
{
    public class DbBuildOutputWriter : ILogger
    {
        private readonly System.Data.IDbConnection _connection;
        private readonly Guid _buildQueueID;
        private int messageNumber = 0;

        public DbBuildOutputWriter(System.Data.IDbConnection connection, Guid buildQueueID)
        {
            _connection = connection;
            _buildQueueID = buildQueueID;
        }

        public MessageSeverity MaxSeverity { get; set; } = MessageSeverity.Debug;

        public bool IsEnabled(MessageSeverity severity)
        {
            return severity <= this.MaxSeverity;
        }

        public async Task LogMessageAsync(MessageSeverity severity, string message)
        {
            if (this.IsEnabled(severity))
            {
                messageNumber++;

                await _connection.InsertAsync<DbBuildLogRecord>(new DbBuildLogRecord()
                {
                    BuildQueueID = _buildQueueID,
                    MessageNumber = messageNumber,
                    Severity = (byte)severity,
                    Message = message
                });
            }
        }
    }
}
