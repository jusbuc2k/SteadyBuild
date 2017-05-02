using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class TaskEnvironmentTests
    {
        [TestMethod]
        public void TestWriteMessage()
        {
            using (var testSteam = new System.IO.StreamWriter(new System.IO.MemoryStream()))
            {
                using (var te = new BuildEnvironment("%TEMP%\\Build\\TestProject", testSteam))
                {
                    te.WriteMessage(MessageSeverity.Debug, "This is a debug message.");
                    te.WriteMessage(MessageSeverity.Info, "This is an info message.");
                    te.WriteMessage(MessageSeverity.Warn, "This is a warn message.");
                    te.WriteMessage(MessageSeverity.Error, "This is an error message.");
                }

                Assert.IsTrue(testSteam.BaseStream.CanRead);

                testSteam.BaseStream.Position = 0;
                using (var reader = new System.IO.StreamReader(testSteam.BaseStream))
                {
                    string line;
                    int prefixLength = DateTime.Now.ToString("s").Length + 1;

                    line = reader.ReadLine();
                    Assert.AreEqual("DEBUG", line.Substring(prefixLength, 5));

                    line = reader.ReadLine();
                    Assert.AreEqual("INFO", line.Substring(prefixLength, 4));

                    line = reader.ReadLine();
                    Assert.AreEqual("WARN", line.Substring(prefixLength, 4));

                    line = reader.ReadLine();
                    Assert.AreEqual("ERROR", line.Substring(prefixLength, 5));
                }
            }
        }

        [TestMethod]
        public void TestStartProcessCreatesEnvironmentVariables()
        {
            using (var testSteam = new System.IO.StreamWriter(new System.IO.MemoryStream()))
            {
                using (var te = new BuildEnvironment("C:\\Build\\Test", testSteam))
                {
                    var processResult = te.StartProcessAsync("cmd.exe", "/c set");

                    var resultCode = processResult.Result;
                    Assert.IsTrue(processResult.IsCompleted);
                    Assert.AreEqual(0, resultCode);
                }

                string CI = null;
                string CI_WORKINGPATH = null;

                testSteam.BaseStream.Position = 0;
                Assert.IsTrue(testSteam.BaseStream.Length > 0);
                using (var reader = new System.IO.StreamReader(testSteam.BaseStream))
                {
                    string line;
                    while (true)
                    {
                        line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        if (line.StartsWith("CI="))
                        {
                            CI = line.Substring(3);
                        }
                        else if (line.StartsWith("CI_WORKINGPATH="))
                        {
                            CI_WORKINGPATH = line.Substring(15);
                        }
                    }
                }

                Assert.AreEqual("BuildRunner", CI);
                Assert.AreEqual("C:\\Build\\Test", CI_WORKINGPATH);
            }
        }

        [TestMethod]
        public void TestStartProcessCapturesStandardError()
        {
            using (var testSteam = new System.IO.MemoryStream())
            {
                string testToken = "SuperDuperTokenTest";

                using (var te = new BuildEnvironment("C:\\Build\\Test", new System.IO.StreamWriter(testSteam)))
                {
                    var processResult = te.StartProcessAsync("cmd.exe", $"/c echo {testToken} 1>&2");

                    var resultCode = processResult.Result;
                    Assert.IsTrue(processResult.IsCompleted);
                    Assert.AreEqual(0, resultCode);
                }
                
                testSteam.Position = 0;
                Assert.IsTrue(testSteam.Length > 0);

                string logData;
                using (var reader = new System.IO.StreamReader(testSteam))
                {
                    logData = reader.ReadToEnd();
                }

                Assert.IsTrue(logData.Contains(testToken));
            }
        }

        [TestMethod]
        public void TestStartProcessCapturesStandardOutput()
        {
            using (var testSteam = new System.IO.MemoryStream())
            {
                string testToken = "Reply from";

                using (var te = new BuildEnvironment("C:\\Build\\Test", new System.IO.StreamWriter(testSteam)))
                {
                    var processResult = te.StartProcessAsync("cmd.exe", $"/c ping localhost -n 2");

                    var resultCode = processResult.Result;
                    Assert.IsTrue(processResult.IsCompleted);
                    Assert.AreEqual(0, resultCode);
                }

                testSteam.Position = 0;
                Assert.IsTrue(testSteam.Length > 0);

                string logData;
                using (var reader = new System.IO.StreamReader(testSteam))
                {
                    logData = reader.ReadToEnd();
                }

                Assert.IsTrue(logData.Contains(testToken));
            }
        }
    }
}
