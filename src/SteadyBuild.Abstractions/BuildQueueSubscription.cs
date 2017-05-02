using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildQueueSubscription : IDisposable
    {
        private readonly System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription> _collection;
        private bool isDisposed = false;

        public BuildQueueSubscription(System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription> collection)
        {
            _collection = collection;
            _collection.Add(this);
        }

        public string AgentIdentifier { get; set; }

        public Action<BuildQueueEntry> Callback { get; set; }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            var toRemove = this;

            isDisposed = _collection.TryTake(out toRemove);
        }
    }
}
