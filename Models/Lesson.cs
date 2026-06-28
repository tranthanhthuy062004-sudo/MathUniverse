﻿﻿using System.ComponentModel.DataAnnotations;

namespace MathUniverse.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Grade { get; set; } // Lớp (1-5)

        [Required]
        [StringLength(100)]
        public string Topic { get; set; } = string.Empty; // Chủ đề toán học

        [StringLength(500)]
        public string? VideoUrl { get; set; }

        public int VideoDuration { get; set; } // Thời lượng video (giây)

        [StringLength(500)]
        public string? PdfUrl { get; set; } // Đường dẫn file PDF bài giảng

        public string? TheoryContent { get; set; } // Nội dung lý thuyết (HTML)

        [StringLength(300)]
        public string? ThumbnailUrl { get; set; }

        public int OrderIndex { get; set; } // Thứ tự bài học

        public bool IsPublished { get; set; } = false;

        public bool IsDeleted { get; set; } = false; // Soft delete

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int? PreviousLessonId { get; set; } // Bài học trước đó (để unlock)

        // Navigation properties
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public ICollection<StudentProgress> StudentProgress { get; set; } = new List<StudentProgress>();
        public ICollection<GameContent> GameContents { get; set; } = new List<GameContent>();
    }
}

