namespace MathUniverse.Models
{
    public class GameContent
    {
        public int GameContentId { get; set; }
        public int LessonId { get; set; }
        public string CardQuestion { get; set; } = string.Empty; // Ví dụ: "1 dm"
        public string CardAnswer { get; set; } = string.Empty;   // Ví dụ: "10 cm"
        public int OrderIndex { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public Lesson? Lesson { get; set; }
    }
}

