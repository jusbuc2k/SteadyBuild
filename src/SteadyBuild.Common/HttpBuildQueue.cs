using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SteadyBuild.Abstractions;

namespace SteadyBuild
{
    public class HttpBuildQueue : IBuildQueue
    {
        public System.Net.Http.HttpClient _client;

        public HttpBuildQueue(string url, string apiKey)
        {
            _client = new System.Net.Http.HttpClient();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", apiKey);
            _client.BaseAddress = new Uri(url);
        }

        public async Task<Guid> EnqueBuild(BuildQueueEntry entry)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(entry);

            var message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, $"api/EnqueueJob/{entry.ProjectIdentifier}");

            message.Content = new System.Net.Http.StringContent(json);

            var result = await _client.SendAsync(message);

            result.EnsureSuccessStatusCode();

            var responseContent = await result.Content.ReadJsonAsAsync<dynamic>();

            return responseContent.BuildQueueID;
        }

        public async Task<IEnumerable<BuildQueueEntry>> DequeueBuilds(string agentIdentifier)
        {
            var result = await _client.GetAsync($"api/DequeueJobs/{agentIdentifier}");

            result.EnsureSuccessStatusCode();

            return await result.Content.ReadJsonAsAsync<IEnumerable<BuildQueueEntry>>();
        }
    }
}
