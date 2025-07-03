namespace IngesWebAPI.Models
{
    public class Video
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public DateTime IngestionTime { get; set; } = DateTime.UtcNow;
    }
}
