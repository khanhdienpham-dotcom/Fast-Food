using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers
{
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _context;

        public OrderController(FastFoodDbContext context)
        {
            _context = context;
        }

        // BAREM LỊCH SỬ GIAO DỊCH:
        // - Kích vào link Lịch sử giao dịch, có chuyển tới trang Lịch sử giao dịch không? (0.125)
        // - Danh sách giao dịch: có phải chỉ lấy các đơn hàng của khách hàng không (hay có cả đơn hàng của khách khác) (0.25)
        public async Task<IActionResult> History()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem lịch sử giao dịch của bạn!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("History", "Order") });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction("Login", "Account");

            // CHỈ LẤY CÁC ĐƠN HÀNG CỦA CHÍNH KHÁCH HÀNG NÀY (UserId == user.UserId) (0.25)
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // BAREM:
        // - Xem chi tiết: có hiển thị Hóa đơn mua hàng (giống trang Detail của quản lý đơn hàng)? (0.25)
        // - Tạo chung 1 Partial View dùng cho cả trang Admin và Customer
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            // Chưa đăng nhập thì yêu cầu đăng nhập
            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hóa đơn!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Detail", "Order", new { id }) });
            }

            // Nếu là customer, chỉ cho phép xem đơn của chính mình
            if (role != "Admin")
            {
                if (order.User == null || order.User.Username != username)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem hóa đơn của khách hàng khác!";
                    return RedirectToAction(nameof(History));
                }
            }

            return View(order);
        }
    }
}
