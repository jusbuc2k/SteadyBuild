using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild
{
    public class LocalQueuedJob
    {
        public string RevisionIdentifier { get; set; }

        public BuildProjectConfiguration Configuration { get; set; }

        public IBuildOutputWriter Output { get; set; }
    }
}
