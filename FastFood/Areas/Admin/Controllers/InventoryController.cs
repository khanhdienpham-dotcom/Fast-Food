using FastFood.Models;
using FastFood.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class InventoryController : Controller
    {
        private readonly FastFoodDbContext _context;

        public InventoryController(FastFoodDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Inventory
        // Thống kê hàng theo 2 phần: Danh sách hàng tồn (Quantity > 0), Danh sách hàng cần nhập (Quantity == 0)
        // Dựa trên Partial View theo số lượng tồn (0.25)
        public async Task<IActionResult> Index()
        {
            var inStock = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity > 0)
                .OrderBy(p => p.Quantity)
                .ToListAsync();

            var outOfStock = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity == 0)
                .OrderByDescending(p => p.SoldQuantity)
                .ToListAsync();

            var viewModel = new InventoryViewModel
            {
                InStockProducts = inStock,
                OutOfStockProducts = outOfStock
            };

            return View(viewModel);
        }
    }
}
