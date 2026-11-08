using System.Security.Cryptography;

namespace FastFood.Helpers
{
    /// <summary>
    /// Mã hóa mật khẩu một chiều bằng PBKDF2 (SHA256, có salt ngẫu nhiên).
    /// Chuỗi lưu trong DB có dạng: PBKDF2$số_vòng_lặp$salt_base64$hash_base64 (~83 ký tự, vừa cột Password 100 ký tự).
    /// Mật khẩu cũ lưu dạng chữ thường (vd. tài khoản mẫu "123") vẫn đăng nhập được
    /// và sẽ tự động được mã hóa lại ở lần đăng nhập thành công đầu tiên.
    /// </summary>
    public static class PasswordHelper
    {
        private const string Prefix = "PBKDF2$";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}{Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool IsHashed(string? stored) => stored != null && stored.StartsWith(Prefix);

        public static bool Verify(string password, string? stored)
        {
            if (string.IsNullOrEmpty(stored) || password == null) return false;

            if (!IsHashed(stored))
            {
                // Dữ liệu cũ chưa mã hóa
                return CryptographicOperations.FixedTimeEquals(
                    System.Text.Encoding.UTF8.GetBytes(password), System.Text.Encoding.UTF8.GetBytes(stored));
            }

            var parts = stored.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations)) return false;
            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
