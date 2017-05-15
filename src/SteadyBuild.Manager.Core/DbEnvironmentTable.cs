using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("Environment")]
    public class DbEnvironmentRecord
    {
        [ExplicitKey]
        public Guid EnvironmentID { get; set; } = Guid.NewGuid();
 
        public string Name { get; set; }

        public string Platform { get; set; }

        public string Tooling { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
