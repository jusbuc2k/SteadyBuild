using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteadyBuild;

namespace BuildRunnerTests.Mock
{
    public class MockProjectRepository : IProjectRepository
    {
        public IList<BuildProjectConfiguration> Projects { get; set; } = new List<BuildProjectConfiguration>();

        public Task WriteLogMessageAsync(Guid buildIdentifier, MessageSeverity severity, int messageNumber, string message)
        {
            System.Diagnostics.Debug.WriteLine($"{buildIdentifier}, {severity}, {messageNumber}, {message}");

            return Task.CompletedTask;
        }

        public Task<BuildProjectConfiguration> GetProject(Guid projectIdentifier)
        {
            return Task.FromResult(this.Projects.SingleOrDefault(x => x.ProjectID == projectIdentifier));
        }

        public Task<IEnumerable<Guid>> GetProjectsToPollForChanges()
        {
            return Task.FromResult(
                this.Projects
                    .Where(x => x.TriggerMethod == BuildTriggerMethod.Polling && x.IsActive)
                    .Select(s => s.ProjectID)
            );
        }

        public Task<BuildQueueEntry> GetMostRecentBuildAsync(Guid projectIdentifier)
        {
            var state = this.Projects
                .Where(x => x.ProjectID == projectIdentifier)
                .Select(s => new BuildQueueEntry()
                {
                    BuildQueueID = Guid.NewGuid(),
                    BuildNumber = 1,
                    CreateDateTime = DateTimeOffset.Now
                })
                .SingleOrDefault();

            return Task.FromResult(state);
        }

        public Task SetBuildResultAsync(Guid projectIdentifier, BuildResult result)
        {
            throw new NotImplementedException();
        }
    }
}
