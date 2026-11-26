namespace FastFood.Helpers
{
    public static class PasswordRules
    {
        public const int MinLength = 6;
        public const int MaxLength = 15;
        public const string LengthMessage = "Mật khẩu phải từ 6 đến 15 ký tự";

        public static bool IsValidLength(string? password) =>
            password != null && password.Length >= MinLength && password.Length <= MaxLength;
    }
}
