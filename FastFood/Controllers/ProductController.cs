using FastFood.Models;
using FastFood.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers
{
    public class ProductController : Controller
    {
        private readonly FastFoodDbContext _context;

        public ProductController(FastFoodDbContext context)
        {
            _context = context;
        }

        // BAREM PRODUCT CATEGORY:
        // - Đã có chức năng tìm kiếm chưa? (0.125)
        // - Đã phân trang kết quả tìm kiếm chưa (0.25)
        // - Đã lọc được danh sách sản phẩm theo các tiêu chí lọc chưa (0.25) (Category, khoảng giá, sắp xếp)
        // - Sản phẩm hiển thị trên trang Product Category có số lượng tồn > 0 (0.125)
        // - Có cho phép xem chi tiết sản phẩm (0.125)
        // - Có cho phép MUA sản phẩm không? (0.125)
        public async Task<IActionResult> Index(int? categoryId, string? keyword, decimal? minPrice, decimal? maxPrice, string? sortOrder, int page = 1)
        {
            // CHỈ LẤY SẢN PHẨM CÓ SỐ LƯỢNG TỒN > 0 THEO YÊU CẦU BAREM
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status)
                .AsQueryable();

            // 1. Lọc theo Danh Mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Nếu người dùng nhập ngược khoảng giá (Từ > Đến) thì tự đổi chỗ
            if (minPrice.HasValue && maxPrice.HasValue && maxPrice.Value > 0 && minPrice.Value > maxPrice.Value)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            // LƯU Ý: SQLite không hỗ trợ so sánh / ORDER BY trên kiểu decimal (Price) và hàm lower()
            // của SQLite không xử lý được chữ có dấu tiếng Việt (Đ, Ồ, ...). Vì vậy phần tìm kiếm,
            // lọc giá, sắp xếp được thực hiện trên danh sách đã lấy về (dữ liệu nhỏ nên không ảnh hưởng hiệu năng).
            IEnumerable<Product> list = await query.ToListAsync();

            // 2. Tìm kiếm theo từ khóa (không phân biệt hoa thường)
            //    - Gõ KHÔNG dấu ("ga ran") -> so sánh không dấu, tìm được "Gà Rán"
            //    - Gõ CÓ dấu ("đồ") -> so sánh đúng dấu, không bị khớp nhầm "Double", "độc quyền"
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string kwTrim = keyword.Trim();
                bool hasAccent = RemoveDiacritics(kwTrim) != kwTrim.ToLowerInvariant();
                Func<string?, string> norm = hasAccent
                    ? (s => (s ?? string.Empty).ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormC))
                    : (s => RemoveDiacritics(s ?? string.Empty));
                string kw = norm(kwTrim);
                list = list.Where(p => norm(p.ProductName).Contains(kw) ||
                                       norm(p.Description).Contains(kw) ||
                                       (p.Category != null && norm(p.Category.CategoryName).Contains(kw)));
            }

            // 3. Lọc theo khoảng giá
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                list = list.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                list = list.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. Sắp xếp theo tiêu chí
            list = sortOrder switch
            {
                "price_asc" => list.OrderBy(p => p.Price),
                "price_desc" => list.OrderByDescending(p => p.Price),
                "popular" => list.OrderByDescending(p => p.SoldQuantity),
                _ => list.OrderByDescending(p => p.ProductId)
            };

            // 5. Phân trang kết quả tìm kiếm (0.25)
            var filtered = list.ToList();
            int pageSize = 6;
            int totalItems = filtered.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var products = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = await _context.Categories
                .Where(c => c.Status)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var viewModel = new ProductFilterViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                Keyword = keyword,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // BAREM PRODUCT DETAIL:
        // - Đã hiển thị đầy đủ thông tin sản phẩm theo như trang tham khảo? (0.125)
        // - Có cho phép MUA sản phẩm không? (0.125)
        // - Có truyền được số lượng mua sang trang giỏ hàng không? (0.25)
        // - Sau khi thêm hàng vào giỏ hàng, có chuyển tới trang giỏ hàng hoặc trang Product Category không?
        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null || !product.Status) return NotFound();

            // Sản phẩm liên quan trong cùng category
            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId && p.Status && p.Quantity > 0)
                .Take(3)
                .ToListAsync();

            return View(product);
        }

        // Bỏ dấu tiếng Việt + chuyển chữ thường để so sánh khi tìm kiếm
        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            var normalized = text.ToLowerInvariant().Replace('đ', 'd').Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
