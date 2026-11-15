# Database FastFood

| File | Dùng để làm gì |
|---|---|
| `FastFoodDb.sql` | Tạo database `FastFoodDb`, 5 bảng và **dữ liệu mẫu**. Chạy file này trước. |
| `FastFoodDb_DuLieuHienTai.sql` | **Dữ liệu thật** đang dùng (danh mục, món ăn, tài khoản, đơn hàng). Chạy sau `FastFoodDb.sql`: file sẽ xóa dữ liệu mẫu rồi nạp dữ liệu này vào. |
| `XuatDuLieu.cmd` / `XuatDuLieu.ps1` | Xuất lại dữ liệu hiện tại trong SQL Server LocalDB trên máy ra `FastFoodDb_DuLieuHienTai.sql`. Bấm đúp `XuatDuLieu.cmd` là chạy. |

## Cách lấy dữ liệu giống nhóm

1. Mở **SQL Server Object Explorer** trong Visual Studio (hoặc SSMS), kết nối `(localdb)\mssqllocaldb`.
2. Mở và chạy (Execute) `FastFoodDb.sql`.
3. Mở và chạy `FastFoodDb_DuLieuHienTai.sql`.
4. Chạy web như bình thường (`appsettings.json` đang để `"DatabaseProvider": "SqlServer"`).

## Cập nhật dữ liệu mới lên GitHub

Sau khi thêm món, tạo đơn... trên máy mình: bấm đúp `Database\XuatDuLieu.cmd`, rồi commit + push file `FastFoodDb_DuLieuHienTai.sql`.

> Mật khẩu trong bảng `Users` đã được mã hóa (PBKDF2). Tài khoản mẫu: `admin` / `123`, `khachhang` / `123`, `tranb` / `123`.
