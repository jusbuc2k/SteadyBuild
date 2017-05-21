using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
{
    public interface IBuildQueue
    {
        Task<Guid> EnqueBuild(BuildQueueEntry entry);

        Task<IEnumerable<BuildQueueEntry>> DequeueBuilds(string agentIdentifier);
    }
}
