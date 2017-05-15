using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class ProcessUtilsTests
    {
        [TestMethod]
        public void ProcessUtilsStartProcessReadFromStdOutAndError()
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
