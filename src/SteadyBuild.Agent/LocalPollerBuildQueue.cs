using System;
using System.Collections.Generic;
using System.Text;
using SteadyBuild.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{
    //public class LocalPollerBuildQueue : IBuildQueue
    //{
    //    private readonly System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription> _subscriptions;
    //    private readonly IProjectRepository _projects;
    //    private readonly int _pollDuration = 1000 * 60 * 15;
    //    private System.Threading.CancellationTokenSource _cancelSource;

    //    public async Task Start()
    //    {
    //        _cancelSource = new System.Threading.CancellationTokenSource();
    //        var cancelToken = _cancelSource.Token;

    //        await this.PollForChangesAsync(cancelToken);

    //        _cancelSource = null;
    //    }

    //    protected async Task PollForChangesAsync(System.Threading.CancellationToken cancelToken)
    //    {
    //        while (!cancelToken.IsCancellationRequested)
    //        {
    //            foreach (var sub in _subscriptions)
    //            {
    //                var projectsForSub = await _projects.GetProjects(sub.AgentIdentifier);

    //                foreach (var project in projectsForSub)
    //                {
    //                    var projectState = await _projects.GetProjectState(project.ProjectIdentifier);
    //                    var code = CodeRepositoryFactory.Create(project);

    //                    var pollResult = await projectState.EvaluateBuild(project, code);

    //                    if (pollResult.NeedsBuild)
    //                    {
    //                        sub.Callback(new BuildQueueEntry()
    //                        {
    //                            EnqueueDateTime = DateTimeOffset.Now,
    //                            ProjectIdentifier = project.ProjectIdentifier,
    //                            RevisionIdentifier = pollResult.RevisionIdentifier
    //                        });
    //                    }
    //                }
    //            }

    //            System.Threading.Thread.Sleep(_pollDuration);
    //        }
    //    }

    //    public void Subscribe(string agentIdentifier, Action<BuildQueueEntry> onBuildQueued)
    //    {
    //        _subscriptions.Add(new BuildQueueSubscription()
    //        {
    //            AgentIdentifier = agentIdentifier,
    //            Callback = onBuildQueued
    //        });
    //    }
    //}
}
