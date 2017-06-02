using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("BuildQueue")]
    internal class DbBuildQueueRecord
    {
        [ExplicitKey]
        public Guid BuildQueueID { get; set; }

        public int BuildNumber { get; set; }

        public Guid ProjectID { get; set; }

        public DateTimeOffset CreateDateTime { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset? AssignedDateTime { get; set; }

        public Guid? AssignedAgentID { get; set; }

        public DateTimeOffset? CompleteDateTime { get; set; }

        public string RevisionIdentifier { get; set; }

        public byte Status { get; set; }

        /// <summary>
        /// Gets or sets the date/time when the lock to a particular agent expires
        /// </summary>
        public DateTimeOffset LockExpiresDateTime { get; set; }

        public int? LastResultCode { get; set; }

        public DateTimeOffset? LastResultDateTime { get; set; }

        public int FailCount { get; set; } = 0;
    }
}
