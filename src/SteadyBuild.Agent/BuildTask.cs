using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SteadyBuild.Abstractions;

namespace SteadyBuild.Agent
{
    public class BuildTask
    {
        public const string PowerShellExe = "powershell.exe";
        public const string ShellExe = "cmd.exe";

        public static BuildTask Create(string expression, BuildJob project, BuildEnvironment environment)
        {
            var allVariables = new Dictionary<string, string>(project.Configuration.Variables);

           

            foreach (var variable in project.Configuration.Variables)
            {
                expression = expression.Replace(string.Concat("${", variable.Key, "}"), variable.Value);
            }

            return new BuildTask(expression);
        }

        public BuildTask(string expression)
        {
            this.Expression = expression;
        }

        public string Expression { get; set; }

        public IEnumerable<int> SuccessfulExitCodes { get; set; } = new int[] { 0 };

        public async Task<BuildTaskResult> RunAsync(BuildEnvironment environment)
        {
            var result = new BuildTaskResult();
            int exitCode;

            if (Expression.StartsWith("ps"))
            {
                exitCode = await environment.StartProcessAsync(PowerShellExe, Expression.Substring(2));
            }
            else if (Expression.StartsWith("cmd"))
            {
                exitCode = await environment.StartProcessAsync(ShellExe, string.Concat("/c ", Expression.Substring(3)));
            }
            else
            {
                //TODO: how to have platform agnostic fallback for the else condition instead of cmd.exe
                exitCode = await environment.StartProcessAsync(ShellExe, string.Concat("/c ", Expression));
            }

            result.ExitCode = exitCode;
            result.Success = this.SuccessfulExitCodes.Contains(exitCode);

            return result;
        }
    }
}
