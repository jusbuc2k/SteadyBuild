using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("ProjectSetting")]
    internal class DbProjectSettingRecord
    {
        [ExplicitKey]
        public Guid ProjectID { get; set; }

        [ExplicitKey]
        public string TypeName { get; set; }

        [ExplicitKey]
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
