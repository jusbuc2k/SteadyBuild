using SteadyBuild;
using SteadyBuild.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildRunnerTests.Mock
{
    public class MockBuildQueue : IBuildQueue
    {
        public List<BuildQueueEntry> QueueItems = new List<BuildQueueEntry>();

        public Task<Guid> EnqueueBuild(BuildQueueEntry entry)
        {
            if (entry.BuildQueueID == Guid.Empty)
            {
                entry.BuildQueueID = Guid.NewGuid();
            }

            QueueItems.Add(entry);

            return Task.FromResult(entry.BuildQueueID);
        }

        public Task<IEnumerable<BuildQueueEntry>> DequeueBuilds(string agentIdentifier)
        {
            return Task.FromResult(QueueItems.AsEnumerable());
        }
    }
}
