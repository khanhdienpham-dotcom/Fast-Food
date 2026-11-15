# =====================================================================
#  XUẤT DỮ LIỆU HIỆN TẠI CỦA DATABASE FastFoodDb RA FILE .SQL
#  - Đọc toàn bộ dữ liệu 5 bảng trong SQL Server LocalDB trên máy này
#  - Ghi ra Database\FastFoodDb_DuLieuHienTai.sql (thay thế dữ liệu mẫu)
#  Cách chạy: chuột phải file này -> "Run with PowerShell"
#             hoặc: powershell -ExecutionPolicy Bypass -File Database\XuatDuLieu.ps1
# =====================================================================
param(
    [string]$Server = "(localdb)\mssqllocaldb",
    [string]$Database = "FastFoodDb"
)

$ErrorActionPreference = "Stop"
$inv = [System.Globalization.CultureInfo]::InvariantCulture
$outFile = Join-Path $PSScriptRoot "FastFoodDb_DuLieuHienTai.sql"

# Thứ tự bảng: bảng cha trước, bảng con sau (theo khóa ngoại)
$tables = @("Categories", "Products", "Users", "Orders", "OrderDetails")

function Format-SqlValue($value) {
    if ($value -is [System.DBNull] -or $null -eq $value) { return "NULL" }
    if ($value -is [bool]) { if ($value) { return "1" } else { return "0" } }
    if ($value -is [datetime]) { return "'" + $value.ToString("yyyy-MM-ddTHH:mm:ss.fffffff", $inv) + "'" }
    if ($value -is [decimal] -or $value -is [double] -or $value -is [single]) { return $value.ToString($inv) }
    if ($value -is [int] -or $value -is [long] -or $value -is [int16] -or $value -is [byte]) { return $value.ToString($inv) }
    return "N'" + ([string]$value).Replace("'", "''") + "'"
}

$conn = New-Object System.Data.SqlClient.SqlConnection("Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True")
$conn.Open()

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("/* =====================================================================")
[void]$sb.AppendLine("   FASTFOOD STORE - DỮ LIỆU HIỆN TẠI (xuất lúc " + (Get-Date).ToString("dd/MM/yyyy HH:mm") + ")")
[void]$sb.AppendLine("   Cách dùng (SSMS hoặc SQL Server Object Explorer trong Visual Studio):")
[void]$sb.AppendLine("     1. Chạy FastFoodDb.sql trước (tạo database + bảng)")
[void]$sb.AppendLine("     2. Chạy file này: xóa dữ liệu mẫu, nạp toàn bộ dữ liệu hiện tại")
[void]$sb.AppendLine("   Mật khẩu trong bảng Users đã được mã hóa (PBKDF2), không phải mật khẩu gốc.")
[void]$sb.AppendLine("   ===================================================================== */")
[void]$sb.AppendLine("USE FastFoodDb;")
[void]$sb.AppendLine("GO")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("/* Xóa dữ liệu cũ: bảng con trước, bảng cha sau */")
for ($i = $tables.Count - 1; $i -ge 0; $i--) { [void]$sb.AppendLine("DELETE FROM " + $tables[$i] + ";") }
[void]$sb.AppendLine("GO")

$summary = @()
foreach ($t in $tables) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT * FROM [$t] ORDER BY 1"
    $reader = $cmd.ExecuteReader()
    $cols = @()
    for ($c = 0; $c -lt $reader.FieldCount; $c++) { $cols += $reader.GetName($c) }
    $colList = ($cols | ForEach-Object { "[$_]" }) -join ", "

    [void]$sb.AppendLine("")
    [void]$sb.AppendLine("/* ---------- $t ---------- */")
    [void]$sb.AppendLine("SET IDENTITY_INSERT [$t] ON;")
    $count = 0
    while ($reader.Read()) {
        $vals = @()
        for ($c = 0; $c -lt $reader.FieldCount; $c++) { $vals += (Format-SqlValue $reader.GetValue($c)) }
        [void]$sb.AppendLine("INSERT INTO [$t] ($colList) VALUES (" + ($vals -join ", ") + ");")
        $count++
    }
    $reader.Close()
    [void]$sb.AppendLine("SET IDENTITY_INSERT [$t] OFF;")
    # Cho cột tự tăng tiếp tục đúng số sau dòng lớn nhất
    [void]$sb.AppendLine("DECLARE @max_$t INT = ISNULL((SELECT MAX(" + $cols[0] + ") FROM [$t]), 0);")
    [void]$sb.AppendLine("DBCC CHECKIDENT ('$t', RESEED, @max_$t);")
    [void]$sb.AppendLine("GO")
    $summary += "$t : $count dong"
}
$conn.Close()

[System.IO.File]::WriteAllText($outFile, $sb.ToString(), (New-Object System.Text.UTF8Encoding($true)))

Write-Host ""
Write-Host "DA XUAT XONG -> $outFile" -ForegroundColor Green
$summary | ForEach-Object { Write-Host "  $_" }
