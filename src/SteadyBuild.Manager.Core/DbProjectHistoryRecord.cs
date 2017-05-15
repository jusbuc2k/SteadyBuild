using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("ProjectHistory")]
    internal class DbProjectHistoryRecord
    {
        [ExplicitKey]
        public Guid ProjectID { get; set; }

        [ExplicitKey]
        public int BuildNumber { get; set; }

        public DateTimeOffset BuildDate { get; set; }

        public int ResultCode { get; set; }
    }
}
