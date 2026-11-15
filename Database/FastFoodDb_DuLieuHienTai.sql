/* =====================================================================
   FASTFOOD STORE - DỮ LIỆU HIỆN TẠI (xuất lúc 15/11/2026 21:06)
   Cách dùng (SSMS hoặc SQL Server Object Explorer trong Visual Studio):
     1. Chạy FastFoodDb.sql trước (tạo database + bảng)
     2. Chạy file này: xóa dữ liệu mẫu, nạp toàn bộ dữ liệu hiện tại
   Mật khẩu trong bảng Users đã được mã hóa (PBKDF2), không phải mật khẩu gốc.
   ===================================================================== */
USE FastFoodDb;
GO

/* Xóa dữ liệu cũ: bảng con trước, bảng cha sau */
DELETE FROM OrderDetails;
DELETE FROM Orders;
DELETE FROM Users;
DELETE FROM Products;
DELETE FROM Categories;
GO

/* ---------- Categories ---------- */
SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (1, N'Burger', N'Burger bò, gà, phô mai chuẩn vị Mỹ', N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&q=80', 1, 1);
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (2, N'Gà Rán', N'Gà rán giòn rụm, vàng ươm, cay và không cay', N'https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=400&q=80', 2, 1);
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (3, N'Pizza', N'Pizza phô mai kéo sợi, hải sản, xúc xích', N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&q=80', 3, 1);
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (4, N'Khoai Tây & Ăn Kèm', N'Khoai tây chiên giòn, phô mai que, xúc xích', N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=400&q=80', 4, 1);
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (5, N'Đồ Uống', N'Coca Cola, Pepsi, Trà đào, Trà sữa mát lạnh', N'https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=400&q=80', 5, 1);
INSERT INTO [Categories] ([CategoryId], [CategoryName], [Description], [ImageUrl], [DisplayOrder], [Status]) VALUES (6, N'Combo Siêu Tiết Kiệm', N'Combo gia đình, combo cặp đôi đầy đủ món và nước', N'https://images.unsplash.com/photo-1551782450-a2132b4ba21d?w=400&q=80', 6, 1);
SET IDENTITY_INSERT [Categories] OFF;
DECLARE @max_Categories INT = ISNULL((SELECT MAX(CategoryId) FROM [Categories]), 0);
DBCC CHECKIDENT ('Categories', RESEED, @max_Categories);
GO

/* ---------- Products ---------- */
SET IDENTITY_INSERT [Products] ON;
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (1, N'Double Beef Burger', N'2 miếng bò Úc nướng lửa hồng, phô mai Cheddar béo ngậy, sốt tiêu đen đặc biệt và rau xà lách tươi xanh.', 59000.00, N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&q=80', 45, 120, 1, 1, '2026-10-25T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (2, N'Burger Bò Phô Mai Nấm', N'Burger bò thượng hạng kèm nấm xào bơ tỏi thơm lừng và sốt nấm Truffle.', 65000.00, N'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=600&q=80', 30, 85, 1, 1, '2026-10-28T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (3, N'Gà Rán Giòn Cay (2 miếng)', N'Gà tươi 100% tẩm ướp gia vị cay độc quyền, chiên vàng giòn rụm bên ngoài mọng nước bên trong.', 45000.00, N'https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=600&q=80', 60, 210, 2, 1, '2026-10-20T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (4, N'Cánh Gà Sốt BBQ (4 miếng)', N'Cánh gà chiên giòn phủ đẫm sốt BBQ chua ngọt kiểu Mỹ thơm nức mũi.', 55000.00, N'https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=600&q=80', 25, 64, 2, 1, '2026-10-30T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (5, N'Pizza Hải Sản Phô Mai', N'Đế bánh mỏng giòn, tôm mực tươi ngon, phủ phô mai Mozzarella kéo sợi béo ngậy.', 89000.00, N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=600&q=80', 19, 53, 3, 1, '2026-11-01T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (6, N'Khoai Tây Chiên Lắc Phô Mai', N'Khoai tây chiên vàng giòn rụm lắc cùng bột phô mai mặn ngọt đậm đà khó cưỡng.', 29000.00, N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&q=80', 47, 323, 4, 1, '2026-10-15T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (7, N'Coca Cola Tươi Size L', N'Coca Cola tươi mát lạnh sảng khoái cực đã khi dùng kèm đồ chiên rán.', 20000.00, N'https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=600&q=80', 99, 541, 5, 1, '2026-10-10T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (8, N'Combo Solo No Nê', N'Gồm 1 Double Beef Burger + 1 Khoai tây chiên + 1 Coca Cola mát lạnh. Tiết kiệm 20.000đ.', 89000.00, N'https://images.unsplash.com/photo-1585032226651-759b368d7246?w=600&q=80', 35, 180, 6, 1, '2026-10-26T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (9, N'Combo Gia Đình Thịnh Soạn', N'Gồm 2 Burger bò phô mai, 4 miếng gà rán cay, 2 phần khoai lắc và 4 ly nước ngọt.', 199000.00, N'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=600&q=80', 15, 45, 6, 1, '2026-11-02T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (10, N'Pizza Pepperoni Xúc Xích Ý', N'Pizza xúc xích bò cay truyền thống Ý kết hợp sốt cà chua tươi hảo hạng.', 79000.00, N'https://images.unsplash.com/photo-1628840042765-356cda07504e?w=600&q=80', 1, 95, 3, 1, '2026-11-04T07:29:51.4900000');
INSERT INTO [Products] ([ProductId], [ProductName], [Description], [Price], [ImageUrl], [Quantity], [SoldQuantity], [CategoryId], [Status], [CreatedAt]) VALUES (11, N'Mực Chiên Giòn Calamari', N'Khoanh mực tươi tẩm bột chiên phồng chấm kèm sốt Tartar chua thanh dịu ngọt.', 49000.00, N'https://images.unsplash.com/photo-1599488615731-7e5c2823ff28?w=600&q=80', 0, 32, 4, 1, '2026-11-05T07:29:51.4900000');
SET IDENTITY_INSERT [Products] OFF;
DECLARE @max_Products INT = ISNULL((SELECT MAX(ProductId) FROM [Products]), 0);
DBCC CHECKIDENT ('Products', RESEED, @max_Products);
GO

/* ---------- Users ---------- */
SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [Username], [Password], [FullName], [Email], [Phone], [Address], [Role], [CreatedAt], [Status]) VALUES (1, N'admin', N'PBKDF2$100000$mebZQSVDZh138L+2MTs6hQ==$cR0GkL5saSindfe3ES43Iw/W6efOiwzVOgEM82H9O2c=', N'Quản Trị Hệ Thống', N'admin@fastfood.vn', N'0988888888', N'123 Nguyễn Trãi, Quận 1, TP.HCM', N'Admin', '2026-10-10T07:29:51.4933333', 1);
INSERT INTO [Users] ([UserId], [Username], [Password], [FullName], [Email], [Phone], [Address], [Role], [CreatedAt], [Status]) VALUES (2, N'khachhang', N'PBKDF2$100000$rPQ3ZwTx9wBUz8LVwgHWyg==$OyZwYyPujQI2m48iJuEVYk3cBsHX3e59DBWVsTj2lr4=', N'Nguyễn Văn A', N'nguyenvana@gmail.com', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM', N'Customer', '2026-10-20T07:29:51.4933333', 1);
INSERT INTO [Users] ([UserId], [Username], [Password], [FullName], [Email], [Phone], [Address], [Role], [CreatedAt], [Status]) VALUES (3, N'tranb', N'123', N'Trần Thị B', N'tranthib@gmail.com', N'0912345678', N'789 Điện Biên Phủ, Bình Thạnh, TP.HCM', N'Customer', '2026-10-30T07:29:51.4933333', 1);
INSERT INTO [Users] ([UserId], [Username], [Password], [FullName], [Email], [Phone], [Address], [Role], [CreatedAt], [Status]) VALUES (4, N'Huy123', N'PBKDF2$100000$JurKOoZLA9c2Ba0PitKXAQ==$DdsTjMNVFYRSURezAxXQkFFu5x7STX55oVJAtTVp8kM=', N'Huy', N'khanhdienpham@gmail.com', N'08123128139', N'quoc lo 50', N'Customer', '2026-11-09T08:19:21.8050704', 1);
SET IDENTITY_INSERT [Users] OFF;
DECLARE @max_Users INT = ISNULL((SELECT MAX(UserId) FROM [Users]), 0);
DBCC CHECKIDENT ('Users', RESEED, @max_Users);
GO

/* ---------- Orders ---------- */
SET IDENTITY_INSERT [Orders] ON;
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (1, N'FF-20261108-0001', 2, N'Nguyễn Văn A', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM', N'Giao giờ hành chính, không bấm chuông', 149000.00, N'COD', N'Đã giao', '2026-11-07T07:29:51.5000000');
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (2, N'FF-20261108-0002', 2, N'Nguyễn Văn A', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM', N'Cho thêm nhiều tương cà và tương ớt', 109000.00, N'COD', N'Đang chuẩn bị', '2026-11-09T06:29:51.5000000');
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (3, N'FF-20261108-0003', 3, N'Trần Thị B', N'0912345678', N'789 Điện Biên Phủ, Bình Thạnh, TP.HCM', N'Giao trước 18h', 199000.00, N'VNPAY', N'Đang giao', '2026-11-09T04:29:51.5000000');
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (4, N'FF-20261109-0004', 4, N'Huy', N'08123128139', N'quoc lo 50', NULL, 138000.00, N'COD', N'Đang chuẩn bị', '2026-11-09T08:20:45.5146896');
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (7, N'FF-20261109-0007', 4, N'Huy', N'08123128139', N'quoc lo 50', NULL, 29000.00, N'COD', N'Đang chuẩn bị', '2026-11-09T08:30:36.9460368');
INSERT INTO [Orders] ([OrderId], [OrderCode], [UserId], [CustomerName], [Phone], [Address], [Note], [TotalAmount], [PaymentMethod], [Status], [OrderDate]) VALUES (8, N'FF-20261115-0008', 1, N'Quản Trị Hệ Thống', N'0988888888', N'123 Nguyễn Trãi, Quận 1, TP.HCM', NULL, 29000.00, N'COD', N'Đã giao', '2026-11-15T19:42:42.2987582');
SET IDENTITY_INSERT [Orders] OFF;
DECLARE @max_Orders INT = ISNULL((SELECT MAX(OrderId) FROM [Orders]), 0);
DBCC CHECKIDENT ('Orders', RESEED, @max_Orders);
GO

/* ---------- OrderDetails ---------- */
SET IDENTITY_INSERT [OrderDetails] ON;
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (1, 1, 1, N'Double Beef Burger', 59000.00, 1, 59000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (2, 1, 3, N'Gà Rán Giòn Cay (2 miếng)', 45000.00, 2, 90000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (3, 2, 8, N'Combo Solo No Nê', 89000.00, 1, 89000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (4, 2, 7, N'Coca Cola Tươi Size L', 20000.00, 1, 20000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (5, 3, 9, N'Combo Gia Đình Thịnh Soạn', 199000.00, 1, 199000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (6, 4, 7, N'Coca Cola Tươi Size L', 20000.00, 1, 20000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (7, 4, 6, N'Khoai Tây Chiên Lắc Phô Mai', 29000.00, 1, 29000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (8, 4, 5, N'Pizza Hải Sản Phô Mai', 89000.00, 1, 89000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (11, 7, 6, N'Khoai Tây Chiên Lắc Phô Mai', 29000.00, 1, 29000.00);
INSERT INTO [OrderDetails] ([OrderDetailId], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [SubTotal]) VALUES (12, 8, 6, N'Khoai Tây Chiên Lắc Phô Mai', 29000.00, 1, 29000.00);
SET IDENTITY_INSERT [OrderDetails] OFF;
DECLARE @max_OrderDetails INT = ISNULL((SELECT MAX(OrderDetailId) FROM [OrderDetails]), 0);
DBCC CHECKIDENT ('OrderDetails', RESEED, @max_OrderDetails);
GO
