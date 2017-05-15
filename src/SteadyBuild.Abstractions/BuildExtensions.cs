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
        public static async Task<BuildResult> RunAllAsync(this IEnumerable<BuildTaskCommand> tasks, BuildEnvironment environment)
        {
            foreach (var task in tasks.OrderBy(o => o.TaskNumber))
            {
                environment.WriteMessage(MessageSeverity.Info, $"Executing task: ${task.CommandText}");

                var taskRunner = new BuildTask(task.CommandText)
                {
                    SuccessfulExitCodes = task.SuccessExitCodes
                };

                var taskResult = await taskRunner.RunAsync(environment);

                if (!taskResult.Success)
                {
                    return BuildResult.Fail(taskResult.ExitCode);
                }
            }

            return new BuildResult(0);
        }

    }
}
