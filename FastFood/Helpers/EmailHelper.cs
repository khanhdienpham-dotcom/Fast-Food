using FastFood.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Helpers
{
    /// <summary>
    /// Đảm bảo mỗi Gmail chỉ gắn với 1 tài khoản.
    /// Gmail bỏ qua dấu chấm và phần sau dấu "+" trong tên hộp thư, nên
    /// "nguyen.van.a@gmail.com", "nguyenvana+abc@gmail.com" và "NguyenVanA@googlemail.com"
    /// đều là CÙNG MỘT hộp thư -> phải coi là trùng.
    /// </summary>
    public static class EmailHelper
    {
        public static string Normalize(string? email)
        {
            var e = (email ?? string.Empty).Trim().ToLowerInvariant();
            int at = e.LastIndexOf('@');
            if (at <= 0) return e;

            string local = e[..at];
            string domain = e[(at + 1)..];

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                int plus = local.IndexOf('+');
                if (plus >= 0) local = local[..plus];
                local = local.Replace(".", "");
                domain = "gmail.com";
            }
            return local + "@" + domain;
        }

        /// <summary>Email (sau khi chuẩn hóa) đã thuộc về tài khoản khác chưa?</summary>
        public static async Task<bool> IsTakenAsync(FastFoodDbContext db, string? email, int? excludeUserId = null)
        {
            string target = Normalize(email);
            if (target.Length == 0) return false;

            var emails = await db.Users
                .Where(u => excludeUserId == null || u.UserId != excludeUserId)
                .Select(u => u.Email)
                .ToListAsync();

            return emails.Any(e => Normalize(e) == target);
        }
    }
}
