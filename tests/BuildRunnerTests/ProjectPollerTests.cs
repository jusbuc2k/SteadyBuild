using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BuildRunnerTests
{
    [TestClass]
    public class ProjectPollerTests
    {
        [TestMethod]
        public void PollOnceForChanges()
        {
            var repo = new Mock.MockProjectRepository();
            var queue = new Mock.MockBuildQueue();
            var logger = new SteadyBuild.StreamLogger(new System.IO.MemoryStream());

            var projectID = Guid.NewGuid();

            repo.Projects.Add(new SteadyBuild.Abstractions.BuildProjectConfiguration()
            {
                ProjectID = projectID,
                IsActive = true,
                RepositoryType = "svn",
                RepositoryPath = "https://github.com/jusbuc2k/SteadyBuild.git",
                RepositorySettings = new Dictionary<string, string>(),
                NextBuildNumber = 100
            });

            var poller = new SteadyBuild.Poller.ProjectPoller(repo, queue, logger);

            Assert.AreEqual(0, queue.QueueItems.Count);

            poller.PollOnceForChangesAsync().Wait();

            Assert.AreEqual(1, queue.QueueItems.Count);
            Assert.AreEqual(projectID, queue.QueueItems[0].ProjectID);
            Assert.IsNotNull(queue.QueueItems[0].RevisionIdentifier);
            Assert.AreNotEqual(Guid.Empty, queue.QueueItems[0].BuildQueueID);
        }
    }
}
