using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildAgentOptions
    {
        public int ConcurrentBuilds { get; set; } = 1;

        public string AgentIdentifier { get; set; }

        public string WorkingPath { get; set; }

        public string ProjectLogFileName { get; set; } = "ci-build.log";
    }
}
