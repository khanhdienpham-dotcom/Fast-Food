using FastFood.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Thông báo lỗi model binding bằng tiếng Việt (thay cho "The value '' is invalid." mặc định)
builder.Services.AddControllersWithViews(options =>
{
    var m = options.ModelBindingMessageProvider;
    m.SetValueMustNotBeNullAccessor(_ => "Trường này là bắt buộc.");
    m.SetMissingBindRequiredValueAccessor(name => $"Vui lòng nhập {name}.");
    m.SetValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ.");
    m.SetAttemptedValueIsInvalidAccessor((v, name) => $"Giá trị '{v}' không hợp lệ cho {name}.");
    m.SetNonPropertyAttemptedValueIsInvalidAccessor(v => $"Giá trị '{v}' không hợp lệ.");
    m.SetUnknownValueIsInvalidAccessor(name => $"Giá trị nhập cho {name} không hợp lệ.");
    m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Giá trị nhập không hợp lệ.");
    m.SetValueMustBeANumberAccessor(name => $"{name} phải là số.");
    m.SetNonPropertyValueMustBeANumberAccessor(() => "Giá trị phải là số.");
});
builder.Services.AddHttpContextAccessor();

// Configure Database (Sqlite as default, or SqlServer if configured)
var dbProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
if (dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
{
    var connStr = builder.Configuration.GetConnectionString("FastFoodDbSqlServer");
    builder.Services.AddDbContext<FastFoodDbContext>(options =>
        options.UseSqlServer(connStr));
}
else
{
    var connStr = builder.Configuration.GetConnectionString("FastFoodDbSqlite") ?? "Data Source=fastfood.db";
    builder.Services.AddDbContext<FastFoodDbContext>(options =>
        options.UseSqlite(connStr));
}

// Session configuration
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed Database automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FastFoodDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi xảy ra trong quá trình khởi tạo dữ liệu mẫu (Database Seeding).");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Trang lỗi thân thiện cho 404 (gõ sai đường dẫn, /Product/Detail/99999 ...) thay vì trang trắng của trình duyệt
app.UseStatusCodePagesWithReExecute("/Home/HttpError", "?code={0}");

app.UseHttpsRedirection();

// Cố định văn hóa vi-VN cho mọi máy (định dạng tiền 59.000 đ, ngày dd/MM/yyyy),
// tránh việc website hiển thị/nhận dữ liệu khác nhau tùy cài đặt Region của Windows.
var viCulture = new System.Globalization.CultureInfo("vi-VN");
app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(viCulture),
    SupportedCultures = new[] { viCulture },
    SupportedUICultures = new[] { viCulture }
});

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Route for Areas (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Default Route (Customer)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
