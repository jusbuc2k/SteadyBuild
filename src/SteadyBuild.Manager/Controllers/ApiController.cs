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
            //TODO: Not this. It's stupid. What to do instead?
            _queue = repository as DbProjectRepository;
        }

        #region Queue Methods

        [Route("DequeueJobs/{agentIdentifier}")]
        public async Task<IEnumerable<BuildQueueEntry>> DequeueJobs(string agentIdentifier)
        {
            return await _queue.DequeueBuilds(agentIdentifier);
        }

        [Route("EnqueueJob/{projectId}")]
        [HttpPost]
        public Task EnqueueJob(string projectId, [FromBody]BuildQueueEntry entry)
        {
            _queue.EnqueBuild(entry);

            return Task.CompletedTask;
        }

        #endregion

        [Route("Project/{projectID}")]
        public async Task<BuildProjectConfiguration> GetProject(Guid projectID)
        {
            return await _repository.GetProject(projectID);
        }

        [Route("Projects/ByTriggerMethod/{method}")]
        public async Task<IEnumerable<Guid>> GetProjectsByTriggerMethod(string method)
        {
            if (System.Enum.TryParse(method, out BuildTriggerMethod methodEnum))
            {
                return await _repository.GetProjectsByTriggerMethodAsync(methodEnum);
            }
            else
            {
                throw new Exception("Bad request");
            }
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