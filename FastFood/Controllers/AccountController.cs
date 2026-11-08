using FastFood.Models;
using FastFood.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers
{
    public class AccountController : Controller
    {
        private readonly FastFoodDbContext _context;

        public AccountController(FastFoodDbContext context)
        {
            _context = context;
        }

        // BAREM ĐĂNG KÝ:
        // - Đã kiểm tra dữ liệu nhập (Data Annotation: độ dài, trường bắt buộc, kiểu dữ liệu, giá trị dữ liệu), trùng thông tin với tài khoản đã có chưa ? (0.125)
        // - Đăng ký thất bại, có hiển thị thông báo? (0.125)
        // - Đăng ký thành công, có redirect về trang Đăng nhập? (0.125)
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Ô bỏ trống sẽ được bind thành null -> phải kiểm tra trước khi gọi ToLower() (tránh lỗi 500)
            string usernameLower = (model.Username ?? string.Empty).Trim().ToLower();
            string emailLower = (model.Email ?? string.Empty).Trim().ToLower();

            if (usernameLower.Length > 0 && await _context.Users.AnyAsync(u => u.Username.ToLower() == usernameLower))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại trong hệ thống!");
            }

            if (emailLower.Length > 0 && await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower))
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được đăng ký tài khoản!");
            }

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    Username = model.Username.Trim(),
                    Password = model.Password,
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Phone = model.Phone.Trim(),
                    Address = model.Address?.Trim(),
                    Role = "Customer",
                    CreatedAt = DateTime.Now,
                    Status = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction(nameof(Login));
            }

            TempData["ErrorMessage"] = "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin!";
            return View(model);
        }

        // BAREM ĐĂNG NHẬP:
        // - Đăng nhập thất bại, có hiển thị thông báo? (0.125)
        // - Đăng nhập thành công, có đổi Icon thành icon Hồ sơ người dùng (ẩn 2 link Đăng ký và Đăng nhập) (0.125)
        // - Đăng nhập thành công, có hiển thị tên người dùng cạnh Hồ sơ người dùng (0.125)
        // - Đăng nhập thành công, có truyền thông tin người dùng vào phần Thông tin thanh toán (0.125)
        // - Đăng nhập thành công, dưới icon Hồ sơ người dùng, có hiển thị 2 link: Lịch sử giao dịch và Hồ sơ khách hàng (0.25)
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (ModelState.IsValid)
            {
                string loginName = model.Username.Trim().ToLower();
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    (u.Username.ToLower() == loginName || u.Email.ToLower() == loginName)
                    && u.Password == model.Password);

                if (user != null)
                {
                    if (!user.Status)
                    {
                        TempData["ErrorMessage"] = "Tài khoản của bạn đang tạm thời bị khóa!";
                        return View(model);
                    }

                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FullName", user.FullName);
                    HttpContext.Session.SetString("Role", user.Role);
                    HttpContext.Session.SetInt32("UserId", user.UserId);

                    TempData["SuccessMessage"] = $"Chào mừng bạn trở lại, {user.FullName}!";

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác!");
            }

            return View(model);
        }

        public async Task<IActionResult> Profile()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username)) return RedirectToAction(nameof(Login));

            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Phone))
            {
                TempData["ErrorMessage"] = "Họ tên và số điện thoại không được để trống!";
                return RedirectToAction(nameof(Profile));
            }
            if (!string.IsNullOrEmpty(model.Password) && model.Password.Length < 4)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới phải từ 4 ký tự trở lên!";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                user.FullName = model.FullName.Trim();
                user.Phone = model.Phone.Trim();
                user.Address = model.Address;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }

                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("FullName", user.FullName);
                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            }
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất an toàn khỏi hệ thống!";
            return RedirectToAction("Index", "Home");
        }
    }
}
