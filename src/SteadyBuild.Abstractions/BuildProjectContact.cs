using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild
{
    public class BuildProjectContact
    {
        public Guid ProjectContactID { get; set; }

        public string Name { get; set; }

        public string RepositoryUsername { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public bool NotifyOnSuccess { get; set; }

        public bool NotifyOnFailure { get; set; }
    }
}
