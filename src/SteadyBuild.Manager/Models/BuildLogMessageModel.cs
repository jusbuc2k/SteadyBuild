using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyBuild.Manager.Models
{
    public class BuildLogMessageModel
    {
        public int MessageNumber { get; set; }

        public MessageSeverity Severity { get; set; }

        public string Message { get; set; }
    }
}
