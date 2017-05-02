using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public class BuildEnvironment : IDisposable
    {
        public const string ENV_PREFIX = "BUILD_";

        public BuildEnvironment(string workingPath) : this(workingPath, null)
        {            
        }

        public BuildEnvironment(string workingPath, System.IO.TextWriter output)
        {
            this.WorkingPath = Environment.ExpandEnvironmentVariables(workingPath);
            this.Variables = new Dictionary<string, string>();
            this.EnvironmentVariables = new Dictionary<string, string>();

            if (!System.IO.Directory.Exists(this.WorkingPath))
            {
                System.IO.Directory.CreateDirectory(this.WorkingPath);
            }

            if (!System.IO.Directory.Exists(this.CodePath))
            {
                System.IO.Directory.CreateDirectory(this.CodePath);
            }

            if (output == null)
            {
                this.OwnsStream = true;
                output = new System.IO.StreamWriter(System.IO.File.Open(this.LogFile, System.IO.FileMode.OpenOrCreate | System.IO.FileMode.Append));
            }
            
            this.Variables["WorkingPath"] = this.WorkingPath;
            this.Variables["CodePath"] = this.CodePath;
            this.Variables["LogFile"] = this.LogFile;

            this.Output = output;            
        }

        protected bool OwnsStream { get; private set; }

        public string WorkingPath { get; private set; }

        public string CodePath
        {
            get
            {
                return System.IO.Path.Combine(this.WorkingPath, "src");
            }
        }

        public string LogFile
        {
            get
            {
                return System.IO.Path.Combine(this.WorkingPath, "ci-build.log");
            }
        }

        public IDictionary<string, string> Variables { get; set; }

        public IDictionary<string, string> EnvironmentVariables { get; set; }

        public void AddGlobalVariables()
        {
            this.Variables.Add("BuildDate", DateTime.Now.ToString("u"));
            this.Variables.Add("BuildDateTime", DateTime.Now.ToString("s"));
        }

        public void AddAgentVariables(BuildAgentOptions agent)
        {
            this.Variables.Add("AgentHost", Environment.MachineName);
            this.Variables.Add("AgentIdentifier", agent.AgentIdentifier);
        }

        public void AddProjectConfigurationVariables(BuildProjectConfiguration configuration)
        {
            this.Variables.Add("ProjectName", configuration.Name);
            this.Variables.Add("ProjectID", configuration.ProjectIdentifier);
            this.Variables.Add("RepositoryType", configuration.RepositoryType);
            this.Variables.Add("RepositoryPath", configuration.RepositoryPath);

            if (configuration.Variables != null)
            {
                foreach (var variable in configuration.Variables)
                {
                    this.Variables.Add($"{variable.Key}", variable.Value);
                }
            }
        }

        public void AddProjectStateVariables(BuildProjectState state)
        {
            this.Variables.Add("BuildNumber", state.NextBuildNumber.ToString());
        }

        public virtual void AddCodeInfo(CodeRepositoryInfo codeInfo)
        {
            this.Variables.Add("RevisionAuthor", codeInfo.Author);
            this.Variables.Add("RevisionIdentifier", codeInfo.RevisionIdentifier);
        }

        protected void CreateEnvironmentVariables(IDictionary<string, string> env)
        {
            env.Add("CI", "true");

            foreach (var variable in this.Variables)
            {
                env.Add($"{ENV_PREFIX}{variable.Key.ToUpper()}", variable.Value);
            }
        }

        public Task<int> StartProcessAsync(string fileName, string arguments = null, int timeoutSeconds = 90)
        {
            var vars = new Dictionary<string, string>();

            this.CreateEnvironmentVariables(vars);

            if (this.EnvironmentVariables != null)
            {
                foreach (var variable in this.EnvironmentVariables)
                {
                    vars.Add(variable.Key, variable.Value);
                }
            }

            return ProcessUtils.StartProcessAsync(fileName, arguments: arguments, workingPath: this.CodePath, timeout: timeoutSeconds * 1000, environment: vars, output: (msg) =>
            {
                this.Output.WriteAsync(msg);
            }, errorOutput: (msg) =>
            {
                this.Output.WriteAsync($"ERR: {msg}");
            });
        }

        public void WriteMessage(MessageSeverity severity, string message)
        {
            this.Output.WriteLine($"{DateTime.Now:s} {severity.ToString().ToUpper()}: {message}");
        }

        public System.IO.TextWriter Output { get; private set; }

        public void Dispose()
        {
            this.Output.Flush();
            if (this.OwnsStream)
            {
                this.Output.Dispose();
            }
        }
    }
}
