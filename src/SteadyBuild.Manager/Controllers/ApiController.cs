using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SteadyBuild.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace SteadyBuild.Manager.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    [Authorize]
    public class ApiController : Controller
    {
        private readonly IProjectRepository _repository;
        private readonly IBuildQueue _queue;

        public ApiController(IProjectRepository repository)
        {
            _repository = repository;
        }

        [Route("DequeueJobs/{agentIdentifier}")]
        public async Task<IEnumerable<BuildQueueEntry>> DequeueJobs(string agentIdentifier)
        {
            return await _queue.DequeueBuilds(Guid.Parse(agentIdentifier));
        }

        [Route("EnqueueJob/{projectId}")]
        [HttpPost]
        public Task EnqueueJob(string projectId, [FromBody]BuildQueueEntry entry)
        {
            _queue.EnqueBuild(entry);

            return Task.CompletedTask;
        }

        [HttpPost]
        [Route("ProjectResult/{projectIdentifier}")]
        public async Task<ActionResult> SetProjectResult(string projectIdentifier, [FromBody]BuildProjectState state)
        {
            await _repository.SetProjectState(Guid.Parse(projectIdentifier), state);

            return this.NoContent();
        }

        [HttpPost]
        [Route("WriteBuildMessages/{projectIdentifier}/{buildNumber}")]
        public ActionResult WriteBuildMessages(string projectIdentifier, int buildNumber)
        {
            throw new NotImplementedException();
        }
    }
}