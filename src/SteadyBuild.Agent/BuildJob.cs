using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{
    public class BuildJob
    {
        public BuildJob(string revisionIdentifier, BuildProjectConfiguration config, BuildProjectState state)
        {
            this.RevisionIdentifier = revisionIdentifier;
            this.Configuration = config;
            this.State = state;
        }

        public BuildProjectConfiguration Configuration { get; set; }

        public BuildProjectState State { get; set; }

        public BuildResult LastResult { get; set; }

        public string RevisionIdentifier { get; set; }
                
        public Task<BuildResult> BuildAsync(BuildEnvironment environment)
        {
            System.Threading.Thread.Sleep(5000);

            return Task.FromResult(new BuildResult(0));

            //foreach (var task in this.Configuration.Tasks)
            //{
            //    var taskRunner = new BuildTask(task);
            //    var taskResult = await taskRunner.RunAsync(environment);

            //    if (!taskResult.Success)
            //    {
            //        return BuildResult.Fail(taskResult.ExitCode);
            //    }
            //}

            //return new BuildResult(0);
        }

    }
}
