using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildQueueEntry
    {
        public Guid ProjectIdentifier { get; set; }

        public string RevisionIdentifier { get; set; }

        public DateTimeOffset EnqueueDateTime { get; set; }

        public Guid BuildIdentifier { get; set; }
    }
}
