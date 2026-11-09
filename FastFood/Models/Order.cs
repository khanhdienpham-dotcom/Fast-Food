using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastFood.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Display(Name = "Mã đơn hàng")]
        public int OrderId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã tra cứu")]
        public string OrderCode { get; set; } = string.Empty;

        [Display(Name = "Mã khách hàng")]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TotalAmount { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Hình thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD";

        [StringLength(50)]
        [Display(Name = "Trạng thái đơn hàng")]
        public string Status { get; set; } = "Đang chuẩn bị";

        [Display(Name = "Thời gian đặt")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
