using System;

namespace SteadyBuild.Poller
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];

            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                var repository = new SteadyBuild.Manager.DbProjectRepository(connection);
                var poller = new ProjectPoller(repository, repository);
                var cancelTokenSource = new System.Threading.CancellationTokenSource();

                poller.StartPollLoopForChangesAsync(cancelTokenSource.Token);
            }
        }
    }
}