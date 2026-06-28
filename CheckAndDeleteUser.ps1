# Script PowerShell để kiểm tra và xóa tài khoản abc@gmail.com
# Chạy: .\CheckAndDeleteUser.ps1

Write-Host "=== Kiểm tra và xóa tài khoản abc@gmail.com ===" -ForegroundColor Cyan
Write-Host ""

# Đường dẫn đến database
$dbPath = "D:\MathUniverse\MathUniverse.db"

if (-not (Test-Path $dbPath)) {
    Write-Host "Không tìm thấy database: $dbPath" -ForegroundColor Red
    exit
}

Write-Host "1. Kiểm tra tài khoản hiện tại..." -ForegroundColor Yellow

# Kiểm tra xem có sqlite3 không
$sqlite = "sqlite3"
try {
    $null = & $sqlite -version 2>&1
} catch {
    Write-Host "Cần cài đặt SQLite3. Tải tại: https://www.sqlite.org/download.html" -ForegroundColor Red
    Write-Host "Hoặc sử dụng cách khác trong file HUONG_DAN_XOA_HOC_SINH.md" -ForegroundColor Yellow
    exit
}

# Kiểm tra tài khoản
Write-Host "`nKiểm tra tài khoản abc@gmail.com..." -ForegroundColor Yellow
& $sqlite $dbPath "SELECT Id, Email, UserName FROM AspNetUsers WHERE Email = 'abc@gmail.com';"

$response = Read-Host "`nBạn có thấy tài khoản abc@gmail.com không? (y/n)"

if ($response -eq 'y' -or $response -eq 'Y') {
    Write-Host "`n2. Bắt đầu xóa tài khoản abc@gmail.com..." -ForegroundColor Yellow
    
    # Xóa theo thứ tự để tránh foreign key constraint
    Write-Host "  - Xóa ExerciseResults..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM ExerciseResults WHERE StudentId IN (SELECT s.StudentId FROM Students s INNER JOIN AspNetUsers u ON s.UserId = u.Id WHERE u.Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa StudentProgress..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM StudentProgress WHERE StudentId IN (SELECT s.StudentId FROM Students s INNER JOIN AspNetUsers u ON s.UserId = u.Id WHERE u.Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa Notifications..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM Notifications WHERE StudentId IN (SELECT s.StudentId FROM Students s INNER JOIN AspNetUsers u ON s.UserId = u.Id WHERE u.Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa Students..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM Students WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa ActivityLogs..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM ActivityLogs WHERE UserId IN (SELECT CAST(Id AS TEXT) FROM AspNetUsers WHERE Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa AspNetUserRoles..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM AspNetUserRoles WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = 'abc@gmail.com');"
    
    Write-Host "  - Xóa AspNetUsers..." -ForegroundColor Gray
    & $sqlite $dbPath "DELETE FROM AspNetUsers WHERE Email = 'abc@gmail.com';"
    
    Write-Host "`n3. Kiểm tra lại..." -ForegroundColor Yellow
    $result = & $sqlite $dbPath "SELECT COUNT(*) FROM AspNetUsers WHERE Email = 'abc@gmail.com';"
    
    if ($result -eq "0") {
        Write-Host "`n✓ ĐÃ XÓA THÀNH CÔNG tài khoản abc@gmail.com!" -ForegroundColor Green
        Write-Host "Vui lòng khởi động lại ứng dụng và hard refresh trình duyệt (Ctrl+Shift+R)" -ForegroundColor Cyan
    } else {
        Write-Host "`n✗ Có lỗi xảy ra. Tài khoản vẫn còn trong database." -ForegroundColor Red
    }
} else {
    Write-Host "`n✓ Tài khoản đã được xóa khỏi database!" -ForegroundColor Green
    Write-Host "Vấn đề có thể do cache. Hãy:" -ForegroundColor Yellow
    Write-Host "  1. Dừng ứng dụng web (Ctrl+C)" -ForegroundColor Gray
    Write-Host "  2. Hard refresh trình duyệt (Ctrl+Shift+R)" -ForegroundColor Gray
    Write-Host "  3. Khởi động lại ứng dụng: dotnet run" -ForegroundColor Gray
}

Write-Host "`n=== Hoàn tất ===" -ForegroundColor Cyan

