using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class AgentBuildEnvironmentTests
    {
        //[TestMethod]
        //public void TestWriteMessage()
        //{
        //    using (var testSteam = new System.IO.StreamWriter(new System.IO.MemoryStream()))
        //    {
        //        using (var te = new BuildEnvironment("%TEMP%\\Build\\TestProject", testSteam))
        //        {
        //            te.WriteMessage(MessageSeverity.Debug, "This is a debug message.");
        //            te.WriteMessage(MessageSeverity.Info, "This is an info message.");
        //            te.WriteMessage(MessageSeverity.Warn, "This is a warn message.");
        //            te.WriteMessage(MessageSeverity.Error, "This is an error message.");
        //        }

        //        Assert.IsTrue(testSteam.BaseStream.CanRead);

        //        testSteam.BaseStream.Position = 0;
        //        using (var reader = new System.IO.StreamReader(testSteam.BaseStream))
        //        {
        //            string line;
        //            int prefixLength = DateTime.Now.ToString("s").Length + 1;

        //            line = reader.ReadLine();
        //            Assert.AreEqual("DEBUG", line.Substring(prefixLength, 5));

        //            line = reader.ReadLine();
        //            Assert.AreEqual("INFO", line.Substring(prefixLength, 4));

        //            line = reader.ReadLine();
        //            Assert.AreEqual("WARN", line.Substring(prefixLength, 4));

        //            line = reader.ReadLine();
        //            Assert.AreEqual("ERROR", line.Substring(prefixLength, 5));
        //        }
        //    }
        //}

        [TestMethod]
        public void TestStartProcessCreatesEnvironmentVariables()
        {
            var writer = new Mock.MockBuildOutputWriter()
            {
                PrefixSeverity = false
            };

            var agentOptions = new BuildAgentOptions()
            {
                AgentIdentifier = "AgentID",
                ConcurrentBuilds = 1,
                WorkingPath = "WorkingPath"
            };

            var codeInfo = new CodeRepositoryInfo()
            {
                Author = "Author",
                RevisionIdentifier = "RevIdent"
            };

            var projectState = new BuildProjectState()
            {
                NextBuildNumber = 999
            };

            var project = new BuildProjectConfiguration()
            {
                ProjectID = Guid.NewGuid(),
                Name = "TestProject",
                RepositoryType = "svn",
                RepositoryPath = "repo-path",
                Variables = new Dictionary<string, string>()
               {
                   { "VarTest", "123" }
               }
            };
            
            using (var env = new BuildEnvironment("C:\\Build", writer))
            {
                env.AddAgentVariables(agentOptions);
                env.AddCodeInfoVariables(codeInfo);
                env.AddGlobalVariables();
                env.AddProjectStateVariables(projectState);
                env.AddProjectConfigurationVariables(project);

                var resultCode = env.StartProcessAsync("cmd.exe", "/c set").Result;

                Assert.AreEqual(0, resultCode);
            }

            var vars = new Dictionary<string, string>();

            using (var reader = new System.IO.StringReader(writer.StringWriter.ToString()))
            {
                string line;
                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    var parts = line.Split(new char[] { '=' }, 2);

                    vars.Add(parts[0], parts[1]);
                }
            }

            Assert.AreEqual("true", vars["CI"]);
            Assert.AreEqual("C:\\Build", vars["CI_WORKINGPATH"]);

            Assert.AreEqual(Environment.MachineName, vars["CI_AGENTHOST"]);
            Assert.AreEqual(agentOptions.AgentIdentifier, vars["CI_AGENTIDENTIFIER"]);

            Assert.AreEqual(codeInfo.RevisionIdentifier, vars["CI_REVISIONIDENTIFIER"]);
            Assert.AreEqual(codeInfo.Author, vars["CI_REVISIONAUTHOR"]);

            Assert.IsNotNull(vars["CI_BUILDDATE"]);
            Assert.IsNotNull(vars["CI_BUILDDATETIME"]);

            Assert.AreEqual(projectState.NextBuildNumber, int.Parse(vars["CI_BUILDNUMBER"]));

            Assert.AreEqual(project.ProjectID.ToString(), vars["CI_PROJECTID"]);
            Assert.AreEqual(project.Name, vars["CI_PROJECTNAME"]);
            Assert.AreEqual(project.RepositoryType, vars["CI_REPOSITORYTYPE"]);
            Assert.AreEqual(project.RepositoryPath, vars["CI_REPOSITORYPATH"]);
            Assert.AreEqual(project.Variables["VarTest"], vars["CI_VARTEST"]);
        }

        [TestMethod]
        public void BuildEnvironmentStartProcessCapturesStandardError()
        {
            var writer = new Mock.MockBuildOutputWriter();
            string testToken = "SuperDuperTokenTest";

            using (var te = new BuildEnvironment("C:\\Build", writer))
            {
                var processResult = te.StartProcessAsync("cmd.exe", $"/c echo {testToken} 1>&2");

                var resultCode = processResult.Result;
                Assert.IsTrue(processResult.IsCompleted);
                Assert.AreEqual(0, resultCode);
            }

            Assert.IsTrue(writer.StringWriter.ToString().Contains(testToken));
        }

        [TestMethod]
        public void BuildEnvironmentStartProcessCapturesStandardOutput()
        {
            var writer = new Mock.MockBuildOutputWriter();
            string testToken = "Reply from";

            using (var te = new BuildEnvironment("C:\\Build", writer))
            {
                var processResult = te.StartProcessAsync("cmd.exe", $"/c ping localhost -n 2");

                var resultCode = processResult.Result;
                Assert.IsTrue(processResult.IsCompleted);
                Assert.AreEqual(0, resultCode);
            }

            Assert.IsTrue(writer.StringWriter.ToString().Contains(testToken));
        }
    }
}
