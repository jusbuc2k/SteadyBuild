using System;

namespace SteadyBuild.Poller
{
    class Program
    {
        static void Main(string[] args)
        {
            var version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version;

            Console.WriteLine();
            Console.WriteLine($"SteadyBuild Poller {version.Major}.{version.Minor}.{version.Revision}");
            Console.WriteLine();

            var connectionString = args[0];
            var logger = new StreamLogger(Console.Out, Abstractions.MessageSeverity.Debug);

            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var repository = new SteadyBuild.Manager.DbProjectRepository(connection);
                var poller = new ProjectPoller(repository, repository, logger);
                var cancelTokenSource = new System.Threading.CancellationTokenSource();

                // Poll for changes and queue the build.
                poller.StartAsync(cancelTokenSource.Token).Wait();
            }
        }
    }
}