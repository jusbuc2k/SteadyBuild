using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SteadyBuild.Abstractions;

namespace SteadyBuild.Abstractions
{

    /// <summary>
    /// Represents a single task, which is typically a shell command, performed during the process of a build.
    /// </summary>
    public class BuildTask
    {
        public const string PowerShellExe = "powershell.exe";
        public const string ShellExe = "cmd.exe";

        /// <summary>
        /// Initializes a new instance with the given shell expression.
        /// </summary>
        /// <param name="expression"></param>
        public BuildTask(string expression)
        {
            this.Expression = expression;
        }

        /// <summary>
        /// Gets or sets the expression that will be executed.
        /// </summary>
        public virtual string Expression { get; set; }

        /// <summary>
        /// Gets or sets a collection of status codes that will indicate the task was successful.
        /// </summary>
        public virtual IEnumerable<int> SuccessfulExitCodes { get; set; } = new int[] { 0 };

        /// <summary>
        /// Executes the <see cref="Expression"/> using the given environment.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns></returns>
        public virtual async Task<BuildTaskResult> RunAsync(BuildEnvironment environment)
        {
            var result = new BuildTaskResult();
            int exitCode;
            var expression = this.Expression;

            foreach (var variable in environment.Variables)
            {
                expression = System.Text.RegularExpressions.Regex.Replace(expression, "\\$\\{" + variable.Key + "\\}", variable.Value, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            if (Expression.StartsWith("ps"))
            {
                exitCode = await environment.StartProcessAsync(PowerShellExe, expression.Substring(2));
            }
            else if (Expression.StartsWith("cmd"))
            {
                exitCode = await environment.StartProcessAsync(ShellExe, string.Concat("/c ", expression.Substring(3)));
            }
            else
            {
                //TODO: how to have platform agnostic fallback for the else condition instead of cmd.exe
                exitCode = await environment.StartProcessAsync(ShellExe, string.Concat("/c ", expression));
            }

            result.ExitCode = exitCode;
            result.Success = this.SuccessfulExitCodes.Contains(exitCode);

            return result;
        }
    }
}
