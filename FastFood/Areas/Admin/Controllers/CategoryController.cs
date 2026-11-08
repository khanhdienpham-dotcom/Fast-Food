using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class CategoryController : Controller
    {
        private readonly FastFoodDbContext _context;

        public CategoryController(FastFoodDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Category
        // Index: có hiển thị tổng số HÀNG HÓA thuộc danh mục (0.25)
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        // GET: Admin/Category/Details/5
        // Details: có chứa Danh sách sản phẩm liên quan (Product Category) (0.25)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View(new Category { DisplayOrder = 1, Status = true });
        }

        // POST: Admin/Category/Create
        // Create: kiểm tra Data Annotation (0.125)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description,ImageUrl,DisplayOrder,Status")] Category category)
        {
            // Không cho tạo 2 danh mục trùng tên
            if (!string.IsNullOrWhiteSpace(category.CategoryName) &&
                await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower()))
                ModelState.AddModelError("CategoryName", "Tên danh mục này đã tồn tại!");
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Admin/Category/Edit/5
        // Edit: kiểm tra Data Annotation (0.125)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,Description,ImageUrl,DisplayOrder,Status")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (!string.IsNullOrWhiteSpace(category.CategoryName) &&
                await _context.Categories.AnyAsync(c => c.CategoryId != category.CategoryId && c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower()))
                ModelState.AddModelError("CategoryName", "Tên danh mục này đã tồn tại!");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.CategoryId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: Admin/Category/Delete/5
        // Delete: đã kiểm tra ràng buộc dữ liệu (constraints) với các bảng liên quan chưa? (0.125)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            // KIỂM TRA RÀNG BUỘC VỚI BẢNG PRODUCTS THEO YÊU CẦU BAREM
            if (category.Products != null && category.Products.Any())
            {
                // Thông báo lỗi nếu danh mục đang có sản phẩm
                TempData["ErrorMessage"] = $"Không thể xóa danh mục '{category.CategoryName}' vì đang có {category.Products.Count} sản phẩm liên kết!";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã xóa danh mục thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
}
