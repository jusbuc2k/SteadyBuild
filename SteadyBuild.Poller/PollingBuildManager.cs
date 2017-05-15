using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SteadyBuild.Poller
{
    public class ProjectPoller
    {
        private readonly IProjectRepository _repository;
        private readonly IBuildQueue _queue;

        public ProjectPoller(IProjectRepository repository, IBuildQueue queue)
        {
            _repository = repository;
            _queue = queue;
        }

        public async Task PollOnceForChangesAsync()
        {
            var projectIds = await _repository.GetProjectsByTriggerMethodAsync(BuildTriggerMethod.Polling);

            foreach (var projectID in projectIds)
            {
                var projectConfig = await _repository.GetProject(projectID);
                var state = projectConfig.LastState;
                var codeRepo = CodeRepositoryFactory.Create(projectConfig);
                var buildStatus = await state.EvaluateBuild(projectConfig, codeRepo);

                if (buildStatus.NeedsBuild)
                {
                    await _queue.EnqueBuild(new BuildQueueEntry()
                    {
                        BuildIdentifier = Guid.NewGuid(),
                        EnqueueDateTime = DateTimeOffset.Now,
                        ProjectIdentifier = projectID,
                        RevisionIdentifier = buildStatus.RevisionIdentifier
                    });
                }
            }
        }

        public Task StartPollLoopForChangesAsync(CancellationToken cancelToken, int interval = 60000)
        {
            return Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    await this.PollOnceForChangesAsync();

                    System.Threading.Thread.Sleep(interval);
                }
            });
        }

    }
}
