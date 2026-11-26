using FastFood.Helpers;
using FastFood.Models;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.ViewComponents
{
    public class PaymentQrViewModel
    {
        public string QrBase64 { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Content { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentQrViewComponent : ViewComponent
    {
        private readonly IConfiguration _config;

        public PaymentQrViewComponent(IConfiguration config)
        {
            _config = config;
        }

        public IViewComponentResult Invoke(Order order)
        {
            if (order == null || order.PaymentMethod == "COD" || order.Status == OrderStatus.Cancelled)
                return Content(string.Empty);

            var section = _config.GetSection("BankTransfer");
            string bankBin = section["BankBin"] ?? "970436";
            string accountNo = section["AccountNo"] ?? "0123456789";
            string content = "TT " + order.OrderCode.Replace("-", "");

            var payload = VietQrHelper.BuildPayload(bankBin, accountNo, order.TotalAmount, content);

            var model = new PaymentQrViewModel
            {
                QrBase64 = VietQrHelper.ToPngBase64(payload),
                BankName = section["BankName"] ?? "Vietcombank",
                AccountNo = accountNo,
                AccountName = section["AccountName"] ?? "FASTFOOD STORE",
                Amount = order.TotalAmount,
                Content = content,
                PaymentMethod = order.PaymentMethod
            };
            return View(model);
        }
    }
}
