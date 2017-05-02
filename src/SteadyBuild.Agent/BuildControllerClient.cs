using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{
    public class BuildControllerClient : IBuildQueue, IProjectRepository
    {
        System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription> _subs = new System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription>();

        public void Connect()
        {
            Task.Run(() =>
            {
                int buildJobNumber = 0;

                while (true)
                {
                    foreach (var sub in _subs)
                    {
                        buildJobNumber++;
                        sub.Callback.Invoke(new BuildQueueEntry()
                        {
                            ProjectIdentifier = $"TestProject{buildJobNumber}",
                            EnqueueDateTime = DateTimeOffset.Now,
                            RevisionIdentifier = "1"
                        });
                    }

                    System.Threading.Thread.Sleep(100);
                }
            });
        }

        public Task<BuildProjectConfiguration> GetProject(string projectIdentifier)
        {
            return Task.FromResult(new BuildProjectConfiguration()
            {
                Name = projectIdentifier,
                ProjectIdentifier = projectIdentifier,
                RepositoryType = "svn",
                RepositoryPath = "https://github.com/jusbuc2k/knockroute.git/trunk",
                RepositorySettings = new Dictionary<string, string>()
                {
                    { "compat", "github" }
                }
            });
        }

        public Task<IEnumerable<BuildProjectConfiguration>> GetProjects(string agentIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<BuildProjectState> GetProjectState(string projectIdentifier)
        {
            return Task.FromResult(new BuildProjectState()
            {
                LastFailCommitIdentifier = "1",
                LastSuccessCommitIdentifier = "2",
                NextBuildNumber = 1
            });
        }

        public Task SetProjectState(string projectIdentifier, BuildProjectState state)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(string agentIdentifier, Action<BuildQueueEntry> onBuildQueued)
        {
            return new BuildQueueSubscription(_subs)
            {
                AgentIdentifier = agentIdentifier,
                Callback = onBuildQueued
            };
        }

    }
}
