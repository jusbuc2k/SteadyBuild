using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public interface IBuildQueue
    {
        IDisposable Subscribe(string agentIdentifier, Action<BuildQueueEntry> onBuildQueued);
    }
}
