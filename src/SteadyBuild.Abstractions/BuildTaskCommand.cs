using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildTaskCommand
    {
        public int TaskNumber { get; set; }

        public string CommandText { get; set; }

        public IEnumerable<int> SuccessExitCodes { get; set; } = new int[] { 0 };

        public string TypeName { get; set; }
    }
}
