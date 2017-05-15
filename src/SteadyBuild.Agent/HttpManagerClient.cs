using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild.Agent
{
    public class HttpManagerClient : IBuildQueueConsumer, IProjectRepository
    {
        System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription> _subs = new System.Collections.Concurrent.ConcurrentBag<BuildQueueSubscription>();
        private HttpClient _client;
        private string _endpointAddress;
        private int _pollingInterval = 1000 * 60;

        public HttpManagerClient(string endpointAddress)
        {
            _endpointAddress = endpointAddress;
            _client = new HttpClient();
            //_client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"))
        }

        public string AgentIdentifier { get; set; }

        protected string BuildRequestUrl(string path)
        {
            return string.Concat(_endpointAddress, "/", path);
        }

        public Task<BuildProjectConfiguration> GetProject(Guid projectID)
        {
            throw new NotImplementedException();
        }

        public async Task<BuildProjectState> GetProjectState(Guid projectIdentifier)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Guid>> GetProjectsByTriggerMethodAsync(BuildTriggerMethod method)
        {
            throw new NotImplementedException();
        }

        public async Task SetProjectState(Guid projectIdentifier, BuildProjectState updatedState)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(updatedState);

            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(BuildRequestUrl($"api/ProjectResult/{projectIdentifier}"), content);

            response.EnsureSuccessStatusCode();
        }

        public Task<IBuildOutputWriter> GetMessageWriterAsync(Guid projectIdentifier, Guid buildIdentifier)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<BuildQueueEntry>> WaitForJobsAsync()
        {
            //TODO: these should be passed in
            var cancelTokenSource = new System.Threading.CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            while (cancelToken.IsCancellationRequested == false)
            {
                try
                {
                    var response = await _client.GetAsync(BuildRequestUrl($"api/DequeueJobs/{this.AgentIdentifier}"));

                    response.EnsureSuccessStatusCode();

                    var queueEntries = await response.Content.ReadJsonAsAsync<IEnumerable<BuildQueueEntry>>();

                    if (queueEntries == null || queueEntries.Count() == 0)
                    {
                        continue;
                    }

                    return queueEntries;
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                System.Threading.Thread.Sleep(_pollingInterval);
            }

            return Enumerable.Empty<BuildQueueEntry>();
        }

        //public Task<BuildProjectConfiguration> GetProject(string projectIdentifier)
        //{
        //    return Task.FromResult(new BuildProjectConfiguration()
        //    {
        //        Name = projectIdentifier,
        //        ProjectIdentifier = projectIdentifier,
        //        RepositoryType = "svn",
        //        RepositoryPath = "https://github.com/jusbuc2k/knockroute.git/trunk",
        //        RepositorySettings = new Dictionary<string, string>()
        //        {
        //            { "compat", "github" }
        //        }
        //    });
        //}

        //public Task<IEnumerable<BuildProjectConfiguration>> GetProjects(string agentIdentifier)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task<BuildProjectState> GetProjectState(string projectIdentifier)
        //{
        //    return Task.FromResult(new BuildProjectState()
        //    {
        //        LastFailCommitIdentifier = "1",
        //        LastSuccessCommitIdentifier = "2",
        //        NextBuildNumber = 1
        //    });
        //}

    }
}
