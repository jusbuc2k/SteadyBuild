using System;
using SteadyBuild.Abstractions;
using Microsoft.Extensions.CommandLineUtils;
using System.IO;
using System.Text;

namespace SteadyBuild.Agent
{
    class Program
    {
        private const string DEFAULT_URL = "https://steadybuild/";

        static void Main(string[] args)
        {
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            Console.WriteLine();
            Console.WriteLine($"SteadyBuild Agent {version.Major}.{version.Minor}.{version.Revision}");
            Console.WriteLine();
            
            var app = new CommandLineApplication(throwOnUnexpectedArg: false);

            var managerPathOption = app.Option("-q | --queue-manager <url>", $"The URL to the build queue manager endpoint. Defaults to {DEFAULT_URL}", CommandOptionType.SingleValue);
            var logFileOption = app.Option("-l | --log <filename>", "File where log messages will be written.", CommandOptionType.SingleValue);
            var workingPathOption = app.Option("-p | --working-path <path>", "The path where project code will be downloaded during build processes.", CommandOptionType.SingleValue);
            var threadOption = app.Option("-t | --thread-count <count>", "The number of concurrent builds that can be performed.", CommandOptionType.SingleValue);
            var agentIdentOption = app.Option("-i | --agent-identifier <value>", "The agent identifier string used when connecting to the build queue manager.", CommandOptionType.SingleValue);
            
            app.OnExecute(() =>
            {
                int threadCount = threadOption.HasValue() ? int.Parse(threadOption.Value()) : 1;
                string agentId = agentIdentOption.HasValue() ? agentIdentOption.Value() : Environment.MachineName;
                string managerPath = managerPathOption.HasValue() ? managerPathOption.Value() : DEFAULT_URL;
                string workingPath = workingPathOption.HasValue() ? workingPathOption.Value() : "%TEMP%\\SteadyBuild";

                TextWriter logWriter;

                if (logFileOption.HasValue())
                {
                    logWriter = new System.IO.StreamWriter(System.IO.File.Open(Environment.ExpandEnvironmentVariables(logFileOption.Value()), System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write), Encoding.UTF8);
                }
                else
                {
                    logWriter = Console.Out;
                }

                using (var logger = new StreamLogger(logWriter, MessageSeverity.Debug))
                {
                    Console.WriteLine($"  Agent ID\t : {agentId}");
                    Console.WriteLine($"  Manager\t : {managerPath}");
                    Console.WriteLine($"  Working Path\t : {workingPath}");
                    Console.WriteLine($"  Threads\t : {threadCount}");
                    Console.WriteLine();

                    var client = new HttpManagerClient(managerPath);
                    
                    var options = new BuildAgentOptions()
                    {
                        AgentIdentifier = agentId,
                        ConcurrentBuilds = threadCount,
                        WorkingPath = Environment.ExpandEnvironmentVariables(workingPath)
                    };

                    var agent = new BuildAgent(options, client, client, logger);

                    Console.WriteLine("Starting agent thread.");

                    agent.Run();

                    Console.WriteLine("Agent thread exited.");
                }

                return 0;
            });

            app.Execute(args);
           
        }
    }
}