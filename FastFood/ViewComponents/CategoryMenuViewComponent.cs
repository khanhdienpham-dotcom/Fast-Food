using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly FastFoodDbContext _context;

        public CategoryMenuViewComponent(FastFoodDbContext context)
        {
            _context = context;
        }

        // view = "Default" (menu dropdown trên thanh điều hướng) hoặc "Footer" (danh sách link ở chân trang)
        public async Task<IViewComponentResult> InvokeAsync(string view = "Default")
        {
            var categories = await _context.Categories
                .Where(c => c.Status)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(view, categories);
        }
    }
}
