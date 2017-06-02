using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface IProjectRepository
    {
        Task<BuildProjectConfiguration> GetProject(Guid projectIdentifier);

        Task<BuildQueueEntry> GetMostRecentBuildAsync(Guid projectIdentifier);

        Task<IEnumerable<Guid>> GetProjectsToPollForChanges();

        Task WriteLogMessageAsync(Guid buildIdentifier, MessageSeverity severity, int messageNumber, string message);

        Task SetBuildResultAsync(Guid buildQueueID, BuildResult result);
    }
}
