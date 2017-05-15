using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BuildRunnerTests.Mock
{
    public class MockProjectRepository
    {
        public Task<BuildProjectConfiguration> GetProject(string projectIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BuildProjectConfiguration>> GetProjects(string agentIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<BuildProjectState> GetProjectState(string projectIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task SetProjectState(string projectIdentifier, BuildProjectState state)
        {
            throw new NotImplementedException();
        }
    }
}
