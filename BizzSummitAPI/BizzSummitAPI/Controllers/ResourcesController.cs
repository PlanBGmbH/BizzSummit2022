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
    public class ResourcesController : ControllerBase
    {
        private readonly ILogger<ResourcesController> _logger;

        private readonly IResourcesService _resourcesService;

        public ResourcesController(IResourcesService resourcesService, ILogger<ResourcesController> logger)
        {
            _resourcesService = resourcesService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Resource>>> GetResources()
        {
            try
            {
                var family = await _resourcesService.GetResourcesAsync("SELECT * FROM c");
                return Ok(family);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while retreiving data");
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddResource(Resource resource)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _resourcesService.AddResourceAsync(resource);
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
        public async Task<ActionResult> DeleteResource(string id)
        {
            try
            {
                await _resourcesService.DeleteResourceAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, "Some error occured while deleting data");
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateResource(Resource resource)
        {
            try
            {
                await _resourcesService.UpdateResourceAsync(resource);
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
