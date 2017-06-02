using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public class BuildQueueEntry
    {
        public Guid BuildQueueID { get; set; }


        public Guid ProjectID { get; set; }

        public string RevisionIdentifier { get; set; }

        public int BuildNumber { get; set; }

        public DateTimeOffset CreateDateTime { get; set; }

        public DateTimeOffset? AssignedDateTime { get; set; }

        public Guid? AssignedAgentID { get; set; }

        public DateTimeOffset? CompleteDateTime { get; set; }

        public int Status { get; set; }

        public DateTimeOffset LockExpiresDateTime { get; set; }

        public int? LastResultCode { get; set; }

        public DateTimeOffset? LastResultDateTime { get; set; }
        
        public int FailCount { get; set; }

        public DateTimeOffset? RetryAfterDateTime { get; set; }
    }
}
