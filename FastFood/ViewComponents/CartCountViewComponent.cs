using FastFood.Helpers;
using FastFood.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int count = cart.Sum(i => i.Quantity);
            return View(count);
        }
    }
}
