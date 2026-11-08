/* =====================================================================
   FASTFOOD STORE - SCRIPT CƠ SỞ DỮ LIỆU (SQL Server)
   - Tạo database FastFoodDb, 5 bảng: Categories, Products, Users, Orders, OrderDetails
   - Khóa chính, khóa ngoại (FK) và ràng buộc giống hệt model Entity Framework
   - Dữ liệu mẫu giống DbInitializer.cs (6 danh mục, 11 món, 3 tài khoản, 3 đơn hàng)
   Cách dùng: mở bằng SQL Server Management Studio (SSMS) -> Execute (F5)
   Muốn web dùng SQL Server: appsettings.json đổi "DatabaseProvider": "SqlServer"
   ===================================================================== */

USE master;
GO
IF DB_ID(N'FastFoodDb') IS NOT NULL
BEGIN
    ALTER DATABASE FastFoodDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FastFoodDb;
END
GO
CREATE DATABASE FastFoodDb;
GO
USE FastFoodDb;
GO

/* ---------------------------- 1. DANH MỤC ---------------------------- */
CREATE TABLE Categories (
    CategoryId    INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    CategoryName  NVARCHAR(100) NOT NULL,
    Description   NVARCHAR(500) NULL,
    ImageUrl      NVARCHAR(255) NULL,
    DisplayOrder  INT NOT NULL CONSTRAINT DF_Categories_DisplayOrder DEFAULT 0,
    Status        BIT NOT NULL CONSTRAINT DF_Categories_Status DEFAULT 1,
    CONSTRAINT CK_Categories_DisplayOrder CHECK (DisplayOrder BETWEEN 0 AND 9999)
);
GO

/* ---------------------------- 2. SẢN PHẨM (có Quantity) ---------------------------- */
CREATE TABLE Products (
    ProductId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    ProductName   NVARCHAR(150) NOT NULL,
    Description   NVARCHAR(2000) NULL,
    Price         DECIMAL(18,2) NOT NULL,
    ImageUrl      NVARCHAR(300) NULL,
    Quantity      INT NOT NULL CONSTRAINT DF_Products_Quantity DEFAULT 0,       -- số lượng tồn
    SoldQuantity  INT NOT NULL CONSTRAINT DF_Products_SoldQuantity DEFAULT 0,   -- số lượng đã bán
    CategoryId    INT NOT NULL,
    Status        BIT NOT NULL CONSTRAINT DF_Products_Status DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId)
        REFERENCES Categories (CategoryId) ON DELETE NO ACTION,              -- Restrict: không xóa danh mục còn sản phẩm
    CONSTRAINT CK_Products_Price CHECK (Price >= 1000),
    CONSTRAINT CK_Products_Quantity CHECK (Quantity >= 0),
    CONSTRAINT CK_Products_SoldQuantity CHECK (SoldQuantity >= 0)
);
CREATE INDEX IX_Products_CategoryId ON Products (CategoryId);
GO

/* ---------------------------- 3. NGƯỜI DÙNG (Admin + Customer) ---------------------------- */
CREATE TABLE Users (
    UserId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username   NVARCHAR(50)  NOT NULL,
    Password   NVARCHAR(100) NOT NULL,
    FullName   NVARCHAR(100) NOT NULL,
    Email      NVARCHAR(100) NOT NULL,
    Phone      NVARCHAR(20)  NOT NULL,
    Address    NVARCHAR(250) NULL,
    Role       NVARCHAR(20)  NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'Customer',
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT GETDATE(),
    Status     BIT NOT NULL CONSTRAINT DF_Users_Status DEFAULT 1,
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Customer'))
);
GO

/* ---------------------------- 4. ĐƠN HÀNG ---------------------------- */
CREATE TABLE Orders (
    OrderId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    OrderCode      NVARCHAR(50)  NOT NULL,
    UserId         INT NULL,                         -- NULL = khách vãng lai
    CustomerName   NVARCHAR(100) NOT NULL,
    Phone          NVARCHAR(20)  NOT NULL,
    Address        NVARCHAR(250) NOT NULL,
    Note           NVARCHAR(500) NULL,
    TotalAmount    DECIMAL(18,2) NOT NULL CONSTRAINT DF_Orders_TotalAmount DEFAULT 0,
    PaymentMethod  NVARCHAR(50)  NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT N'COD',
    Status         NVARCHAR(50)  NOT NULL CONSTRAINT DF_Orders_Status DEFAULT N'Đang chuẩn bị',
    OrderDate      DATETIME2 NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT GETDATE(),
    CONSTRAINT FK_Orders_Users_UserId FOREIGN KEY (UserId)
        REFERENCES Users (UserId) ON DELETE SET NULL,
    CONSTRAINT CK_Orders_PaymentMethod CHECK (PaymentMethod IN (N'COD', N'VNPAY', N'MOMO')),
    CONSTRAINT CK_Orders_Status CHECK (Status IN (N'Đang chuẩn bị', N'Đang giao', N'Đã giao', N'Đã hủy'))
);
CREATE INDEX IX_Orders_UserId ON Orders (UserId);
GO

/* ---------------------------- 5. CHI TIẾT ĐƠN HÀNG ---------------------------- */
CREATE TABLE OrderDetails (
    OrderDetailId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderDetails PRIMARY KEY,
    OrderId        INT NOT NULL,
    ProductId      INT NOT NULL,
    ProductName    NVARCHAR(150) NOT NULL,
    Price          DECIMAL(18,2) NOT NULL,
    Quantity       INT NOT NULL,
    SubTotal       DECIMAL(18,2) NOT NULL,             -- Thành tiền = Price * Quantity
    CONSTRAINT FK_OrderDetails_Orders_OrderId FOREIGN KEY (OrderId)
        REFERENCES Orders (OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products_ProductId FOREIGN KEY (ProductId)
        REFERENCES Products (ProductId) ON DELETE NO ACTION,   -- Restrict: không xóa món đã có trong đơn
    CONSTRAINT CK_OrderDetails_Quantity CHECK (Quantity BETWEEN 1 AND 1000)
);
CREATE INDEX IX_OrderDetails_OrderId ON OrderDetails (OrderId);
CREATE INDEX IX_OrderDetails_ProductId ON OrderDetails (ProductId);
GO

/* =====================================================================
   DỮ LIỆU MẪU
   ===================================================================== */
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (CategoryId, CategoryName, Description, ImageUrl, DisplayOrder, Status) VALUES
(1, N'Burger',               N'Burger bò, gà, phô mai chuẩn vị Mỹ',                 N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&q=80', 1, 1),
(2, N'Gà Rán',               N'Gà rán giòn rụm, vàng ươm, cay và không cay',        N'https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=400&q=80', 2, 1),
(3, N'Pizza',                N'Pizza phô mai kéo sợi, hải sản, xúc xích',           N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&q=80', 3, 1),
(4, N'Khoai Tây & Ăn Kèm',   N'Khoai tây chiên giòn, phô mai que, xúc xích',        N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=400&q=80', 4, 1),
(5, N'Đồ Uống',              N'Coca Cola, Pepsi, Trà đào, Trà sữa mát lạnh',        N'https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=400&q=80', 5, 1),
(6, N'Combo Siêu Tiết Kiệm', N'Combo gia đình, combo cặp đôi đầy đủ món và nước',   N'https://images.unsplash.com/photo-1551782450-a2132b4ba21d?w=400&q=80',    6, 1);
SET IDENTITY_INSERT Categories OFF;
GO

SET IDENTITY_INSERT Products ON;
INSERT INTO Products (ProductId, ProductName, Description, Price, ImageUrl, Quantity, SoldQuantity, CategoryId, Status, CreatedAt) VALUES
(1,  N'Double Beef Burger',          N'2 miếng bò Úc nướng lửa hồng, phô mai Cheddar béo ngậy, sốt tiêu đen đặc biệt và rau xà lách tươi xanh.', 59000,  N'https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&q=80', 45,  120, 1, 1, DATEADD(DAY, -15, GETDATE())),
(2,  N'Burger Bò Phô Mai Nấm',       N'Burger bò thượng hạng kèm nấm xào bơ tỏi thơm lừng và sốt nấm Truffle.',                                  65000,  N'https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=600&q=80',    30,  85,  1, 1, DATEADD(DAY, -12, GETDATE())),
(3,  N'Gà Rán Giòn Cay (2 miếng)',   N'Gà tươi 100% tẩm ướp gia vị cay độc quyền, chiên vàng giòn rụm bên ngoài mọng nước bên trong.',           45000,  N'https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=600&q=80', 60,  210, 2, 1, DATEADD(DAY, -20, GETDATE())),
(4,  N'Cánh Gà Sốt BBQ (4 miếng)',   N'Cánh gà chiên giòn phủ đẫm sốt BBQ chua ngọt kiểu Mỹ thơm nức mũi.',                                       55000,  N'https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=600&q=80', 25,  64,  2, 1, DATEADD(DAY, -10, GETDATE())),
(5,  N'Pizza Hải Sản Phô Mai',       N'Đế bánh mỏng giòn, tôm mực tươi ngon, phủ phô mai Mozzarella kéo sợi béo ngậy.',                          89000,  N'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=600&q=80', 20,  52,  3, 1, DATEADD(DAY, -8,  GETDATE())),
(6,  N'Khoai Tây Chiên Lắc Phô Mai', N'Khoai tây chiên vàng giòn rụm lắc cùng bột phô mai mặn ngọt đậm đà khó cưỡng.',                           29000,  N'https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&q=80', 50,  320, 4, 1, DATEADD(DAY, -25, GETDATE())),
(7,  N'Coca Cola Tươi Size L',       N'Coca Cola tươi mát lạnh sảng khoái cực đã khi dùng kèm đồ chiên rán.',                                    20000,  N'https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=600&q=80', 100, 540, 5, 1, DATEADD(DAY, -30, GETDATE())),
(8,  N'Combo Solo No Nê',            N'Gồm 1 Double Beef Burger + 1 Khoai tây chiên + 1 Coca Cola mát lạnh. Tiết kiệm 20.000đ.',                89000,  N'https://images.unsplash.com/photo-1585032226651-759b368d7246?w=600&q=80', 35,  180, 6, 1, DATEADD(DAY, -14, GETDATE())),
(9,  N'Combo Gia Đình Thịnh Soạn',   N'Gồm 2 Burger bò phô mai, 4 miếng gà rán cay, 2 phần khoai lắc và 4 ly nước ngọt.',                       199000, N'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=600&q=80', 15,  45,  6, 1, DATEADD(DAY, -7,  GETDATE())),
(10, N'Pizza Pepperoni Xúc Xích Ý',  N'Pizza xúc xích bò cay truyền thống Ý kết hợp sốt cà chua tươi hảo hạng.',                                 79000,  N'https://images.unsplash.com/photo-1628840042765-356cda07504e?w=600&q=80', 0,   95,  3, 1, DATEADD(DAY, -5,  GETDATE())), -- hết hàng
(11, N'Mực Chiên Giòn Calamari',     N'Khoanh mực tươi tẩm bột chiên phồng chấm kèm sốt Tartar chua thanh dịu ngọt.',                            49000,  N'https://images.unsplash.com/photo-1599488615731-7e5c2823ff28?w=600&q=80', 0,   32,  4, 1, DATEADD(DAY, -4,  GETDATE())); -- hết hàng
SET IDENTITY_INSERT Products OFF;
GO

SET IDENTITY_INSERT Users ON;
INSERT INTO Users (UserId, Username, Password, FullName, Email, Phone, Address, Role, CreatedAt, Status) VALUES
(1, N'admin',     N'123', N'Quản Trị Hệ Thống', N'admin@fastfood.vn',    N'0988888888', N'123 Nguyễn Trãi, Quận 1, TP.HCM',        N'Admin',    DATEADD(DAY, -30, GETDATE()), 1),
(2, N'khachhang', N'123', N'Nguyễn Văn A',      N'nguyenvana@gmail.com', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM',          N'Customer', DATEADD(DAY, -20, GETDATE()), 1),
(3, N'tranb',     N'123', N'Trần Thị B',        N'tranthib@gmail.com',   N'0912345678', N'789 Điện Biên Phủ, Bình Thạnh, TP.HCM',  N'Customer', DATEADD(DAY, -10, GETDATE()), 1);
SET IDENTITY_INSERT Users OFF;
GO

SET IDENTITY_INSERT Orders ON;
INSERT INTO Orders (OrderId, OrderCode, UserId, CustomerName, Phone, Address, Note, TotalAmount, PaymentMethod, Status, OrderDate) VALUES
(1, N'FF-20261108-001', 2, N'Nguyễn Văn A', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM',         N'Giao giờ hành chính, không bấm chuông', 149000, N'COD',   N'Đã giao',       DATEADD(DAY,  -2, GETDATE())),
(2, N'FF-20261108-002', 2, N'Nguyễn Văn A', N'0901234567', N'456 Lê Văn Sỹ, Quận 3, TP.HCM',         N'Cho thêm nhiều tương cà và tương ớt',   109000, N'COD',   N'Đang chuẩn bị', DATEADD(HOUR, -1, GETDATE())),
(3, N'FF-20261108-003', 3, N'Trần Thị B',   N'0912345678', N'789 Điện Biên Phủ, Bình Thạnh, TP.HCM', N'Giao trước 18h',                        199000, N'VNPAY', N'Đang giao',     DATEADD(HOUR, -3, GETDATE()));
SET IDENTITY_INSERT Orders OFF;
GO

SET IDENTITY_INSERT OrderDetails ON;
INSERT INTO OrderDetails (OrderDetailId, OrderId, ProductId, ProductName, Price, Quantity, SubTotal) VALUES
(1, 1, 1, N'Double Beef Burger',        59000,  1, 59000),
(2, 1, 3, N'Gà Rán Giòn Cay (2 miếng)', 45000,  2, 90000),
(3, 2, 8, N'Combo Solo No Nê',          89000,  1, 89000),
(4, 2, 7, N'Coca Cola Tươi Size L',     20000,  1, 20000),
(5, 3, 9, N'Combo Gia Đình Thịnh Soạn', 199000, 1, 199000);
SET IDENTITY_INSERT OrderDetails OFF;
GO

/* Kiểm tra nhanh */
SELECT N'Categories' AS Bang, COUNT(*) AS SoDong FROM Categories UNION ALL
SELECT N'Products',   COUNT(*) FROM Products   UNION ALL
SELECT N'Users',      COUNT(*) FROM Users      UNION ALL
SELECT N'Orders',     COUNT(*) FROM Orders     UNION ALL
SELECT N'OrderDetails', COUNT(*) FROM OrderDetails;
GO
