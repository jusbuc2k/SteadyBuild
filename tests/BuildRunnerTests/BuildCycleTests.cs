using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class BuildCycleTests
    {

        public static IEnumerable<BuildProjectConfiguration> GetProjects()
        {
            yield return new BuildProjectConfiguration()
            {
                Name = "NetCoreTestLibrary",
                RepositoryType = "git",
                RepositoryPath = "https://www.example.com/TestProject1.git",
                RepositorySettings = new Dictionary<string, string>()
                {
                    { "Username", "justin" },
                    { "Password", "P@ssword123" }
                },
                Contacts = new string[] { "justin@example.com" },
                BlameTheAuthor = true,
                NotifyTheAuthor = true,
                PraiseTheAuthor = true,
                AssetPaths = new string[]
                {
                    "\\**\\bin\\$(Configuration)\\*.nupkg",
                },
                Variables = new Dictionary<string, string>()
                {
                    { "DeployPath", "" },
                    { "BAR", "456" }
                },
                Tasks = new string[]
                {
                    "build.cmd"
                }
            };
        }

        [TestMethod]
        public void TestReadFromStdOutAndError()
        {
            var output = new System.Text.StringBuilder();
            var errorOutput = new System.Text.StringBuilder();
            int lineCount = 0;
            int errorCount = 0;
           
            int exitCode1 = SteadyBuild.Abstractions.ProcessUtils.StartProcessAsync("powershell.exe", "-Command \"&{ for ($i = 1; $i -le 500; $i++) { Write-Output 'Blah blah blah this is some out text that will be written.' ; [Console]::Error.WriteLine('Blah blah blah this is some error text that will be written.') }}\"",
                output: (val) =>
                {
                    lineCount++;
                    output.AppendLine(val);
                },
                errorOutput: (val) =>
                {
                    errorCount++;
                    errorOutput.AppendLine(val);
                }
            ).Result;

            Assert.AreEqual(0, exitCode1);
            Assert.AreEqual(500, lineCount);
            Assert.AreEqual(500, errorCount);
        }
    }
}
