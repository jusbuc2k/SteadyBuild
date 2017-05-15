using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface IProjectRepository
    {
        Task<BuildProjectConfiguration> GetProject(Guid projectIdentifier);

        Task<BuildProjectState> GetProjectState(Guid projectIdentifier);

        Task SetProjectState(Guid projectIdentifier, BuildProjectState state);

        Task<IEnumerable<Guid>> GetProjectsByTriggerMethodAsync(BuildTriggerMethod method);

        Task<IBuildOutputWriter> GetMessageWriterAsync(Guid projectIdentifier, Guid buildIdentifier);
    }
}
