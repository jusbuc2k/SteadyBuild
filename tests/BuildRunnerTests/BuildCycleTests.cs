using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteadyBuild;
using SteadyBuild.Abstractions;
using SteadyBuild.Agent;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuildRunnerTests
{
    [TestClass]
    public class BuildAgentTests
    {
        private const string Agent_Ident = "Test-Agent1";

        private BuildProjectConfiguration _testProject = new BuildProjectConfiguration()
        {
            Name = "NetCoreTestLibrary",
            ProjectID = Guid.NewGuid(),
            RepositoryType = "git",
            RepositoryPath = "https://www.example.com/TestProject1.git",
            RepositorySettings = new Dictionary<string, string>()
            {
                { "Username", "justin" },
                { "Password", "P@ssword123" }
            },
            Contacts = new BuildProjectContact[] {
                new BuildProjectContact()
                {
                    Name = "Justin",
                    Email = "justin@example.com",
                    ProjectContactID = Guid.NewGuid(),
                    RepositoryUsername = "justin"
                }
            },
            BlameTheAuthor = true,
            NotifyTheAuthor = true,
            PraiseTheAuthor = true,
            AssetPaths = new string[]
            {
                "\\**\\bin\\$(Configuration)\\*.nupkg",
            },
            Variables = new Dictionary<string, string>()
            {
                { "DeployPath", "" },
                { "BAR", "456" }
            },
            Tasks = new BuildTask[]
            {
                new BuildTask("ping.exe localhost")
                {
                    SuccessfulExitCodes = new int[]{ 0 },
                }
            }
        };
        private Mock.MockBuildQueue _queue;
        private IProjectRepository _repository;
        private System.IO.TextWriter _output;
        private BuildAgent _agent;

        //[ClassInitialize]
        //public void SetupAgent()
        //{
        //    var agentOptions = new BuildAgentOptions();
        //    agentOptions.AgentIdentifier = Agent_Ident;
        //    agentOptions.ConcurrentBuilds = 1;
        //    agentOptions.WorkingPath = "%TEMP%\\SteadyBuild";

        //    _queue = new Mock.MockBuildQueue();
        //    _output = new System.IO.StreamWriter(new System.IO.MemoryStream());
        //    _agent = new BuildAgent(_output, _queue, _repository, agentOptions);
        //}



    }
}
