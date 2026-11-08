using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FastFood.Helpers
{
    /// <summary>
    /// Chỉ cho phép tài khoản có Role = "Admin" (lưu trong Session) truy cập khu vực quản trị.
    /// Chưa đăng nhập -> chuyển tới trang Đăng nhập; đăng nhập nhưng không phải Admin -> về trang chủ.
    /// </summary>
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var username = session.GetString("Username");
            var role = session.GetString("Role");

            if (string.IsNullOrEmpty(username))
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl });
                return;
            }
            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "" });
            }
        }
    }
}
