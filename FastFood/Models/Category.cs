using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Display(Name = "Mã danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        [Range(0, 9999, ErrorMessage = "Thứ tự từ 0 đến 9999")]
        public int DisplayOrder { get; set; } = 0;

        [Display(Name = "Trạng thái hiển thị")]
        public bool Status { get; set; } = true;

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
