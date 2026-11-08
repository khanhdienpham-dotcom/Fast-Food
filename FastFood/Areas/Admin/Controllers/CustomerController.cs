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

        // Quản lý khách hàng: Trang Index: lưu giữ danh sách khách hàng, không có nút Thêm/Sửa/Xóa (0.125)
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Users
                .Include(u => u.Orders)
                .Where(u => u.Role == "Customer")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(customers);
        }

        // Trang Detail: hiển thị thông tin khách hàng + danh sách đơn hàng của khách hàng (0.25)
        // Tạo Partial View cho trang Index của Order để nhúng Danh sách đơn hàng vào trang Detail của Customer
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
