using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteadyBuild.Manager
{
    [Table("ProjectAsset")]
    internal class DbProjectAssetRecord
    {

        public DbProjectAssetRecord()
        {

        }

        public DbProjectAssetRecord(Guid projectID, string assetPath)
        {
            this.ProjectID = projectID;
            this.Path = assetPath;
        }

        [Key]
        public int ProjectAssetID { get; set; }

        public Guid ProjectID { get; set; }

        public string Path { get; set; }
    }
}
