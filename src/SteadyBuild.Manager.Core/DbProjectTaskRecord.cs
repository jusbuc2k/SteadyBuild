using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("ProjectTask")]
    internal class DbProjectTaskRecord
    {

        public DbProjectTaskRecord()
        {

        }

        public DbProjectTaskRecord(Guid projectID, BuildTaskCommand command)
        {
            this.ProjectID = projectID;
            this.CommandText = command.CommandText;
            this.TypeName = command.TypeName;
            this.SuccessExitCodes = string.Join(",", command.SuccessExitCodes);
        }

        [ExplicitKey]
        public Guid ProjectID { get; set; }

        [ExplicitKey]
        public int TaskNumber { get; set; }

        public string TypeName { get; set; }

        public string CommandText { get; set; }

        public string SuccessExitCodes { get; set; }
    }
}
