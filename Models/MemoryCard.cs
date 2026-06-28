namespace MathUniverse.Models
{
    public class MemoryCard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public int MatchId { get; set; }
        public string Type { get; set; } = string.Empty; // "Question" or "Answer"
    }
}

