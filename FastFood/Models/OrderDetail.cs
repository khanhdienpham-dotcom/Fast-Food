using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.Models
{
    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Mã sản phẩm")]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Tên món ăn")]
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Đơn giá")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Số lượng phải từ 1")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Thành tiền")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal SubTotal { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
