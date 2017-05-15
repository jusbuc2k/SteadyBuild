using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("BuildLog")]
    internal class DbBuildLogRecord
    {
        [ExplicitKey]
        public Guid ProjectID { get; set; }

        [ExplicitKey]
        public Guid BuildQueueID { get; set; }

        [ExplicitKey]
        public int MessageNumber { get; set; }

        public int Severity { get; set; }

        public string Message { get; set; }
    }
}
