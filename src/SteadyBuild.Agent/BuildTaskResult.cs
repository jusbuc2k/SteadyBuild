using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Agent
{
    public class BuildTaskResult
    {
        public int ExitCode { get; set; } = -1;

        public bool Success { get; set; } = false;
    }
    
}
