using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 150 ký tự")]
        [Display(Name = "Tên món ăn")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Mô tả không vượt quá 2000 ký tự")]
        [Display(Name = "Mô tả chi tiết")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, 100000000, ErrorMessage = "Giá sản phẩm phải từ 1.000đ đến 100.000.000đ")]
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Đơn giá (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:N0} đ", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }

        [StringLength(300)]
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn")]
        [Range(0, 100000, ErrorMessage = "Số lượng tồn phải từ 0 trở lên")]
        [Display(Name = "Số lượng tồn")]
        public int Quantity { get; set; } = 0;

        [Range(0, 1000000, ErrorMessage = "Số lượng đã bán phải từ 0 trở lên")]
        [Display(Name = "Số lượng đã bán")]
        public int SoldQuantity { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Đang kinh doanh")]
        public bool Status { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key navigation
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
