using Microsoft.AspNetCore.Mvc;
using UrlShortener.API.Models;
using UrlShortener.API.Data;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UrlController> _logger;

        public UrlController(ApplicationDbContext context, ILogger<UrlController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/url
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShortenedUrl>>> GetAllUrls()
        {
            try
            {
                var urls = await _context.ShortenedUrls.ToListAsync();
                return Ok(urls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all URLs");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // GET: api/url/{shortCode}
        [HttpGet("{shortCode}")]
        public async Task<ActionResult> GetAndRedirectUrl(string shortCode)
        {
            try
            {
                var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Shortened == shortCode);
                if (url == null)
                {
                    return NotFound("URL not found");
                }

                url.ClickCount++;
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Incremented click count for {shortCode} to {url.ClickCount}");

                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting and redirecting URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // POST: api/url
        [HttpPost]
        public async Task<ActionResult<ShortenedUrl>> CreateShortUrl([FromBody] UrlCreateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.OriginalUrl))
                {
                    return BadRequest("Original URL is required");
                }

                string originalUrl = request.OriginalUrl;
                if (!originalUrl.StartsWith("https://") && !originalUrl.StartsWith("http://"))
                {
                    originalUrl = "https://" + originalUrl;
                }

                // Check if URL already exists to prevent duplicates
                var existingUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);
                if (existingUrl != null)
                {
                    var baseUrl = $"{Request.Scheme}://{Request.Host}/";
                    var shortenedUrl = baseUrl + existingUrl.Shortened;
                    return Ok(new { OriginalUrl = originalUrl, ShortenedUrl = shortenedUrl }); 
                }

                var shortCode = UrlController.GenerateShortCode();
                var shortUrl = new ShortenedUrl
                {
                    OriginalUrl = originalUrl,
                    Shortened = shortCode,
                    CreatedAt = DateTime.UtcNow,
                    ClickCount = 0
                };

                _context.ShortenedUrls.Add(shortUrl);
                await _context.SaveChangesAsync();

                var newBaseUrl = $"{Request.Scheme}://{Request.Host}/";
                var newShortenedUrl = newBaseUrl + shortCode;
            
                return Ok(new { OriginalUrl = originalUrl, ShortenedUrl = newShortenedUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating short URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // PUT: api/url/{shortCode}
        [HttpPut("{shortCode}")]
        public async Task<IActionResult> UpdateUrl(string shortCode, [FromBody] UrlUpdateRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.NewUrl))
                {
                    return BadRequest("New URL is required");
                }

                var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Shortened == shortCode);
                if (url == null)
                {
                    return NotFound("URL not found");
                }

                string newUrl = request.NewUrl;
                if (!newUrl.StartsWith("https://") && !newUrl.StartsWith("http://"))
                {
                    newUrl = "https://" + newUrl;
                }

                url.OriginalUrl = newUrl;
                await _context.SaveChangesAsync();

                return NoContent(); // successful update, no content to return
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // DELETE: api/url/{shortCode}
        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> DeleteUrl(string shortCode)
        {
            try
            {
                var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Shortened == shortCode);
                if (url == null)
                {
                    return NotFound("URL not found");
                }

                _context.ShortenedUrls.Remove(url);
                await _context.SaveChangesAsync();

                return NoContent(); // successful update, no content to return
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        private static string GenerateShortCode()
        {
            string chars = "abcdefghijklmopqrstuvwxyz0123456789";
            var random = new Random();

            string code = new([.. Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)])]);
            return code;
        }
    }
}