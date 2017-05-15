using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{

    /// <summary>
    /// Represents a single build job for a project.
    /// </summary>
    public class BuildJob
    {
        public BuildJob(string revisionIdentifier, BuildProjectConfiguration config)
        {
            this.RevisionIdentifier = revisionIdentifier;
            this.Configuration = config;
        }

        /// <summary>
        /// Gets or sets the build project configuration
        /// </summary>
        public BuildProjectConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets or sets the revision identifier that determines which revision of the code the job will be executed against.
        /// </summary>
        public string RevisionIdentifier { get; private set; }
    }
}
