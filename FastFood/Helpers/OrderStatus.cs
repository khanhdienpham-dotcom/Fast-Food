using FastFood.Models;

namespace FastFood.Helpers
{
    public static class OrderStatus
    {
        public const string Preparing = "Đang chuẩn bị";
        public const string Shipping = "Đang giao";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        public static readonly string[] All = { Preparing, Shipping, Delivered, Cancelled };

        public static bool IsFinal(string status) => status == Delivered || status == Cancelled;

        private static int Rank(string status) => status switch
        {
            Preparing => 0,
            Shipping => 1,
            Delivered => 2,
            _ => -1
        };

        public static string? ValidateTransition(string current, string next)
        {
            if (!All.Contains(next)) return "Trạng thái đơn hàng không hợp lệ!";
            if (current == Delivered)
                return "Đơn hàng đã giao thành công, không thể cập nhật lại trạng thái!";
            if (current == Cancelled)
                return "Đơn hàng đã hủy, không thể cập nhật lại trạng thái!";
            if (next != Cancelled && Rank(next) < Rank(current))
                return $"Đơn hàng đang ở trạng thái \"{current}\", không thể quay lại \"{next}\"!";
            return null;
        }

        public static IEnumerable<string> AllowedNext(string current) =>
            All.Where(s => s == current || ValidateTransition(current, s) == null);

        public static bool IsCountedInRevenue(Order o) =>
            o.Status != Cancelled && (o.PaymentMethod != "COD" || o.Status == Delivered);

        public static bool IsPendingCod(Order o) =>
            o.PaymentMethod == "COD" && (o.Status == Preparing || o.Status == Shipping);
    }
}
