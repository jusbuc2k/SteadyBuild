using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{

    [Table("Agent")]
    public class DbAgentRecord
    {
        [ExplicitKey]
        public Guid AgentID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string ApiKey { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid EnvironmentID { get; set; }
    }
}
