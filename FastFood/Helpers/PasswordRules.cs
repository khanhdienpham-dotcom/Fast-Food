namespace FastFood.Helpers
{
    /// <summary>
    /// Quy định độ dài mật khẩu dùng chung cho Đăng ký, Hồ sơ, Admin thêm/sửa tài khoản.
    /// Muốn đổi độ dài thì sửa MinLength/MaxLength VÀ câu LengthMessage bên dưới cho khớp.
    /// </summary>
    public static class PasswordRules
    {
        public const int MinLength = 6;
        public const int MaxLength = 100;
        public const string LengthMessage = "Mật khẩu phải từ 6 đến 100 ký tự";

        public static bool IsValidLength(string? password) =>
            password != null && password.Length >= MinLength && password.Length <= MaxLength;
    }
}
