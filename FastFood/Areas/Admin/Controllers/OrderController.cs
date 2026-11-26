using FastFood.Helpers;
using FastFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Areas.Admin.Controllers
{
    [Area("Admin")]
    [FastFood.Helpers.AdminAuthorize]
    public class OrderController : Controller
    {
        private readonly FastFoodDbContext _context;

        public OrderController(FastFoodDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc đã bị xóa!";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == status)
            {
                TempData["SuccessMessage"] = "Trạng thái đơn hàng không thay đổi.";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            var error = OrderStatus.ValidateTransition(order.Status, status);
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            if (status == OrderStatus.Cancelled)
            {
                foreach (var d in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(d.ProductId);
                    if (product != null)
                    {
                        product.Quantity += d.Quantity;
                        product.SoldQuantity = Math.Max(0, product.SoldQuantity - d.Quantity);
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();

            string note = status switch
            {
                OrderStatus.Cancelled => $" Đã trừ {order.TotalAmount:N0} đ khỏi doanh thu và hoàn lại tồn kho.",
                OrderStatus.Delivered when order.PaymentMethod == "COD" => $" Đã thu tiền mặt {order.TotalAmount:N0} đ, cộng vào doanh thu.",
                _ => string.Empty
            };
            TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{order.OrderCode} thành: {status}.{note}";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại hoặc đã bị xóa!";
                return RedirectToAction(nameof(Index));
            }

            if (order.Status == OrderStatus.Delivered)
            {
                TempData["ErrorMessage"] = $"Đơn hàng #{order.OrderCode} đã giao thành công và đã tính vào doanh thu, không thể xóa!";
                return RedirectToAction(nameof(Details), new { id });
            }

            bool restoreStock = order.Status == OrderStatus.Preparing || order.Status == OrderStatus.Shipping;
            if (restoreStock)
            {
                foreach (var d in order.OrderDetails)
                {
                    var product = await _context.Products.FindAsync(d.ProductId);
                    if (product != null)
                    {
                        product.Quantity += d.Quantity;
                        product.SoldQuantity = Math.Max(0, product.SoldQuantity - d.Quantity);
                    }
                }
            }

            string code = order.OrderCode;
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = restoreStock
                ? $"Đã xóa đơn hàng #{code} và hoàn lại số lượng tồn kho!"
                : $"Đã xóa đơn hàng #{code}!";
            return RedirectToAction(nameof(Index));
        }
    }
}
