using BizzSummitAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BizzSummitAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ILogger<ProjectsController> _logger;

        private readonly IProjectsService _projectsService;

        public ProjectsController(IProjectsService projectsService, ILogger<ProjectsController> logger)
        {
            _projectsService = projectsService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            try
            {
                var family = await _projectsService.GetProjectsAsync("SELECT * FROM c");
                return Ok(family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while retreiving data");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddProject(Project project)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _projectsService.AddProjectAsync(project);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while inserting data");
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteProject(string id)
        {
            try
            {
                await _projectsService.DeleteProjectAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while deleting data");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateProject(Project project)
        {
            try
            {
                await _projectsService.UpdateProjectAsync(project);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while updating data");
            }
        }
    }
}
