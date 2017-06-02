using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SteadyBuild.Abstractions;
using Microsoft.AspNetCore.Authorization;
using SteadyBuild.Manager.Models;

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
            //TODO: Not this. It's stupid. What to do instead?
            _queue = repository as DbProjectRepository;
        }

        #region Queue Methods

        [Route("DequeueJobs/{agentIdentifier}")]
        public async Task<IEnumerable<BuildQueueEntry>> DequeueJobs(string agentIdentifier)
        {
            return await _queue.DequeueBuilds(agentIdentifier);
        }

        [Route("EnqueueBuild/{projectId}")]
        [HttpPost]
        public async Task EnqueueBuild(string projectId, [FromBody]BuildQueueEntry entry)
        {
            await _queue.EnqueueBuild(entry);
        }

        #endregion

        [Route("Project/{projectID}")]
        public async Task<BuildProjectConfiguration> GetProject(Guid projectID)
        {
            return await _repository.GetProject(projectID);
        }

        [Route("Projects/ProjectsToPoll")]
        public async Task<IEnumerable<Guid>> GetProjectsToPoll(string method)
        {
            if (System.Enum.TryParse(method, out BuildTriggerMethod methodEnum))
            {
                return await _repository.GetProjectsToPollForChanges();
            }
            else
            {
                throw new Exception("Bad request");
            }
        }

        [HttpPost]
        [Route("BuildResult/{projectIdentifier}")]
        public async Task<ActionResult> SetBuildResult(Guid projectIdentifier, [FromBody]BuildResult result)
        {
            await _repository.SetBuildResultAsync(projectIdentifier, result);

            return this.NoContent();
        }

        [HttpPost]
        [Route("WriteBuildMessage/{buildIdentifier}")]
        public async Task<ActionResult> WriteBuildMessage(Guid buildIdentifier, [FromBody]BuildLogMessageModel model)
        {
            await _repository.WriteLogMessageAsync(buildIdentifier, model.Severity, model.MessageNumber, model.Message);

            return this.NoContent();
        }
    }
}