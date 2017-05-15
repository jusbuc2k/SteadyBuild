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

        public DateTimeOffset? LastFailDateTime { get; set; }

        public string LastFailCommitIdentifier { get; set; }

        public int LastBuildResultCode { get; set; }

        public int FailCount { get; set; }

        public bool ForceRebuild { get; set; }

        public async Task<EvaluateStateResult> EvaluateBuild(BuildProjectConfiguration config, ICodeRepository code)
        {
            var maxFailureCount = config.MaxFailureCount;
            var force = this.ForceRebuild;
            var codeInfo = await code.GetInfo(config.RepositoryPath);
            string currentCommitIdentifier = codeInfo?.RevisionIdentifier;

            var result = new EvaluateStateResult()
            {
                NeedsBuild = false,
                RevisionIdentifier = currentCommitIdentifier
            };

            if (!force && this.FailCount >= maxFailureCount && currentCommitIdentifier.Equals(this.LastFailCommitIdentifier))
            {
                // reason = $"The build was skipped because it failed {this.FailCount} times and nothing has changed.";
                // continue to the next build
                return result;
            }
            else if (!force && currentCommitIdentifier.Equals(this.LastSuccessCommitIdentifier))
            {
                //reason = $"The build was skipped because it failed {this.FailCount} times and nothing has changed.";
                // continue to the next build
                return result;
            }

            if (!force)
            {
                var changedFiles = await code.GetChangedFiles(config.RepositoryPath, this.LastSuccessCommitIdentifier, currentCommitIdentifier);

                //TODO: Change Filter

                if (changedFiles.Count() <= 0)
                {
                    //reason = $"The build was skipped because no files in the latest commit matched the effective change filter.";
                    // continue to the next build
                    return result;
                }
            }

            result.NeedsBuild = true;

            return result;
        }
    }
}
