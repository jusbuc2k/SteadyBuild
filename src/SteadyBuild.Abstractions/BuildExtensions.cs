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
                environment.WriteMessage(MessageSeverity.Info, $"Executing task: ${task.Expression}");

                var taskResult = await task.RunAsync(environment);

                if (!taskResult.Success)
                {
                    return BuildResult.Fail(taskResult.ExitCode);
                }
            }

            return new BuildResult(0);
        }
    }
}
