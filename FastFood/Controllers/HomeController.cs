using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers
{
    public class HomeController : Controller
    {
        private readonly FastFoodDbContext _context;

        public HomeController(FastFoodDbContext context)
        {
            _context = context;
        }

        // BAREM:
        // - Menu đã lấy theo Category (ViewComponent) (0.125)
        // - Sản phẩm hiển thị trên trang Home có số lượng tồn > 0 (0.125)
        // - Có cho phép xem chi tiết sản phẩm (0.125)
        // - Có cho phép MUA sản phẩm không? (0.125)
        // - Câu query đã lấy dữ liệu đúng theo mục đích cho các phần trên trang Home (0.25)
        public async Task<IActionResult> Index()
        {
            // Query 1: Món ăn nổi bật / Bán chạy nhất (chỉ lấy Quantity > 0)
            var bestSellers = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status)
                .OrderByDescending(p => p.SoldQuantity)
                .Take(4)
                .ToListAsync();

            // Query 2: Combo Tiết Kiệm (chỉ lấy Quantity > 0)
            var combos = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status && p.Category.CategoryName.Contains("Combo"))
                .Take(3)
                .ToListAsync();

            // Query 3: Món mới ra mắt (chỉ lấy Quantity > 0)
            var newProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.BestSellers = bestSellers;
            ViewBag.Combos = combos;
            ViewBag.NewProducts = newProducts;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml",
                new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Trang lỗi thân thiện (404 - không tìm thấy, 400, 403...)
        public IActionResult HttpError(int code = 404)
        {
            ViewBag.Code = code;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
