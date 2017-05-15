using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Contrib.Extensions;
using SteadyBuild.Abstractions;

namespace SteadyBuild.Manager
{

    [Table("Project")]
    internal class DbProjectRecord
    {
        public DbProjectRecord()
        {
        }

        public DbProjectRecord(BuildProjectConfiguration project)
        {
            this.Name = project.Name;
            this.ProjectID = project.ProjectID;
            this.RepositoryType = project.RepositoryType;
            this.RepositoryPath = project.RepositoryPath;
            this.MaxFailureCount = project.MaxFailureCount;
            this.NotifyTheAuthor = project.NotifyTheAuthor;
            this.BlameTheAuthor = project.BlameTheAuthor;
            this.MaxFailureCount = project.MaxFailureCount;
            this.IsActive = project.IsActive;
        }

        public DbProjectRecord(BuildProjectConfiguration project, BuildProjectState state) : this(project)
        {
            this.LastSuccessDateTime = state.LastSuccessDateTime;
            this.LastSuccessCommitIdentifier = state.LastSuccessCommitIdentifier;
            this.LastFailureDateTime = state.LastFailDateTime;
            this.LastFailureCommitIdentifier = state.LastFailCommitIdentifier;
            this.LastBuildResultCode = state.LastBuildResultCode;
            this.FailCount = state.FailCount;
        }

        [ExplicitKey]
        public Guid ProjectID { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public Guid EnvironmentID { get; set; }

        public byte TriggerMethod { get; set; }

        public string RepositoryType { get; set; }

        public string RepositoryPath { get; set; }

        public bool BlameTheAuthor { get; set; } = true;

        public bool NotifyTheAuthor { get; set; } = true;

        public bool PraiseTheAuthor { get; set; } = true;

        public int MaxFailureCount { get; set; } = 1;

        public bool IsActive { get; set; } = true;

        public int NextBuildNumber { get; set; } = 1;

        public int? LastBuildResultCode { get; set; }

        public DateTimeOffset? LastSuccessDateTime { get; set; }

        public string LastSuccessCommitIdentifier { get; set; }

        public DateTimeOffset? LastFailureDateTime { get; set; }

        public string LastFailureCommitIdentifier { get; set; }

        public int FailCount { get; set; } = 0;
    }
}
