using FastFood.Helpers;
using FastFood.Models;
using FastFood.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Controllers
{
    public class CartController : Controller
    {
        private readonly FastFoodDbContext _context;

        public CartController(FastFoodDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }

        public async Task<IActionResult> Index()
        {
            var cart = GetCart();

            if (cart.Any())
            {
                var ids = cart.Select(c => c.ProductId).ToList();
                var products = await _context.Products.Where(p => ids.Contains(p.ProductId)).ToListAsync();
                bool changed = false;
                foreach (var item in cart.ToList())
                {
                    var p = products.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (p == null || !p.Status || p.Quantity <= 0)
                    {
                        cart.Remove(item);
                        TempData["ErrorMessage"] = $"Món '{item.ProductName}' đã hết hàng nên được bỏ khỏi giỏ.";
                        changed = true;
                        continue;
                    }
                    if (item.MaxQuantity != p.Quantity || item.Price != p.Price)
                    {
                        item.MaxQuantity = p.Quantity;
                        item.Price = p.Price;
                        changed = true;
                    }
                    if (item.Quantity > p.Quantity)
                    {
                        item.Quantity = p.Quantity;
                        TempData["ErrorMessage"] = $"Món '{item.ProductName}' chỉ còn {p.Quantity} phần, đã cập nhật lại số lượng.";
                        changed = true;
                    }
                }
                if (changed) SaveCart(cart);
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.Status || product.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại hoặc đã hết hàng!";
                return RedirectToAction("Index", "Product");
            }

            if (quantity < 1) quantity = 1;
            if (quantity > product.Quantity) quantity = product.Quantity;

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                item.MaxQuantity = product.Quantity;
                item.Price = product.Price;
                if ((long)item.Quantity + quantity > product.Quantity)
                {
                    item.Quantity = product.Quantity;
                    TempData["ErrorMessage"] = $"Số lượng trong giỏ đã đạt tối đa tồn kho ({product.Quantity} phần)!";
                }
                else
                {
                    item.Quantity += quantity;
                    TempData["SuccessMessage"] = $"Đã cập nhật số lượng món '{product.ProductName}'!";
                }
            }
            else
            {
                int addQty = Math.Min(quantity, product.Quantity);
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = addQty,
                    MaxQuantity = product.Quantity
                });
                TempData["SuccessMessage"] = $"Đã thêm '{product.ProductName}' vào giỏ hàng!";
            }

            SaveCart(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int? quantity)
        {
            var cart = GetCart();
            if (quantity == null)
            {
                TempData["ErrorMessage"] = "Số lượng nhập vào không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                var product = await _context.Products.FindAsync(productId);
                int maxStock = product?.Quantity ?? item.MaxQuantity;
                item.MaxQuantity = maxStock;

                if (quantity < 0)
                {
                    TempData["ErrorMessage"] = "Số lượng không được nhỏ hơn 0!";
                }
                else if (quantity == 0 || maxStock <= 0)
                {
                    cart.Remove(item);
                    TempData["SuccessMessage"] = "Đã xóa món khỏi giỏ hàng!";
                }
                else if (quantity > maxStock)
                {
                    item.Quantity = maxStock;
                    TempData["ErrorMessage"] = $"Số lượng tồn kho chỉ còn {maxStock} phần!";
                }
                else
                {
                    item.Quantity = quantity.Value;
                    TempData["SuccessMessage"] = "Đã cập nhật số lượng và tổng tiền!";
                }
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã xóa món khỏi giỏ hàng!";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Đã làm trống giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng rỗng, vui lòng chọn món trước!";
                return RedirectToAction("Index", "Product");
            }

            var model = new CheckoutViewModel { Items = cart };

            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.Phone = user.Phone;
                    model.Email = user.Email;
                    model.Address = user.Address ?? string.Empty;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng rỗng!";
                return RedirectToAction("Index", "Product");
            }

            if (!ModelState.IsValid)
            {
                model.Items = cart;
                return View("Checkout", model);
            }

            if (cart.Any(i => i.Quantity <= 0))
            {
                cart.RemoveAll(i => i.Quantity <= 0);
                SaveCart(cart);
                TempData["ErrorMessage"] = "Giỏ hàng có số lượng không hợp lệ, vui lòng kiểm tra lại!";
                return RedirectToAction(nameof(Index));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var cartItem in cart)
            {
                var stockProduct = await _context.Products.FindAsync(cartItem.ProductId);
                if (stockProduct == null || !stockProduct.Status || stockProduct.Quantity < cartItem.Quantity)
                {
                    int available = stockProduct?.Quantity ?? 0;
                    TempData["ErrorMessage"] = $"Món '{cartItem.ProductName}' chỉ còn {available} phần trong kho. Vui lòng cập nhật lại giỏ hàng!";
                    return RedirectToAction(nameof(Index));
                }
                cartItem.Price = stockProduct.Price;
            }

            if (model.PaymentMethod != "COD" && model.PaymentMethod != "VNPAY" && model.PaymentMethod != "MOMO")
                model.PaymentMethod = "COD";

            int? userId = null;
            var username = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null) userId = user.UserId;
            }

            var order = new Order
            {
                OrderCode = "FF-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + Guid.NewGuid().ToString("N")[..4].ToUpper(),
                UserId = userId,
                CustomerName = model.CustomerName.Trim(),
                Phone = model.Phone.Trim(),
                Address = model.Address.Trim(),
                Note = model.Note?.Trim(),
                PaymentMethod = model.PaymentMethod,
                Status = "Đang chuẩn bị",
                OrderDate = DateTime.Now,
                TotalAmount = cart.Sum(i => i.SubTotal)
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    SubTotal = item.SubTotal
                };
                _context.OrderDetails.Add(orderDetail);

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity = Math.Max(0, product.Quantity - item.Quantity);
                    product.SoldQuantity += item.Quantity;
                }
            }

            order.OrderCode = $"FF-{order.OrderDate:yyyyMMdd}-{order.OrderId:D4}";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Remove("Cart");
            HttpContext.Session.SetInt32("LastOrderId", order.OrderId);

            TempData["SuccessMessage"] = "Đặt hàng thành công!";
            return RedirectToAction("Success", new { orderId = order.OrderId });
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return RedirectToAction("Index", "Home");

            bool isJustPlaced = HttpContext.Session.GetInt32("LastOrderId") == order.OrderId;
            bool isOwner = order.UserId != null && HttpContext.Session.GetInt32("UserId") == order.UserId;
            bool isAdmin = HttpContext.Session.GetString("Role") == "Admin";
            if (!isJustPlaced && !isOwner && !isAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem đơn hàng này!";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }
    }
}
