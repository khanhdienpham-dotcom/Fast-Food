using Microsoft.EntityFrameworkCore;

namespace FastFood.Models
{
    public static class DbInitializer
    {
        public static void Initialize(FastFoodDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if already seeded
            if (context.Categories.Any())
            {
                return; // DB has been seeded
            }

            // 1. Seed Categories
            var categories = new List<Category>
            {
                new Category { CategoryName = "Burger", Description = "Burger bò, gà, phô mai chuẩn vị Mỹ", ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=400&q=80", DisplayOrder = 1, Status = true },
                new Category { CategoryName = "Gà Rán", Description = "Gà rán giòn rụm, vàng ươm, cay và không cay", ImageUrl = "https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=400&q=80", DisplayOrder = 2, Status = true },
                new Category { CategoryName = "Pizza", Description = "Pizza phô mai kéo sợi, hải sản, xúc xích", ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=400&q=80", DisplayOrder = 3, Status = true },
                new Category { CategoryName = "Khoai Tây & Ăn Kèm", Description = "Khoai tây chiên giòn, phô mai que, xúc xích", ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=400&q=80", DisplayOrder = 4, Status = true },
                new Category { CategoryName = "Đồ Uống", Description = "Coca Cola, Pepsi, Trà đào, Trà sữa mát lạnh", ImageUrl = "https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=400&q=80", DisplayOrder = 5, Status = true },
                new Category { CategoryName = "Combo Siêu Tiết Kiệm", Description = "Combo gia đình, combo cặp đôi đầy đủ món và nước", ImageUrl = "https://images.unsplash.com/photo-1551782450-a2132b4ba21d?w=400&q=80", DisplayOrder = 6, Status = true }
            };
            context.Categories.AddRange(categories);
            context.SaveChanges();

            // 2. Seed Products
            var products = new List<Product>
            {
                new Product
                {
                    ProductName = "Double Beef Burger",
                    Description = "2 miếng bò Úc nướng lửa hồng, phô mai Cheddar béo ngậy, sốt tiêu đen đặc biệt và rau xà lách tươi xanh.",
                    Price = 59000,
                    ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&q=80",
                    Quantity = 45,
                    SoldQuantity = 120,
                    CategoryId = categories[0].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-15)
                },
                new Product
                {
                    ProductName = "Burger Bò Phô Mai Nấm",
                    Description = "Burger bò thượng hạng kèm nấm xào bơ tỏi thơm lừng và sốt nấm Truffle.",
                    Price = 65000,
                    ImageUrl = "https://images.unsplash.com/photo-1553979459-d2229ba7433b?w=600&q=80",
                    Quantity = 30,
                    SoldQuantity = 85,
                    CategoryId = categories[0].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-12)
                },
                new Product
                {
                    ProductName = "Gà Rán Giòn Cay (2 miếng)",
                    Description = "Gà tươi 100% tẩm ướp gia vị cay độc quyền, chiên vàng giòn rụm bên ngoài mọng nước bên trong.",
                    Price = 45000,
                    ImageUrl = "https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=600&q=80",
                    Quantity = 60,
                    SoldQuantity = 210,
                    CategoryId = categories[1].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new Product
                {
                    ProductName = "Cánh Gà Sốt BBQ (4 miếng)",
                    Description = "Cánh gà chiên giòn phủ đẫm sốt BBQ chua ngọt kiểu Mỹ thơm nức mũi.",
                    Price = 55000,
                    ImageUrl = "https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=600&q=80",
                    Quantity = 25,
                    SoldQuantity = 64,
                    CategoryId = categories[1].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Product
                {
                    ProductName = "Pizza Hải Sản Phô Mai",
                    Description = "Đế bánh mỏng giòn, tôm mực tươi ngon, phủ phô mai Mozzarella kéo sợi béo ngậy.",
                    Price = 89000,
                    ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=600&q=80",
                    Quantity = 20,
                    SoldQuantity = 52,
                    CategoryId = categories[2].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-8)
                },
                new Product
                {
                    ProductName = "Khoai Tây Chiên Lắc Phô Mai",
                    Description = "Khoai tây chiên vàng giòn rụm lắc cùng bột phô mai mặn ngọt đậm đà khó cưỡng.",
                    Price = 29000,
                    ImageUrl = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&q=80",
                    Quantity = 50,
                    SoldQuantity = 320,
                    CategoryId = categories[3].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-25)
                },
                new Product
                {
                    ProductName = "Coca Cola Tươi Size L",
                    Description = "Coca Cola tươi mát lạnh sảng khoái cực đã khi dùng kèm đồ chiên rán.",
                    Price = 20000,
                    ImageUrl = "https://images.unsplash.com/photo-1625772299848-391b6a87d7b3?w=600&q=80",
                    Quantity = 100,
                    SoldQuantity = 540,
                    CategoryId = categories[4].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new Product
                {
                    ProductName = "Combo Solo No Nê",
                    Description = "Gồm 1 Double Beef Burger + 1 Khoai tây chiên + 1 Coca Cola mát lạnh. Tiết kiệm 20.000đ.",
                    Price = 89000,
                    ImageUrl = "https://images.unsplash.com/photo-1585032226651-759b368d7246?w=600&q=80",
                    Quantity = 35,
                    SoldQuantity = 180,
                    CategoryId = categories[5].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-14)
                },
                new Product
                {
                    ProductName = "Combo Gia Đình Thịnh Soạn",
                    Description = "Gồm 2 Burger bò phô mai, 4 miếng gà rán cay, 2 phần khoai lắc và 4 ly nước ngọt.",
                    Price = 199000,
                    ImageUrl = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=600&q=80",
                    Quantity = 15,
                    SoldQuantity = 45,
                    CategoryId = categories[5].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-7)
                },
                // Out of stock product for testing "Quản lý kho hàng: Danh sách hàng cần nhập (Quantity = 0)"
                new Product
                {
                    ProductName = "Pizza Pepperoni Xúc Xích Ý",
                    Description = "Pizza xúc xích bò cay truyền thống Ý kết hợp sốt cà chua tươi hảo hạng.",
                    Price = 79000,
                    ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=600&q=80",
                    Quantity = 0, // Out of stock!
                    SoldQuantity = 95,
                    CategoryId = categories[2].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    ProductName = "Mực Chiên Giòn Calamari",
                    Description = "Khoanh mực tươi tẩm bột chiên phồng chấm kèm sốt Tartar chua thanh dịu ngọt.",
                    Price = 49000,
                    ImageUrl = "https://images.unsplash.com/photo-1599488615731-7e5c2823ff28?w=600&q=80",
                    Quantity = 0, // Out of stock!
                    SoldQuantity = 32,
                    CategoryId = categories[3].CategoryId,
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-4)
                }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            // 3. Seed Users
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Password = FastFood.Helpers.PasswordHelper.Hash("123"), // mật khẩu được mã hóa PBKDF2
                    FullName = "Quản Trị Hệ Thống",
                    Email = "admin@fastfood.vn",
                    Phone = "0988888888",
                    Address = "123 Nguyễn Trãi, Quận 1, TP.HCM",
                    Role = "Admin",
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-30)
                },
                new User
                {
                    Username = "khachhang",
                    Password = FastFood.Helpers.PasswordHelper.Hash("123"),
                    FullName = "Nguyễn Văn A",
                    Email = "nguyenvana@gmail.com",
                    Phone = "0901234567",
                    Address = "456 Lê Văn Sỹ, Quận 3, TP.HCM",
                    Role = "Customer",
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-20)
                },
                new User
                {
                    Username = "tranb",
                    Password = FastFood.Helpers.PasswordHelper.Hash("123"),
                    FullName = "Trần Thị B",
                    Email = "tranthib@gmail.com",
                    Phone = "0912345678",
                    Address = "789 Điện Biên Phủ, Bình Thạnh, TP.HCM",
                    Role = "Customer",
                    Status = true,
                    CreatedAt = DateTime.Now.AddDays(-10)
                }
            };
            context.Users.AddRange(users);
            context.SaveChanges();

            // 4. Seed Sample Orders & OrderDetails
            var order1 = new Order
            {
                OrderCode = "FF-" + DateTime.Now.ToString("yyyyMMdd") + "-001",
                UserId = users[1].UserId,
                CustomerName = users[1].FullName,
                Phone = users[1].Phone,
                Address = users[1].Address ?? "456 Lê Văn Sỹ, Quận 3, TP.HCM",
                Note = "Giao giờ hành chính, không bấm chuông",
                PaymentMethod = "COD",
                Status = "Đã giao",
                OrderDate = DateTime.Now.AddDays(-2),
                TotalAmount = 149000
            };
            context.Orders.Add(order1);
            context.SaveChanges();

            var orderDetails1 = new List<OrderDetail>
            {
                new OrderDetail
                {
                    OrderId = order1.OrderId,
                    ProductId = products[0].ProductId,
                    ProductName = products[0].ProductName,
                    Price = products[0].Price,
                    Quantity = 1,
                    SubTotal = products[0].Price * 1
                },
                new OrderDetail
                {
                    OrderId = order1.OrderId,
                    ProductId = products[2].ProductId,
                    ProductName = products[2].ProductName,
                    Price = products[2].Price,
                    Quantity = 2,
                    SubTotal = products[2].Price * 2
                }
            };
            context.OrderDetails.AddRange(orderDetails1);

            var order2 = new Order
            {
                OrderCode = "FF-" + DateTime.Now.ToString("yyyyMMdd") + "-002",
                UserId = users[1].UserId,
                CustomerName = users[1].FullName,
                Phone = users[1].Phone,
                Address = users[1].Address ?? "456 Lê Văn Sỹ, Quận 3, TP.HCM",
                Note = "Cho thêm nhiều tương cà và tương ớt",
                PaymentMethod = "COD",
                Status = "Đang chuẩn bị",
                OrderDate = DateTime.Now.AddHours(-1),
                TotalAmount = 109000
            };
            context.Orders.Add(order2);
            context.SaveChanges();

            var orderDetails2 = new List<OrderDetail>
            {
                new OrderDetail
                {
                    OrderId = order2.OrderId,
                    ProductId = products[7].ProductId, // Combo Solo
                    ProductName = products[7].ProductName,
                    Price = products[7].Price,
                    Quantity = 1,
                    SubTotal = products[7].Price * 1
                },
                new OrderDetail
                {
                    OrderId = order2.OrderId,
                    ProductId = products[6].ProductId, // Coca Cola
                    ProductName = products[6].ProductName,
                    Price = products[6].Price,
                    Quantity = 1,
                    SubTotal = products[6].Price * 1
                }
            };
            context.OrderDetails.AddRange(orderDetails2);

            var order3 = new Order
            {
                OrderCode = "FF-" + DateTime.Now.ToString("yyyyMMdd") + "-003",
                UserId = users[2].UserId,
                CustomerName = users[2].FullName,
                Phone = users[2].Phone,
                Address = users[2].Address ?? "789 Điện Biên Phủ, Bình Thạnh, TP.HCM",
                Note = "Giao trước 18h",
                PaymentMethod = "VNPAY",
                Status = "Đang giao",
                OrderDate = DateTime.Now.AddHours(-3),
                TotalAmount = 199000
            };
            context.Orders.Add(order3);
            context.SaveChanges();

            var orderDetails3 = new List<OrderDetail>
            {
                new OrderDetail
                {
                    OrderId = order3.OrderId,
                    ProductId = products[8].ProductId, // Combo Gia đình
                    ProductName = products[8].ProductName,
                    Price = products[8].Price,
                    Quantity = 1,
                    SubTotal = products[8].Price * 1
                }
            };
            context.OrderDetails.AddRange(orderDetails3);

            context.SaveChanges();
        }
    }
}
