namespace FastFood.Helpers
{
    /// <summary>
    /// Quy định độ dài mật khẩu dùng chung cho Đăng ký, Hồ sơ, Admin thêm/sửa tài khoản.
    /// Muốn đổi độ dài thì sửa MinLength/MaxLength VÀ câu LengthMessage bên dưới cho khớp.
    /// Lưu ý: đây là độ dài mật khẩu NGƯỜI DÙNG GÕ VÀO. Cột Users.Password trong database
    /// phải giữ 100 ký tự vì nó lưu chuỗi đã mã hóa PBKDF2 (dài khoảng 83 ký tự).
    /// </summary>
    public static class PasswordRules
    {
        public const int MinLength = 6;
        public const int MaxLength = 15;
        public const string LengthMessage = "Mật khẩu phải từ 6 đến 15 ký tự";

        public static bool IsValidLength(string? password) =>
            password != null && password.Length >= MinLength && password.Length <= MaxLength;
    }
}
