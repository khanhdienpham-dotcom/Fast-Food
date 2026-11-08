using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _context;

        public OrderController(FastFoodDbContext context)
        {
            _context = context;
        }

        // Quản lý đơn hàng: Trang Index: lưu giữ danh sách đơn hàng, không có nút Thêm/Sửa/Xóa (0.125)
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // Trang Detail: hiển thị thông tin khách hàng + danh sách đơn hàng của khách hàng. CSS để hiển thị giống như Hóa đơn mua hàng (0.25)
        // Tạo Partial View cho trang Index của Order Detail để nhúng Danh sách chi tiết đơn hàng vào trang Detail của Order
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // Cập nhật trạng thái đơn hàng (Đang giao, Đã giao, Đã hủy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var validStatuses = new[] { "Đang chuẩn bị", "Đang giao", "Đã giao", "Đã hủy" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Trạng thái đơn hàng không hợp lệ!";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order != null)
            {
                if (order.Status == status)
                {
                    TempData["SuccessMessage"] = "Trạng thái đơn hàng không thay đổi.";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                // Đơn đã hủy thì không cho chuyển lại trạng thái khác (tồn kho đã được hoàn lại)
                if (order.Status == "Đã hủy")
                {
                    TempData["ErrorMessage"] = "Đơn hàng đã hủy, không thể thay đổi trạng thái!";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                // Hủy đơn -> hoàn lại số lượng tồn và trừ số lượng đã bán
                if (status == "Đã hủy")
                {
                    foreach (var d in order.OrderDetails)
                    {
                        var product = await _context.Products.FindAsync(d.ProductId);
                        if (product != null)
                        {
                            product.Quantity += d.Quantity;
                            product.SoldQuantity = Math.Max(0, product.SoldQuantity - d.Quantity);
                        }
                    }
                }
                order.Status = status;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{order.OrderCode} thành: {status}";
            }
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
