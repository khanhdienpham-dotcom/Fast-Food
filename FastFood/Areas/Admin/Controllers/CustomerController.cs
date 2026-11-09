using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class CustomerController : Controller
    {
        private readonly FastFoodDbContext _context;

        public CustomerController(FastFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.Role == "Customer")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(customers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Users
                .Include(u => u.Orders.OrderByDescending(o => o.OrderDate))
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.UserId == id && m.Role == "Customer");

            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}
