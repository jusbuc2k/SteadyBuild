using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class AgentBuildTaskTests
    {
        [TestMethod]
        public void BuildTaskRunAsync()
        {
            var task = new BuildTask("ping.exe localhost -n 1");
            var writer = new Mock.MockBuildOutputWriter();
            var env = new BuildEnvironment("c:\\temp", writer);

            var result = task.RunAsync(env).Result;
            var output = writer.StringWriter.ToString();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(0, result.ExitCode);
            Assert.IsTrue(output.Contains("Ping statistics"));
        }

        [TestMethod]
        public void BuildTaskRunAsyncWithCustomExitCode()
        {
            var task = new BuildTask("ping.exe -foobar");
            var writer = new Mock.MockBuildOutputWriter();
            var env = new BuildEnvironment("c:\\temp", writer);

            task.SuccessfulExitCodes = new int[] { 0, 1 };

            var result = task.RunAsync(env).Result;
            var output = writer.StringWriter.ToString();

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.ExitCode);
        }

        [TestMethod]
        public void BuildTaskReplacesVariableTokens()
        {
            var writer = new Mock.MockBuildOutputWriter();
            string testTokenValue1 = "SomeTokenValue";
            string testTokenValue2 = "AnotherTokenValue";

            using (var te = new BuildEnvironment("C:\\Build", writer))
            {
                te.Variables.Add("Foo", testTokenValue1);
                te.Variables.Add("Bar", testTokenValue2);

                var task = new BuildTask("echo ${Foo} and ${Bar}").RunAsync(te);

                Assert.IsTrue(task.Result.Success);
                Assert.IsTrue(writer.StringWriter.ToString().Contains(testTokenValue1));
                Assert.IsTrue(writer.StringWriter.ToString().Contains(testTokenValue2));
            }
        }

        [TestMethod]
        public void BuildTaskReplacesVariableTokensCaseInsensative()
        {
            var writer = new Mock.MockBuildOutputWriter();
            string testTokenValue1 = "SomeTokenValue";
            string testTokenValue2 = "AnotherTokenValue";

            using (var te = new BuildEnvironment("C:\\Build", writer))
            {
                te.Variables.Add("fOO", testTokenValue1);
                te.Variables.Add("bAr", testTokenValue2);

                var task = new BuildTask("echo ${foo} and ${Bar}").RunAsync(te);

                Assert.IsTrue(task.Result.Success);
                Assert.IsTrue(writer.StringWriter.ToString().Contains(testTokenValue1));
                Assert.IsTrue(writer.StringWriter.ToString().Contains(testTokenValue2));
            }
        }
    }
}
