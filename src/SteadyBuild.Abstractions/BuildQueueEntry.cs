using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildQueueEntry
    {
        public string ProjectIdentifier { get; set; }

        public DateTimeOffset EnqueueDateTime { get; set; }

        public DateTimeOffset ProcessedDateTime { get; set; }

        public string RevisionIdentifier { get; set; }
    }
}
