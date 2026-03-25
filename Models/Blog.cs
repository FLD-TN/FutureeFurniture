using System;
using System.ComponentModel.DataAnnotations;

namespace MTKPM_FE.Models
{
    public class Blog
    {
        [Key]
        public int blog_id { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string blog_title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string blog_description { get; set; } = string.Empty;

        public string? blog_image { get; set; }

        [StringLength(250)]
        public string? slug { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}