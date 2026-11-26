using FastFood.Helpers;
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
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.InStockCount = await _context.Products.CountAsync(p => p.Quantity > 0);
            ViewBag.OutOfStockCount = await _context.Products.CountAsync(p => p.Quantity == 0);
            ViewBag.TotalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            ViewBag.TotalAdmins = await _context.Users.CountAsync(u => u.Role == "Admin");
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            var allOrders = await _context.Orders.ToListAsync();
            ViewBag.TotalRevenue = allOrders.Where(OrderStatus.IsCountedInRevenue).Sum(o => o.TotalAmount);
            ViewBag.PendingCod = allOrders.Where(OrderStatus.IsPendingCod).Sum(o => o.TotalAmount);
            ViewBag.PendingCodCount = allOrders.Count(OrderStatus.IsPendingCod);
            ViewBag.CancelledCount = allOrders.Count(o => o.Status == OrderStatus.Cancelled);

            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
        }
    }
}
