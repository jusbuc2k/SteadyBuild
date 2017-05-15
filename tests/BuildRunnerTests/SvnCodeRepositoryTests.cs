using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class SvnCodeRepositoryTests
    {
        private const string REPO_PATH = "https://svn.csgsolutions.com/svn/sandbox";

        [TestMethod]
        public void TestSvnGetInfo()
        {
            var project = new BuildProjectConfiguration();
            var svn = new SteadyBuild.Extensions.Svn.SvnCodeRepository(project);

            project.RepositoryPath = REPO_PATH;

            var svnInfo = svn.GetInfo(REPO_PATH, "112").Result;

            Assert.IsNotNull(svnInfo);
            Assert.AreEqual("langer", svnInfo.Author);
            Assert.AreEqual("112", svnInfo.RevisionIdentifier);
        }

        [TestMethod]
        public void TestSvnGetDiff()
        {
            var project = new BuildProjectConfiguration();
            var svn = new SteadyBuild.Extensions.Svn.SvnCodeRepository(project);            

            project.RepositoryPath = REPO_PATH;

            var changedFiles = svn.GetChangedFiles(REPO_PATH, "110", "112").Result;

            Assert.IsNotNull(changedFiles);
            Assert.AreEqual(61, changedFiles.Count());
        }

        //[TestMethod]
        //public void TestParseXml()
        //{
        //    var doc = System.Xml.Linq.XDocument.Load("c:\\temp\\svn-diff.xml");

        //    var path = doc.Descendants("path");

        //    foreach (var file in files)
        //    {
        //        System.Diagnostics.Debug.WriteLine(file);
        //    }
            
        //}

        [TestMethod]
        public void TestParseSvnInfo()
        {
            var doc = System.Xml.Linq.XDocument.Load("c:\\temp\\svn-info.xml");

            var entries = doc.Descendants("entry");
            var authors = doc.Descendants("author");

        }
    }
}
