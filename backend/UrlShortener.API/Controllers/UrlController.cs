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
        // TODO: how to remove this warning without making these nullable
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UrlController> _logger;

        // GET: api/url
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShortenedUrl>>> GetAllUrls()
        {
            try
            {
                var urls = await _context.ShortenedUrls.ToListAsync();
                return Ok(urls); // TODO: what is this doing what is Ok????
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

                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error getting and redirecting URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // POST: api/url
        [HttpPost]
        public async Task<ActionResult<ShortenedUrl>> CreateShortUrl([FromBody] string originalUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(originalUrl))
                {
                    return BadRequest("Original URL is required");
                }

                // check if URL already exists to prevent duplicates
                var existingUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.OriginalUrl == originalUrl);
                if (existingUrl != null)
                {
                    return Ok(existingUrl); // return existing shortened URL
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

                var baseUrl = $"{Request.Scheme}://{Request.Host}/";
                var shortenedUrl = baseUrl + shortCode;
            
                return CreatedAtAction(nameof(GetAndRedirectUrl), new {shortCode = shortUrl.Shortened}, new { OriginalUrl = originalUrl, ShortenedUrl = shortenedUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating short URL");
                return StatusCode(500, "Internal Server Error");
            }
        }

        // PUT: api/url/{shortCode}
        [HttpPut("{shortCode}")]
        public async Task<IActionResult> UpdateUrl(string shortCode, [FromBody] string newUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(newUrl))
                {
                    return BadRequest("New URL is required");
                }

                var url = await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.Shortened == shortCode);
                if (url == null)
                {
                    return NotFound("URL not found");
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