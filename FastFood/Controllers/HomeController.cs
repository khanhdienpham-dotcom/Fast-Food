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

        public async Task<IActionResult> Index()
        {
            var bestSellers = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status)
                .OrderByDescending(p => p.SoldQuantity)
                .Take(4)
                .ToListAsync();

            var combos = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status && p.Category.CategoryName.Contains("Combo"))
                .Take(3)
                .ToListAsync();

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
