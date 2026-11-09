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

            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == user.UserId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

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

            if (string.IsNullOrEmpty(username))
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem hóa đơn!";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Detail", "Order", new { id }) });
            }

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
