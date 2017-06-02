using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class CodeCheckResult
    {
        public CodeCheckResult(string revisionIdentifier, bool needsBuild)
        {
            this.RevisionIdentifier = revisionIdentifier;
            this.NeedsBuild = needsBuild;
        }

        public string RevisionIdentifier { get; private set; }

        public bool NeedsBuild { get; private set; }

        public static CodeCheckResult Changed(string revisionIdentifier)
        {
            return new CodeCheckResult(revisionIdentifier, true);
        }

        public static CodeCheckResult Force(string revisionIdentifier)
        {
            return new CodeCheckResult(revisionIdentifier, true);
        }

        public static CodeCheckResult Skip()
        {
            return new CodeCheckResult(null, false);
        }

    }
}
