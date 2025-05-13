namespace UrlShortener.API.Models
{
    public class UrlCreateRequest
    {
        public required string OriginalUrl { get; set; }
    }

    public class UrlUpdateRequest
    {
        public required string NewUrl { get; set; }
    }
} 