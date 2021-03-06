﻿using SteadyBuild.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteadyBuild
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

        protected string BuildRequestUrl(string path)
        {
            return string.Concat(_endpointAddress, "/", path);
        }

        public async Task<BuildProjectConfiguration> GetProject(Guid projectID)
        {
            if (projectID == Guid.Empty)
            {
                throw new ArgumentException($"The value for the argument {nameof(projectID)} cannot be empty.");
            }

            var response = await _client.GetAsync(BuildRequestUrl($"api/Project/{projectID}"));

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadJsonAsAsync<BuildProjectConfiguration>();
        }

        public async Task SetBuildResultAsync(Guid buildQueueID, BuildResult result)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(result);

            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(BuildRequestUrl($"api/BuildResult/{buildQueueID}"), content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<Guid>> GetProjectsToPollForChanges()
        {
            var response = await _client.GetAsync($"api/ProjectsToPoll");

            return await response.Content.ReadJsonAsAsync<IEnumerable<Guid>>();
        }

        public Task<BuildQueueEntry> GetMostRecentBuildAsync(Guid projectID)
        {
            throw new NotImplementedException();
        }

        public async Task SetProjectState(Guid projectIdentifier, BuildProjectState updatedState)
        {
            if (projectIdentifier == Guid.Empty)
            {
                throw new ArgumentException($"The value for the argument {nameof(projectIdentifier)} cannot be empty.");
            }
            
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(updatedState);

            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(BuildRequestUrl($"api/ProjectResult/{projectIdentifier}"), content);

            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<BuildQueueEntry>> WaitForJobsAsync(string agentIdentifier, ILogger logger)
        {
            //TODO: these should be passed in
            var cancelTokenSource = new System.Threading.CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;

            while (cancelToken.IsCancellationRequested == false)
            {
                try
                {
                    var response = await _client.GetAsync(BuildRequestUrl($"api/DequeueJobs/{agentIdentifier}"));

                    response.EnsureSuccessStatusCode();

                    var queueEntries = await response.Content.ReadJsonAsAsync<IEnumerable<BuildQueueEntry>>();

                    if (queueEntries != null && queueEntries.Count() > 0)
                    {
                        return queueEntries;
                    }
                }
                catch (Exception ex)
                {
                    await logger.LogErrorAsync(ex.Message);
                }

                await Task.Delay(_pollingInterval);
            }

            return Enumerable.Empty<BuildQueueEntry>();
        }

        public async Task WriteLogMessageAsync(Guid buildIdentifier, MessageSeverity severity, int messageNumber, string message)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Severity = severity,
                Message = message,
                MessageNumber = messageNumber
            });

            var content = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            await _client.PostAsync(BuildRequestUrl($"api/WriteBuildMessage/{buildIdentifier}"), content);
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
