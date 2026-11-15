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

        public async Task<IActionResult> Index(int? categoryId, string? keyword, decimal? minPrice, decimal? maxPrice, string? sortOrder, int page = 1)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Status && p.Quantity > 0 && p.Category != null && p.Category.Status)
                .AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (minPrice.HasValue && maxPrice.HasValue && maxPrice.Value > 0 && minPrice.Value > maxPrice.Value)
            {
                (minPrice, maxPrice) = (maxPrice, minPrice);
            }

            IEnumerable<Product> list = await query.ToListAsync();

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

            if (minPrice.HasValue && minPrice.Value > 0)
            {
                list = list.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                list = list.Where(p => p.Price <= maxPrice.Value);
            }

            list = sortOrder switch
            {
                "price_asc" => list.OrderBy(p => p.Price),
                "price_desc" => list.OrderByDescending(p => p.Price),
                "popular" => list.OrderByDescending(p => p.SoldQuantity),
                _ => list.OrderByDescending(p => p.ProductId)
            };

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

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null || !product.Status) return NotFound();

            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId && p.Status && p.Quantity > 0)
                .Take(3)
                .ToListAsync();

            return View(product);
        }

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
