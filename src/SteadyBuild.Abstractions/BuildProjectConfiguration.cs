﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Abstractions
{
    public class BuildProjectConfiguration
    {
        /// <summary>
        /// Gets or sets the unique ID of the project.
        /// </summary>
        public string ProjectIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the project's display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the list of notification recipients
        /// </summary>
        public IEnumerable<string> Contacts { get; set; }

        public string RepositoryType { get; set; }

        public IDictionary<string, string> RepositorySettings { get; set; }

        public IDictionary<string, string> Variables { get; set; }

        public IEnumerable<string> AssetPaths { get; set; }

        /// <summary>
        /// Show the most recent commit author name in build FAILURE notifications
        /// </summary>
        public bool BlameTheAuthor { get; set; }

        /// <summary>
        /// Show the most recent commit author name in build SUCCESS notifications
        /// </summary>
        public bool PraiseTheAuthor { get; set; }

        /// <summary>
        /// Include the commit author on notification e-mails, regardless of the project contact settings
        /// </summary>
        public bool NotifyTheAuthor { get; set; }

        /// <summary>
        /// Gets or sets the URI to the repository
        /// </summary>
        public string RepositoryPath { get; set; }

        public int MaxFailureCount { get; set; } = 3;

        public IEnumerable<string> Tasks { get; set; }
    }
}