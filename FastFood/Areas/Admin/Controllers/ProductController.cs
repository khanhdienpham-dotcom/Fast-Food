using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class ProductController : Controller
    {
        private readonly FastFoodDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(FastFoodDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Chỉ cho phép ảnh .jpg/.jpeg/.png/.gif/.webp, tối đa 5MB
        private static readonly string[] AllowedImageExt = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private static string? ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExt.Contains(ext)) return "Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .gif, .webp";
            if (file.Length > 5 * 1024 * 1024) return "Dung lượng ảnh tối đa 5MB";
            return null;
        }

        // GET: Admin/Product
        // Trang Index: chỉ hiện 4-5 cột dữ liệu chính (tên, ảnh, giá, số lượng tồn, chủng loại) (0.125)
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .ToListAsync();

            return View(products);
        }

        // GET: Admin/Product/Details/5
        // Trang Detail: format giống trang Detail Product cho trang Customer (không có nút MUA HÀNG)
        // Tạo Partial View Detail Product để tái sử dụng cho cả 2 trang Admin và Customer (0.125)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Admin/Product/Create
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.Where(c => c.Status).OrderBy(c => c.DisplayOrder), "CategoryId", "CategoryName");
            return View(new Product { Status = true });
        }

        // POST: Admin/Product/Create
        // Create: Data Annotation + Upload được ảnh sản phẩm (0.25)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Description,Price,Quantity,CategoryId,Status")] Product product, IFormFile? uploadImage, string? imageUrl)
        {
            var imgError = ValidateImage(uploadImage);
            if (imgError != null) ModelState.AddModelError("", imgError);
            if (!await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId))
                ModelState.AddModelError("CategoryId", "Danh mục không tồn tại, vui lòng chọn lại!");
            if (!string.IsNullOrWhiteSpace(product.ProductName) &&
                await _context.Products.AnyAsync(p => p.ProductName.ToLower() == product.ProductName.Trim().ToLower()))
                ModelState.AddModelError("ProductName", "Tên món ăn này đã tồn tại!");
            if (ModelState.IsValid)
            {
                // XỬ LÝ UPLOAD ẢNH SẢN PHẨM (YÊU CẦU BAREM: Upload được ảnh sản phẩm)
                if (uploadImage != null && uploadImage.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadImage.FileName).ToLowerInvariant();
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadImage.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }
                else if (!string.IsNullOrEmpty(imageUrl))
                {
                    product.ImageUrl = imageUrl;
                }
                else
                {
                    product.ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&q=80";
                }

                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm mới món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        // Edit: Data Annotation + Upload ảnh (0.25)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Description,Price,Quantity,SoldQuantity,CategoryId,Status,ImageUrl")] Product product, IFormFile? uploadImage)
        {
            if (id != product.ProductId) return NotFound();

            var editImgError = ValidateImage(uploadImage);
            if (editImgError != null) ModelState.AddModelError("", editImgError);
            if (!await _context.Categories.AnyAsync(c => c.CategoryId == product.CategoryId))
                ModelState.AddModelError("CategoryId", "Danh mục không tồn tại, vui lòng chọn lại!");
            if (!string.IsNullOrWhiteSpace(product.ProductName) &&
                await _context.Products.AnyAsync(p => p.ProductId != product.ProductId && p.ProductName.ToLower() == product.ProductName.Trim().ToLower()))
                ModelState.AddModelError("ProductName", "Tên món ăn này đã tồn tại!");

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload ảnh mới nếu có
                    if (uploadImage != null && uploadImage.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        string uniqueFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(uploadImage.FileName).ToLowerInvariant();
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await uploadImage.CopyToAsync(fileStream);
                        }
                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    // Giữ nguyên ngày tạo gốc trong DB (không lấy từ form)
                    product.CreatedAt = await _context.Products.Where(p => p.ProductId == id)
                        .Select(p => p.CreatedAt).FirstOrDefaultAsync();
                    if (product.CreatedAt == default) product.CreatedAt = DateTime.Now;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Product/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        // Delete: kiểm tra ràng buộc dữ liệu với các bảng liên quan (OrderDetails) (0.125)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // KIỂM TRA RÀNG BUỘC VỚI BẢNG ORDER DETAILS (HÓA ĐƠN ĐÃ ĐẶT)
            if (product.OrderDetails != null && product.OrderDetails.Any())
            {
                TempData["ErrorMessage"] = $"Không thể xóa món '{product.ProductName}' vì món ăn này đã có trong {product.OrderDetails.Count} đơn hàng của khách!";
                return RedirectToAction(nameof(Index));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
