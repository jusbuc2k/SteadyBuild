using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{
    public class BuildAgent
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<LocalQueuedJob> _localQueue;

        public BuildAgent(BuildAgentOptions agentOptions, IProjectRepository repository, IBuildQueueConsumer queue, ILogger logger)
        {
            this.Repository = repository;
            this.Queue = queue;
            this.Logger = logger;
            this.AgentOptions = agentOptions;
            _localQueue = new System.Collections.Concurrent.ConcurrentQueue<LocalQueuedJob>();
        }

        protected BuildAgentOptions AgentOptions { get; private set; }

        protected IBuildQueueConsumer Queue { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IProjectRepository Repository { get; private set; }

        /// <summary>
        /// Starts monitoring the build queue for build jobs and blocks until the process is ended.
        /// </summary>
        public void Run()
        {
            var cancelTokenSource = new System.Threading.CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;
            var buildReceivedEvent = new System.Threading.AutoResetEvent(false);

            this.Logger.LogInfoAsync("Build agent starting queue monitoring.").Wait();

            // Queue monitoring thread
            var monitorTask = Task.Run(async () =>
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    if (await this.WaitForJobsAsync())
                    {
                        // Notify the main thread we have new builds.
                        buildReceivedEvent.Set();
                    }
                }
            }, cancelToken);

            // Queue processing thread
            while (!cancelToken.IsCancellationRequested)
            {
                buildReceivedEvent.WaitOne();

                this.Logger.LogInfoAsync("Processing new builds in queue...").Wait();

                this.ProcessQueue();
            }
            
            this.Logger.LogInfoAsync("Build agent is stopping.").Wait();
        }

        protected async Task<bool> WaitForJobsAsync()
        {
            try
            {
                var builds = await this.Queue.WaitForJobsAsync(this.AgentOptions.AgentIdentifier, this.Logger);

                await this.Logger.LogInfoAsync($"{builds.Count()} projects were received from the build queue for processing.");

                foreach (var queueEntry in builds)
                {
                    _localQueue.Enqueue(new LocalQueuedJob()
                    {
                        QueueEntry = queueEntry,
                        RevisionIdentifier = queueEntry.RevisionIdentifier,
                        Configuration = await this.Repository.GetProject(queueEntry.ProjectID),
                        Output = new ProjectLogger(this.Repository, queueEntry.BuildQueueID, MessageSeverity.Debug)
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                await this.Logger.LogErrorAsync(ex.Message);
            }

            return false;
        }

        protected void ProcessQueue()
        {
            int numberOfThreads = this.AgentOptions.ConcurrentBuilds;
            int globalTimeout = 1000 * 60 * 15;
            var workers = new List<BuildWorker>(numberOfThreads);

            for (int n = 1; n <= numberOfThreads; n++)
            {
                workers.Add(new BuildWorker(this.AgentOptions, this.Repository, n));
            }

            //TODO: How to make sure the callback above and the loop below run in different threads?
            // Pretty sure the callback passed to subscribe will be run in whatever thread the build queue is running in, right?

            this.Logger.LogDebugAsync($"Processing new items in the build queue with {numberOfThreads} worker(s).");

            var workerTasks = workers.Select(a => a.ProcessQueueAsync(_localQueue));

            if (Task.WhenAll(workerTasks).Wait(globalTimeout))
            {
                this.Logger.LogInfoAsync("All queued builds completed. Sleeping until next notification from build queue.");
            }
            else
            {
                this.Logger.LogErrorAsync($"Timeout occcurred while waiting for all builds to complete.");
            }

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    // Block until any new build is received from the build queue
            //    _buildReceivedFromQueueEvent.WaitOne();
            //    if (cancellationToken.IsCancellationRequested)
            //    {
            //        break;
            //    }

                                
            //}
        }

    }
}
