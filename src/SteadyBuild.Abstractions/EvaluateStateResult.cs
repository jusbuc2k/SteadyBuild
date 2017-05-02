using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class EvaluateStateResult
    {
        public string RevisionIdentifier { get; set; }

        public bool NeedsBuild { get; set; }
    }
}
