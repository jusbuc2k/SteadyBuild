using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild;
using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class BuildJobTests
    {
        [TestMethod]
        public void BuildJobRunsAllTasks()
        {
            string rev = "HEAD";
            var proj = new BuildProjectConfiguration();
            var writer = new Mock.MockBuildOutputWriter();
            var env = new BuildEnvironment("C:\\BUILD", writer);
            var job = new BuildJob(rev, proj, writer);

            proj.Tasks = new List<BuildTaskCommand>();
            proj.Tasks.Add(new BuildTaskCommand()
            {
                CommandText = "echo OK",
                TypeName = "build",
                TaskNumber = 1
            });

            var result = job.BuildAsync(env).Result;
        }
    }
}
