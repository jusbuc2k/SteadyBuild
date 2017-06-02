using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public static class ProcessUtils
    {
        public static Task<int> StartProcessAsync(string fileName, string arguments = null, string workingPath = null, int timeout = 0, IDictionary<string, string> environment = null, Action<string> output = null, Action<string> errorOutput = null)
        {
            var process = new System.Diagnostics.Process();
            var exitEventWait = new System.Threading.AutoResetEvent(false);

            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;

            if (workingPath != null)
            {
                process.StartInfo.WorkingDirectory = workingPath;
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = (output == null) ? false : true;
            process.StartInfo.RedirectStandardError = (errorOutput == null) ? false: true;
            process.EnableRaisingEvents = true;

            if (environment != null)
            {
                foreach (var variable in environment)
                {
                    process.StartInfo.Environment.Add(variable.Key, variable.Value);
                }
            }

            if (output != null)
            {
                process.OutputDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    if (e.Data == null)
                    {  
                        return;
                    }

                    output(e.Data);
                };
            }

            if (errorOutput != null)
            {
                process.ErrorDataReceived += (object sender, System.Diagnostics.DataReceivedEventArgs e) =>
                {
                    if (e.Data == null)
                    {
                        return;
                    }

                    errorOutput(e.Data);
                };
            }

            process.Exited += (object sender, EventArgs e) =>
            {
                exitEventWait.Set();
            };

            return Task.Run(() =>
            {
                if (!process.Start())
                {
                    throw new Exception($"Unable to start process {fileName} {arguments}.");
                }

                if (output != null)
                {
                    process.BeginOutputReadLine();
                }

                if (errorOutput != null)
                {
                    process.BeginErrorReadLine();
                }

                if (timeout > 0)
                {
                    if (!exitEventWait.WaitOne(1000 * timeout))
                    {
                        throw new TimeoutException($"The process timed out after waiting {timeout} seconds.");
                    }
                }
                else
                {
                    exitEventWait.WaitOne();
                }

                // We have to do this so that the output/error 
                // events finish firing before we release the wait
                // on the caller
                process.WaitForExit();

                return process.ExitCode;
            });
        }
    }
}
