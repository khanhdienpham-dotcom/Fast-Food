using FastFood.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFood.Helpers
{
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
