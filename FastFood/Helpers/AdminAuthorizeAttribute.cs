using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FastFood.Helpers
{
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
                return;
            }

            var userId = session.GetInt32("UserId");
            var db = context.HttpContext.RequestServices.GetService(typeof(FastFood.Models.FastFoodDbContext)) as FastFood.Models.FastFoodDbContext;
            var user = (db != null && userId != null) ? db.Users.Find(userId.Value) : null;
            if (user == null || !user.Status || user.Role != "Admin")
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
            }
        }
    }
}
