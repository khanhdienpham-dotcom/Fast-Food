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

        // BAREM GIỎ HÀNG:
        // - Có icon giỏ hàng trên trang Menu chưa? (0.125)
        // - Có hiển thị số lượng hàng trong giỏ không? (0.125)
        // - Đã kiểm tra số lượng hàng khi tăng giảm số lượng? (không cho nhập < 0, và > số lượng tồn) và cập nhật lại số lượng hàng trong giỏ (0.125)
        // - Đã cập nhật được tổng tiền khi thay đổi số lượng hàng chưa? (0.125)
        // - Đã remove được sản phẩm ra khỏi giỏ và cập nhật lại số lượng hàng trong giỏ (0.125)
        // - Có link Tiếp tục mua hàng không (0.125)
        // - Có link Thanh toán không (0.125)
        // - Link Thanh toán có ẩn khi giỏ hàng rỗng không (0.125)
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();

            // Đồng bộ lại tồn kho & đơn giá mới nhất từ DB (Admin có thể đã sửa giá / số lượng tồn
            // hoặc khách khác đã mua hết) để nút + / ô nhập số lượng kiểm tra đúng số lượng tồn hiện tại.
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

        // Thêm vào giỏ hàng
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

            // Không cho phép số lượng mua < 1 (chặn dữ liệu âm / bằng 0 gửi từ client)
            if (quantity < 1) quantity = 1;
            // Chặn tấn công tràn số: gửi quantity = 2147483647 làm item.Quantity + quantity bị tràn thành số ÂM
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

        // Kiểm tra số lượng khi tăng giảm: không < 0, không > số lượng tồn
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
                    // Không cho nhập số âm: giữ nguyên số lượng cũ
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

        // Chỉ nhận POST + token: trước đây là link GET nên trang web khác chỉ cần nhúng
        // <img src="/Cart/ClearCart"> là xóa sạch giỏ hàng của khách (tấn công CSRF)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Đã làm trống giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        // BAREM THANH TOÁN:
        // - Đã ẩn nút thanh toán khi giỏ hàng rỗng (0.125)
        // - Đăng nhập thành công, có truyền thông tin người dùng vào phần Thông tin thanh toán (0.125)
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

        // XỬ LÝ THANH TOÁN:
        // - Thanh toán thành công, đơn hàng được tạo và lưu vào 2 bảng Order và Order Detail (0.125)
        // - Đơn hàng có tổng tiền và Thành tiền chi tiết cho từng mục (0.125)
        // - Đã tính lại số lượng tồn và số lượng đã bán (0.25)
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

            // Chặn dữ liệu giỏ hàng bất thường (số lượng <= 0)
            if (cart.Any(i => i.Quantity <= 0))
            {
                cart.RemoveAll(i => i.Quantity <= 0);
                SaveCart(cart);
                TempData["ErrorMessage"] = "Giỏ hàng có số lượng không hợp lệ, vui lòng kiểm tra lại!";
                return RedirectToAction(nameof(Index));
            }

            // Dùng transaction: nếu 1 bước lỗi thì không tạo đơn dở dang và không trừ kho sai
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Kiểm tra lại số lượng tồn kho trước khi tạo đơn hàng
            foreach (var cartItem in cart)
            {
                var stockProduct = await _context.Products.FindAsync(cartItem.ProductId);
                if (stockProduct == null || !stockProduct.Status || stockProduct.Quantity < cartItem.Quantity)
                {
                    int available = stockProduct?.Quantity ?? 0;
                    TempData["ErrorMessage"] = $"Món '{cartItem.ProductName}' chỉ còn {available} phần trong kho. Vui lòng cập nhật lại giỏ hàng!";
                    return RedirectToAction(nameof(Index));
                }
                // Luôn lấy đơn giá mới nhất từ DB (tránh giá cũ trong Session khi Admin đã đổi giá)
                cartItem.Price = stockProduct.Price;
            }

            // Chỉ chấp nhận phương thức thanh toán hợp lệ
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

            // Mã đơn dễ đọc, không bao giờ trùng: FF-yyyyMMdd-<mã đơn>
            order.OrderCode = $"FF-{order.OrderDate:yyyyMMdd}-{order.OrderId:D4}";
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            HttpContext.Session.Remove("Cart");
            // Ghi nhớ đơn vừa đặt để trang Success chỉ cho xem đơn của chính mình
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

            // CHẶN XEM TRỘM ĐƠN NGƯỜI KHÁC: trước đây gõ /Cart/Success?orderId=1, 2, 3...
            // là xem được tên, SĐT, địa chỉ của mọi khách hàng.
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
