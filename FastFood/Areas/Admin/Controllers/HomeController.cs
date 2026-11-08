using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class HomeController : Controller
    {
        private readonly FastFoodDbContext _context;

        public HomeController(FastFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check admin role if logged in
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                // Can still view or redirect to login
            }

            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.InStockCount = await _context.Products.CountAsync(p => p.Quantity > 0);
            ViewBag.OutOfStockCount = await _context.Products.CountAsync(p => p.Quantity == 0);
            ViewBag.TotalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            ViewBag.TotalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            // SQLite không hỗ trợ SUM trên kiểu decimal -> lấy danh sách tổng tiền rồi cộng trong C#
            var revenueList = await _context.Orders.Where(o => o.Status != "Đã hủy").Select(o => o.TotalAmount).ToListAsync();
            ViewBag.TotalRevenue = revenueList.Sum();

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
