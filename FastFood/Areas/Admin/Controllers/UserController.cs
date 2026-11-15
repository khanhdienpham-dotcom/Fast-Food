using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class UserController : Controller
    {
        private readonly FastFoodDbContext _context;

        public UserController(FastFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var adminUsers = await _context.Users
                .Where(u => u.Role == "Admin")
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(adminUsers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Password,FullName,Email,Phone,Address,Status")] User user)
        {
            string usernameLower = (user.Username ?? string.Empty).Trim().ToLower();
            string emailLower = (user.Email ?? string.Empty).Trim().ToLower();
            if (usernameLower.Length > 0 && await _context.Users.AnyAsync(u => u.Username.ToLower() == usernameLower))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã được sử dụng!");
            }
            if (emailLower.Length > 0 && await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower))
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng!");
            }

            if (ModelState.IsValid)
            {
                user.Role = "Admin";
                user.CreatedAt = DateTime.Now;
                user.Password = FastFood.Helpers.PasswordHelper.Hash(user.Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm quản trị viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Admin");
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Password,FullName,Email,Phone,Address,Status")] User user)
        {
            if (id != user.UserId) return NotFound();

            var existing = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Admin");
            if (existing == null) return NotFound();

            bool keepOldPassword = string.IsNullOrEmpty(user.Password);
            if (keepOldPassword)
            {
                ModelState.Remove("Password");
            }
            ModelState.Remove("Username");
            user.Username = existing.Username;

            string editEmailLower = (user.Email ?? string.Empty).Trim().ToLower();
            if (editEmailLower.Length > 0 && await _context.Users.AnyAsync(u => u.UserId != user.UserId && u.Email.ToLower() == editEmailLower))
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng!");

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == user.UserId && !user.Status)
                ModelState.AddModelError("Status", "Không thể tự khóa tài khoản đang đăng nhập!");

            if (ModelState.IsValid)
            {
                existing.FullName = user.FullName!.Trim();
                existing.Email = user.Email!.Trim();
                existing.Phone = user.Phone!.Trim();
                existing.Address = user.Address;
                if (!keepOldPassword)
                    existing.Password = FastFood.Helpers.PasswordHelper.Hash(user.Password);
                existing.Status = user.Status;
                await _context.SaveChangesAsync();

                if (currentUserId == existing.UserId)
                    HttpContext.Session.SetString("FullName", existing.FullName);

                TempData["SuccessMessage"] = "Cập nhật quản trị viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            user.CreatedAt = existing.CreatedAt;
            return View(user);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Admin");
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == id && m.Role == "Admin");
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Admin");
            if (user == null) return NotFound();

            if (HttpContext.Session.GetInt32("UserId") == user.UserId)
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            int adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            if (adminCount <= 1)
            {
                TempData["ErrorMessage"] = "Không thể xóa quản trị viên duy nhất!";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa tài khoản quản trị viên!";
            return RedirectToAction(nameof(Index));
        }
    }
}
