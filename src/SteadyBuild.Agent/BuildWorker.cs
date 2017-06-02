using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SteadyBuild.Agent
{

    /// <summary>
    /// Represents a single worker thread excuted by the build agent.
    /// </summary>
    public class BuildWorker
    {
        private readonly BuildAgentOptions _options;
        private readonly int _agentInstanceID;
        private readonly IProjectRepository _repository;

        public BuildWorker(BuildAgentOptions options, IProjectRepository repository, int agentInstanceID)
        {
            _options = options;
            _agentInstanceID = agentInstanceID;
            _repository = repository;
        }

        /// <summary>
        /// Processes build jobs in the given queue one at a time until the queue is emtpy.
        /// </summary>
        /// <param name="projectQueue"></param>
        /// <returns></returns>
        public async Task ProcessQueueAsync(System.Collections.Concurrent.ConcurrentQueue<LocalQueuedJob> projectQueue)
        {
            LocalQueuedJob buildJob;

            while (projectQueue.TryDequeue(out buildJob))
            {
                try
                {
                    var environment = new BuildEnvironment(System.IO.Path.Combine(_options.WorkingPath, buildJob.Configuration.Name), buildJob.Output);
                    int failCount = buildJob.QueueEntry.FailCount;

                    environment.AddGlobalVariables();
                    environment.AddAgentVariables(_options);
                    environment.AddProjectConfigurationVariables(buildJob.Configuration);
                    environment.AddQueueEntryVariables(buildJob.QueueEntry);

                    // Create the working path if it does not exist
                    if (!System.IO.Directory.Exists(environment.WorkingPath))
                    {
                        System.IO.Directory.CreateDirectory(environment.WorkingPath);
                    }

                    if (!System.IO.Directory.Exists(environment.CodePath))
                    {
                        System.IO.Directory.CreateDirectory(environment.CodePath);
                    }
                    else
                    {
                        // Delete all the files and folders contained in the working path
                        PathUtils.CleanFolder(environment.CodePath);
                    }

                    var maxFailureCount = buildJob.Configuration.MaxFailureCount;
                    var codeRepo = CodeRepositoryFactory.Create(buildJob.Configuration);
                    var codeInfo = await codeRepo.GetInfo(buildJob.Configuration.RepositoryPath, buildJob.RevisionIdentifier);

                    environment.AddCodeInfoVariables(codeInfo);

                    // export the code into the working code folder
                    await codeRepo.Export(buildJob.Configuration.RepositoryPath, buildJob.RevisionIdentifier, environment.CodePath);

                    // run each build task in order
                    var buildResult = await buildJob.Configuration.Tasks.RunAllAsync(environment);

                    if (buildResult.Success)
                    {
                        environment.WriteMessage(MessageSeverity.Info, "The build project completed successfully.");
                    }
                    else
                    {
                        failCount++;

                        if (failCount >= maxFailureCount)
                        {
                            buildResult = BuildResult.Fail(buildResult.StatusCode, false);

                            environment.WriteMessage(MessageSeverity.Error, $"The build project failed too many times ({failCount}) and will not be attempted again.");
                        }
                        else
                        {
                            environment.WriteMessage(MessageSeverity.Warn, $"The build project has failed on try {failCount} of {maxFailureCount} and will be attempted again.");
                        }
                    }

                    await _repository.SetBuildResultAsync(buildJob.QueueEntry.BuildQueueID, buildResult);
                }
                catch (Exception ex)
                {
                    await buildJob.Output.LogErrorAsync(ex.Message);
                    await _repository.SetBuildResultAsync(buildJob.QueueEntry.BuildQueueID, BuildResult.Fail(3, true));

                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

    }
}
