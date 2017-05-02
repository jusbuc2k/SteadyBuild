using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace SteadyBuild.Agent
{
    public class BuildWorker
    {
        private readonly BuildAgentOptions _options;
        private readonly int _agentInstanceID;

        public BuildWorker(BuildAgentOptions options, int agentInstanceID)
        {
            _options = options;
            _agentInstanceID = agentInstanceID;
        }

        public async Task ProcessQueueAsync(System.Collections.Concurrent.ConcurrentQueue<BuildJob> projectQueue)
        {
            BuildJob project;

            while (projectQueue.TryDequeue(out project))
            {
                try
                {
                    var environment = new BuildEnvironment(System.IO.Path.Combine(_options.WorkingPath, project.Configuration.ProjectIdentifier));

                    environment.AddGlobalVariables();
                    environment.AddAgentVariables(_options);
                    environment.AddProjectConfigurationVariables(project.Configuration);
                    environment.AddProjectStateVariables(project.State);

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

                    var maxFailureCount = project.Configuration.MaxFailureCount;
                    var codeRepo = CodeRepositoryFactory.Create(project.Configuration);
                    var codeInfo = await codeRepo.GetInfo(project.Configuration.RepositoryPath, project.RevisionIdentifier);

                    environment.AddCodeInfo(codeInfo);

                    //var currentCommit = await codeRepo.GetInfo(project.Configuration.RepositoryPath);
                    //var force = project.State.ForceRebuild;

                    //string currentCommitIdentifier = currentCommit?.RevisionIdentifier;

                    //currentCommit.AddToEnvironment(environment);

                    //if (!force && project.State.FailCount >= maxFailureCount && currentCommitIdentifier.Equals(project.State.LastFailCommitIdentifier))
                    //{
                    //    environment.WriteMessage(MessageSeverity.Warn, $"The build was skipped because it failed {project.State.FailCount} times and nothing has changed.");
                    //    // continue to the next build
                    //    continue;
                    //}
                    //else if (!force && currentCommitIdentifier.Equals(project.State.LastSuccessCommitIdentifier))
                    //{
                    //    environment.WriteMessage(MessageSeverity.Info, $"The build was skipped because it failed {project.State.FailCount} times and nothing has changed.");
                    //    // continue to the next build
                    //    continue;
                    //}

                    //if (!force)
                    //{
                    //    var changedFiles = await codeRepo.GetChangedFiles(project.Configuration.RepositoryPath, project.State.LastSuccessCommitIdentifier, currentCommitIdentifier);

                    //    //TODO: Change Filter

                    //    if (changedFiles.Count() <= 0)
                    //    {
                    //        environment.WriteMessage(MessageSeverity.Info, $"The build was skipped because no files in the latest commit matched the effective change filter.");
                    //        // continue to the next build
                    //        continue;
                    //    }
                    //}                

                    await codeRepo.Export(project.Configuration.RepositoryPath, project.RevisionIdentifier, environment.CodePath);

                    var buildResult = await project.BuildAsync(environment);

                    if (buildResult.Success)
                    {
                        project.State.FailCount = 0;
                        project.State.LastSuccessDateTime = DateTimeOffset.UtcNow;
                        project.State.LastSuccessCommitIdentifier = project.RevisionIdentifier;
                        project.State.NextBuildNumber++;

                        environment.WriteMessage(MessageSeverity.Info, "The build project completed successfully.");
                    }
                    else
                    {
                        project.State.FailCount++;
                        project.State.LastFailDateTime = DateTimeOffset.UtcNow;
                        project.State.LastFailCommitIdentifier = project.RevisionIdentifier;

                        if (project.State.FailCount >= maxFailureCount)
                        {
                            environment.WriteMessage(MessageSeverity.Error, $"The build project failed too many times ({project.State.FailCount}) and will not be attempted again.");
                        }
                        else
                        {
                            environment.WriteMessage(MessageSeverity.Warn, $"The build project has failed on try {project.State.FailCount} of {maxFailureCount} and will be attempted again.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Log worker errors somewhere.
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }

    }
}
