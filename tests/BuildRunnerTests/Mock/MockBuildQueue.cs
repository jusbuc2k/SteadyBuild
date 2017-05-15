using SteadyBuild.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests.Mock
{
    public class MockBuildQueue
    {
        public ConcurrentBag<BuildQueueSubscription> Subscribers = new ConcurrentBag<BuildQueueSubscription>();

        public void Enqueue(string agentIdentifier, BuildQueueEntry entry)
        {
            foreach (var sub in this.Subscribers)
            {
                if (sub.AgentIdentifier.Equals(agentIdentifier))
                {
                    sub.Callback(entry);
                }
            }
        }

        public IDisposable Subscribe(string agentIdentifier, Action<BuildQueueEntry> onBuildQueued)
        {
            var sub = new BuildQueueSubscription(this.Subscribers)
            {
                AgentIdentifier = agentIdentifier,
                Callback = onBuildQueued
            };

            this.Subscribers.Add(sub);

            return sub;
        }
    }
}
