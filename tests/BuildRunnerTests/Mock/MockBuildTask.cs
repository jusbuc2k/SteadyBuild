using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;
using System.Threading.Tasks;

namespace BuildRunnerTests.Mock
{
    public class MockBuildTask : BuildTask
    {
        public MockBuildTask() : base("mock-no-op")
        {
        }

        public bool RunAsAsyncCalled { get; set; }

        public BuildEnvironment RunAsAsyncEnvironmentArg { get; set; }

        public override Task<BuildTaskResult> RunAsync(BuildEnvironment environment)
        {
            this.RunAsAsyncCalled = true;
            this.RunAsAsyncEnvironmentArg = environment;

            return Task.FromResult(new BuildTaskResult()
            {
                ExitCode = 0,
                Success = true
            });
        }

    }
}
