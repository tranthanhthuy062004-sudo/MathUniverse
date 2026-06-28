using System.ComponentModel.DataAnnotations;

namespace MathUniverse.Models
{
    public class ActivityLog
    {
        [Key]
        public int LogId { get; set; }

        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? AdditionalData { get; set; }
    }
}

