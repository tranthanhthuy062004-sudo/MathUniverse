using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MathUniverse.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Position { get; set; } // Vị trí: Giáo viên, Nhóm trưởng

        public DateTime JoinDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}

