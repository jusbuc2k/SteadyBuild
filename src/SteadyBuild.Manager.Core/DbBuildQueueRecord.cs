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

    }
}
