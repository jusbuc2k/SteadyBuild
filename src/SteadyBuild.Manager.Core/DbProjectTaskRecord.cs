using Dapper.Contrib.Extensions;
using SteadyBuild.Abstractions;
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

        public DbProjectTaskRecord(Guid projectID, BuildTask task, int taskNumber)
        {
            this.ProjectID = projectID;
            this.Expression = task.Expression;
            this.SuccessExitCodes = string.Join(",", task.SuccessfulExitCodes);
        }

        [ExplicitKey]
        public Guid ProjectID { get; set; }

        [ExplicitKey]
        public int TaskNumber { get; set; }

        public string CategoryName { get; set; }

        public string Expression { get; set; }

        public string SuccessExitCodes { get; set; }
    }
}
