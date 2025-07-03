using Microsoft.AspNetCore.Mvc;
using IngesWebAPI.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly GetAllVideosQuery _getQuery;
        private readonly UpdateVideoCommand _updateCommand;

        public VideosController(GetAllVideosQuery getQuery, UpdateVideoCommand updateCommand)
        {
            _getQuery = getQuery;
            _updateCommand = updateCommand;
        }

        [HttpGet]
        public async Task<ActionResult<List<Video>>> Get() => await _getQuery.ExecuteAsync();

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] Video model)
        {
            var updated = await _updateCommand.ExecuteAsync(id, model.Title, model.Summary);
            return updated ? Ok() : NotFound();
        }
    }
}
