using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public interface IProjectRepository
    {
        Task<BuildProjectConfiguration> GetProject(string projectIdentifier);

        Task<BuildProjectState> GetProjectState(string projectIdentifier);

        Task SetProjectState(string projectIdentifier, BuildProjectState state);

        Task<IEnumerable<BuildProjectConfiguration>> GetProjects(string agentIdentifier);
    }
}
