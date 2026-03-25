using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MTKPM_FE.Models
{
    [Table("ProductImage")] // Thêm dòng này!
    public class ProductImage
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ImagePath { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
