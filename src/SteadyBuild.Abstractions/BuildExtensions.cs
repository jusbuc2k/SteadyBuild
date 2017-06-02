using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SteadyBuild.Abstractions;
using System.Linq;

namespace SteadyBuild
{
    public static class BuildExtensions
    {
        /// <summary>
        /// Executes each build task in the given set in the given environment.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public static async Task<BuildResult> RunAllAsync(this IEnumerable<BuildTask> tasks, BuildEnvironment environment)
        {
            foreach (var task in tasks)
            {
                environment.WriteMessage(MessageSeverity.Info, $"Executing task: {task.Expression}");

                var taskResult = await task.RunAsync(environment);

                if (!taskResult.Success)
                {
                    return BuildResult.Fail(taskResult.ExitCode);
                }
            }

            return new BuildResult(0);
        }

        public static async Task<CodeCheckResult> CheckForNeededBuild(this BuildProjectConfiguration config, ICodeRepository code, BuildQueueEntry latestBuild)
        {
            var maxFailureCount = config.MaxFailureCount;
            var force = false;
            var codeInfo = await code.GetInfo(config.RepositoryPath);
            string currentCommitIdentifier = codeInfo?.RevisionIdentifier;

            if (force || latestBuild == null)
            {
                return CodeCheckResult.Force(currentCommitIdentifier);
            }
            else if (currentCommitIdentifier.Equals(latestBuild.RevisionIdentifier))
            {
                return CodeCheckResult.Skip();
            }
            else if (latestBuild.FailCount > maxFailureCount)
            {
                return CodeCheckResult.Skip();
            }
            else
            {
                var changedFiles = await code.GetChangedFiles(config.RepositoryPath, latestBuild?.RevisionIdentifier, currentCommitIdentifier);

                if (changedFiles.Count() <= 0)
                {
                    //reason = $"The build was skipped because no files in the latest commit matched the effective change filter.";
                    // continue to the next build
                    return CodeCheckResult.Skip();
                }

                return CodeCheckResult.Changed(currentCommitIdentifier);
            }
        }
    }
}
