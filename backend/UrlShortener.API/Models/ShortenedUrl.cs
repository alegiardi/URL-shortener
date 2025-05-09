using System;

namespace UrlShortener.API.Models
{
    public class ShortenedUrl
    {
        public int Id { get; private set; }
        public required string OriginalUrl { get; set; }
        public required string Shortened { get; set; }
        public DateTime CreatedAt { get; set; }
        public required int ClickCount { get; set; } = 0;
    }
}
