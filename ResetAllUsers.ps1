# Script PowerShell để xóa TẤT CẢ users và tạo lại admin mới
# Chạy: .\ResetAllUsers.ps1

Write-Host "=== XÓA TẤT CẢ TÀI KHOẢN VÀ TẠO LẠI ADMIN ===" -ForegroundColor Red
Write-Host ""
Write-Host "CẢNH BÁO: Script này sẽ:" -ForegroundColor Yellow
Write-Host "  - Xóa TẤT CẢ tài khoản users (kể cả admin hiện tại)" -ForegroundColor Yellow
Write-Host "  - Xóa TẤT CẢ dữ liệu học sinh (progress, results, notifications)" -ForegroundColor Yellow
Write-Host "  - GIỮ NGUYÊN dữ liệu bài học (Lessons, Exercises, Questions)" -ForegroundColor Green
Write-Host "  - Tạo lại 1 tài khoản admin mới" -ForegroundColor Green
Write-Host ""

$confirm = Read-Host "Bạn có chắc chắn muốn tiếp tục? (YES để xác nhận)"

if ($confirm -ne "YES") {
    Write-Host "`nĐã hủy bỏ. Không có thay đổi nào được thực hiện." -ForegroundColor Cyan
    exit
}

# Đường dẫn đến database
$dbPath = "D:\MathUniverse\MathUniverse.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "Không tìm thấy database: $dbPath" -ForegroundColor Red
    exit
}

# Kiểm tra xem có sqlite3 không
$sqlite = "sqlite3"
try {
    $null = & $sqlite -version 2>&1
} catch {
    Write-Host "Cần cài đặt SQLite3. Tải tại: https://www.sqlite.org/download.html" -ForegroundColor Red
    exit
}

Write-Host "`n1. Backup database..." -ForegroundColor Yellow
$backupPath = "D:\MathUniverse\MathUniverse_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').db"
Copy-Item $dbPath $backupPath
Write-Host "  ✓ Đã backup tại: $backupPath" -ForegroundColor Green

Write-Host "`n2. Xóa tất cả users và dữ liệu liên quan..." -ForegroundColor Yellow

# Xóa theo thứ tự để tránh foreign key constraint
Write-Host "  - Xóa EssayAnswers..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM EssayAnswers;"

Write-Host "  - Xóa ExerciseResults..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM ExerciseResults;"

Write-Host "  - Xóa StudentProgress..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM StudentProgress;"

Write-Host "  - Xóa Notifications..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM Notifications;"

Write-Host "  - Xóa Students..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM Students;"

Write-Host "  - Xóa Admins..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM Admins;"

Write-Host "  - Xóa ActivityLogs..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM ActivityLogs;"

Write-Host "  - Xóa GameContents (dữ liệu trò chơi)..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM GameContents;"

Write-Host "  - Xóa AspNetUserRoles..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM AspNetUserRoles;"

Write-Host "  - Xóa AspNetUserTokens..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM AspNetUserTokens;"

Write-Host "  - Xóa AspNetUserLogins..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM AspNetUserLogins;"

Write-Host "  - Xóa AspNetUserClaims..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM AspNetUserClaims;"

Write-Host "  - Xóa AspNetUsers..." -ForegroundColor Gray
& $sqlite $dbPath "DELETE FROM AspNetUsers;"

Write-Host "  ✓ Đã xóa tất cả users!" -ForegroundColor Green

Write-Host "`n3. Tạo tài khoản admin mới..." -ForegroundColor Yellow

# Tạo admin user mới
$createUserSql = @"
INSERT INTO AspNetUsers (UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, CreatedDate)
VALUES (
    'admin@mathuniverse.com',
    'ADMIN@MATHUNIVERSE.COM',
    'admin@mathuniverse.com',
    'ADMIN@MATHUNIVERSE.COM',
    1,
    'AQAAAAIAAYagAAAAEDqKzKqI+7rBmNnTCO6E3VJVvYxLGCnU4jLXGC8gXqhHMWcL7+pX8FQxJMVHqYYcwQ==',
    'WXYZ1234ABCD5678EFGH9012IJKL3456',
    'abcd1234-5678-90ab-cdef-1234567890ab',
    0,
    0,
    1,
    0,
    'Administrator',
    datetime('now')
);
"@

& $sqlite $dbPath $createUserSql
Write-Host "  ✓ Đã tạo user admin" -ForegroundColor Green

# Tạo Admin profile
$createAdminProfileSql = @"
INSERT INTO Admins (UserId, Department, CreatedDate)
SELECT Id, 'Quản trị hệ thống', datetime('now')
FROM AspNetUsers 
WHERE Email = 'admin@mathuniverse.com';
"@

& $sqlite $dbPath $createAdminProfileSql
Write-Host "  ✓ Đã tạo Admin profile" -ForegroundColor Green

# Gán role Admin
$assignRoleSql = @"
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, 1
FROM AspNetUsers 
WHERE Email = 'admin@mathuniverse.com';
"@

& $sqlite $dbPath $assignRoleSql
Write-Host "  ✓ Đã gán vai trò Admin" -ForegroundColor Green

Write-Host "`n4. Kiểm tra kết quả..." -ForegroundColor Yellow

$userCount = & $sqlite $dbPath "SELECT COUNT(*) FROM AspNetUsers;"
Write-Host "  - Số lượng users: $userCount" -ForegroundColor Cyan

$lessonCount = & $sqlite $dbPath "SELECT COUNT(*) FROM Lessons;"
Write-Host "  - Số lượng bài học: $lessonCount" -ForegroundColor Cyan

$exerciseCount = & $sqlite $dbPath "SELECT COUNT(*) FROM Exercises;"
Write-Host "  - Số lượng bài tập: $exerciseCount" -ForegroundColor Cyan

Write-Host "`n=== HOÀN TẤT ===" -ForegroundColor Green
Write-Host ""
Write-Host "✓ Đã xóa tất cả tài khoản cũ" -ForegroundColor Green
Write-Host "✓ Đã tạo tài khoản admin mới" -ForegroundColor Green
Write-Host "✓ Dữ liệu bài học được giữ nguyên" -ForegroundColor Green
Write-Host ""
Write-Host "THÔNG TIN ĐĂNG NHẬP MỚI:" -ForegroundColor Cyan
Write-Host "  Email: admin@mathuniverse.com" -ForegroundColor White
Write-Host "  Password: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "Vui lòng khởi động lại ứng dụng: dotnet run" -ForegroundColor Yellow

