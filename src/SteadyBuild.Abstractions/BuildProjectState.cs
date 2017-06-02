using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Abstractions
{
    public class BuildProjectState
    {
        public int NextBuildNumber { get; set; } = 1;

        public DateTimeOffset? LastSuccessDateTime { get; set; }

        public string LastSuccessCommitIdentifier { get; set; }

        public DateTimeOffset? LastFailureDateTime { get; set; }

        public string LastFailureCommitIdentifier { get; set; }

        public int? LastBuildResultCode { get; set; }

        public int FailCount { get; set; }

        public bool ForceRebuild { get; set; }
    }
}
