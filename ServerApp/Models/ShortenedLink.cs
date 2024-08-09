namespace ServerApp.Models
{
    public class ShortenedLink
    {
        public int Id { get; set; }
        public string originalUrl { get; set; }
        public string shortenedUrl { get; set; }
        public int shortenedBy { get; set; }
        public DateTime? shortenedAt { get; set; }
    }
}
