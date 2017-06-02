using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface IBuildQueueConsumer
    {
        Task<IEnumerable<BuildQueueEntry>> WaitForJobsAsync(string agentIdentifier, ILogger logger);
    }
}
