// Sửa file Models/Feedback.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTKPM_FE.Models
{
    public class Feedback
    {
        [Key]
        public int feedback_id { get; set; }
        public string user_name { get; set; }
        public string user_message { get; set; }

        // Thêm các trường mới
        public int? CustomerId { get; set; }  // Foreign key
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } // Navigation property
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Rating { get; set; }
        public string Content { get; set; }
    }
}