using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild;
using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void RunsAllTasksExtension()
        {
            var writer = new Mock.MockBuildOutputWriter();
            var env = new BuildEnvironment("C:\\BUILD", writer);
            var tasks = new List<Mock.MockBuildTask>();

            tasks.Add(new Mock.MockBuildTask());
            tasks.Add(new Mock.MockBuildTask());
            tasks.Add(new Mock.MockBuildTask());

            var result = tasks.RunAllAsync(env).Result;

            Assert.IsTrue(result.Success);
            Assert.IsTrue(tasks.All(x => x.RunAsAsyncCalled));
            Assert.IsTrue(tasks.All(x => x.RunAsAsyncEnvironmentArg == env));
        }
    }
}
