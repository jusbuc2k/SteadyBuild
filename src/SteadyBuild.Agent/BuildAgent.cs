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
        private readonly System.Collections.Concurrent.ConcurrentQueue<BuildJob> _localQueue;
        private System.Threading.CancellationTokenSource _cancelTokenSource;
        private System.IO.TextWriter _output;
        private readonly object _outputLock = new object();
        private readonly System.Threading.AutoResetEvent _buildReceivedFromQueueEvent = new System.Threading.AutoResetEvent(false);

        public BuildAgent(System.IO.TextWriter output, IBuildQueue queue, IProjectRepository projectRepository, BuildAgentOptions agentOptions)
        {
            _output = output;
            this.Queue = queue;
            this.Projects = projectRepository;
            this.AgentOptions = agentOptions;
            _localQueue = new System.Collections.Concurrent.ConcurrentQueue<BuildJob>();
        }

        protected void WriteOutputLine(string message)
        {
            lock (_outputLock)
            {
                _output.WriteLine(message);
            }
        }

        protected void WriteErrorLine(string message)
        {
            this.WriteOutputLine($"ERR: {message}");
        }

        protected BuildAgentOptions AgentOptions { get; private set; }

        protected IBuildQueue Queue { get; private set; }

        protected IProjectRepository Projects { get; private set; }

        public async Task StartAsync()
        {
            _cancelTokenSource = new System.Threading.CancellationTokenSource();
            var cancelToken = _cancelTokenSource.Token;

            await Task.Run(() =>
            {
                WriteOutputLine("Build agent starting to monitor queue.");

                this.MonitorQueue(cancelToken);

                WriteOutputLine("Build agent exiting.");
            }, cancelToken);

            _cancelTokenSource = null;
        }

        protected void MonitorQueue(System.Threading.CancellationToken cancellationToken)
        {
            using (var subscription = this.Queue.Subscribe(this.AgentOptions.AgentIdentifier, (queueEntry) =>
            {
                this.WriteOutputLine($"Processing a new project in the build queue (Project ID: {queueEntry.ProjectIdentifier}).");

                var projectConfig = this.Projects.GetProject(queueEntry.ProjectIdentifier).Result;
                var projectState = this.Projects.GetProjectState(queueEntry.ProjectIdentifier).Result;

                _localQueue.Enqueue(new BuildJob(queueEntry.RevisionIdentifier, projectConfig, projectState));

                // Notify the 
                _buildReceivedFromQueueEvent.Set();
            }))
            {
                int numberOfThreads = this.AgentOptions.ConcurrentBuilds;
                //TODO: Setting
                int globalTimeout = 1000 * 60 * 15;
                var workers = new List<BuildWorker>(numberOfThreads);

                for (int n = 1; n <= numberOfThreads; n++)
                {
                    workers.Add(new BuildWorker(this.AgentOptions, n));
                }

                //TODO: How to make sure the callback above and the loop below run in different threads?
                // Pretty sure the callback passed to subscribe will be run in whatever thread the build queue is running in, right?

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Block until any new build is received from the build queue
                    _buildReceivedFromQueueEvent.WaitOne();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    WriteOutputLine($"Processing items in the build queue with {numberOfThreads} workers.");

                    var workerTasks = workers.Select(a => a.ProcessQueueAsync(_localQueue));

                    if (!Task.WhenAll(workerTasks).Wait(globalTimeout))
                    {
                        WriteErrorLine("At least one build worker timed out.");
                    }

                    WriteOutputLine("All queued builds completed. Sleeping until next notification from build queue.");
                }
            }
        }

        public void Stop()
        {
            if (_cancelTokenSource != null)
            {
                // release the monitor loop wait and cancel
                _cancelTokenSource.Cancel();
                _buildReceivedFromQueueEvent.Set();                
            }
        }
    }
}
