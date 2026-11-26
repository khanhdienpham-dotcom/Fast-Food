using System.Text;
using QRCoder;

namespace FastFood.Helpers
{
    public static class VietQrHelper
    {
        private static string Field(string id, string value) => id + value.Length.ToString("D2") + value;

        public static string BuildPayload(string bankBin, string accountNo, decimal amount, string content)
        {
            string merchant = Field("00", "A000000727")
                            + Field("01", Field("00", bankBin) + Field("01", accountNo))
                            + Field("02", "QRIBFTTA");

            var sb = new StringBuilder();
            sb.Append(Field("00", "01"));
            sb.Append(Field("01", "12"));
            sb.Append(Field("38", merchant));
            sb.Append(Field("53", "704"));
            sb.Append(Field("54", ((long)Math.Round(amount)).ToString()));
            sb.Append(Field("58", "VN"));
            sb.Append(Field("62", Field("08", Ascii(content, 25))));
            sb.Append("6304");
            sb.Append(Crc16(sb.ToString()).ToString("X4"));
            return sb.ToString();
        }

        public static string ToPngBase64(string payload, int pixelsPerModule = 7)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data).GetGraphic(pixelsPerModule);
            return Convert.ToBase64String(png);
        }

        private static string Ascii(string text, int maxLen)
        {
            var normalized = (text ?? string.Empty).Replace('đ', 'd').Replace('Đ', 'D').Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (c < 128 && (char.IsLetterOrDigit(c) || c == ' ')) sb.Append(c);
            }
            var result = sb.ToString().Trim();
            return result.Length > maxLen ? result[..maxLen] : result;
        }

        private static ushort Crc16(string data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in Encoding.ASCII.GetBytes(data))
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                    crc = (crc & 0x8000) != 0 ? (ushort)((crc << 1) ^ 0x1021) : (ushort)(crc << 1);
            }
            return crc;
        }
    }
}
