using System;
using SteadyBuild.Abstractions;

namespace SteadyBuild.Agent
{
    class Program
    {
        static void Main(string[] args)
        {
            var controller = new BuildControllerClient();
            var options = new BuildAgentOptions()
            {
                WorkingPath = "c:\\Build\\Temp"
            };
            var agent = new BuildAgent(Console.Out, controller, controller, options);
            var task = agent.StartAsync();

            controller.Connect();

            task.Wait();
        }
    }
}