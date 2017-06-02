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
        private readonly ILogger _logger;

        public ProjectPoller(IProjectRepository repository, IBuildQueue queue, ILogger logger)
        {
            _repository = repository;
            _queue = queue;
            _logger = logger;
        }

        /// <summary>
        /// Gets the latest revision identifier from each project with a trigger method of Polling and pushes the project onto the build queue if there are recent changes.
        /// </summary>
        /// <returns></returns>
        public async Task PollOnceForChangesAsync()
        {
            var projectIds = await _repository.GetProjectsToPollForChanges();

            await _logger.LogInfoAsync($"Checking for changes with {projectIds.Count()} project(s).");

            foreach (var projectID in projectIds)
            {
                await _logger.LogDebugAsync($"Processing project {projectID}...");

                var projectConfigTask = _repository.GetProject(projectID);
                var latestBuildTask = _repository.GetMostRecentBuildAsync(projectID);

                await Task.WhenAll(projectConfigTask, latestBuildTask);

                var projectConfig = projectConfigTask.Result;
                var latestBuild = latestBuildTask.Result;
                var codeRepo = CodeRepositoryFactory.Create(projectConfig);

                var codeStatus = await projectConfig.CheckForNeededBuild(codeRepo, latestBuild);
                                                
                if (codeStatus.NeedsBuild == true)
                {
                    await _logger.LogInfoAsync($"Adding project {projectID} to the build queue.");

                    await _queue.EnqueueBuild(new BuildQueueEntry()
                    {
                        BuildQueueID = Guid.NewGuid(),
                        CreateDateTime = DateTimeOffset.Now,
                        ProjectID = projectID,
                        RevisionIdentifier = codeStatus.RevisionIdentifier,
                        BuildNumber = projectConfig.NextBuildNumber
                    });
                }
            }

            await _logger.LogInfoAsync("Project status check complete.");
        }

        /// <summary>
        /// Continuously invokes <see cref="PollOnceForChangesAsync"/> at the given interval.
        /// </summary>
        /// <param name="cancelToken">A cancel token that can be used to cancel the polling operation.</param>
        /// <param name="interval">The interval (in milliseconds) at which <see cref="PollOnceForChangesAsync"/> should be invoked.</param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancelToken, int interval = 60000)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    await this.PollOnceForChangesAsync();
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync(ex.Message);
                }

                await Task.Delay(interval);
            }
        }

    }
}
