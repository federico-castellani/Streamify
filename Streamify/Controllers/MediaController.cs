using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Streamify.Controllers
{
    [Route("api/media")]
    [ApiController]
    [Authorize]
    public class MediaController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public MediaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("stream/movie/{slug}/{filename}")]
        public IActionResult StreamMovie(string slug, string filename)
        {
            var filePath = Path.Combine(_env.WebRootPath, "Media", "Movies", slug, filename);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }

        [HttpGet("stream/series/{slug}/{filename}")]
        public IActionResult StreamSeries(string slug, string filename)
        {
            var filePath = Path.Combine(_env.WebRootPath, "Media", "Series", slug, filename);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
    }
}