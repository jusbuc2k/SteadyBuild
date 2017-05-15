using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("ProjectContact")]
    internal class DbProjectContactRecord
    {
        public DbProjectContactRecord()
        {

        }

        public DbProjectContactRecord(Guid projectID, BuildProjectContact contact)
        {
            this.ProjectContactID = contact.ProjectContactID;
            this.ProjectID = projectID;
            this.Name = contact.Name;
            this.Email = contact.Email;
            this.Phone = contact.Phone;
            this.NotifyOnFailure = contact.NotifyOnFailure;
            this.NotifyOnSuccess = contact.NotifyOnSuccess;
            this.RepositoryUsername = contact.RepositoryUsername;
        }

        [ExplicitKey]
        public Guid ProjectContactID { get; set; }

        public Guid ProjectID { get; set; }

        public string Name { get; set; }

        public string RepositoryUsername { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool NotifyOnSuccess { get; set; }

        public bool NotifyOnFailure { get; set; }
    }
}
