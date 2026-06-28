# Script PowerShell để xóa TẤT CẢ dữ liệu GameContents
# Chạy: .\DeleteAllGameContents.ps1

Write-Host "=== XÓA TẤT CẢ DỮ LIỆU TRÒ CHƠI ===" -ForegroundColor Red
Write-Host ""
Write-Host "CẢNH BÁO: Script này sẽ:" -ForegroundColor Yellow
Write-Host "  - Xóa TẤT CẢ dữ liệu GameContents (thẻ trò chơi)" -ForegroundColor Yellow
Write-Host "  - Admin sẽ phải tự tạo lại thẻ trò chơi cho từng bài học" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Bạn có chắc chắn muốn xóa tất cả dữ liệu trò chơi? (YES để xác nhận)"

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

Write-Host "`n2. Kiểm tra dữ liệu hiện tại..." -ForegroundColor Yellow
$currentCount = & $sqlite $dbPath "SELECT COUNT(*) FROM GameContents;"
Write-Host "  - Số lượng GameContents hiện tại: $currentCount" -ForegroundColor Cyan

if ($currentCount -eq "0") {
    Write-Host "`n  ✓ Không có dữ liệu GameContents nào để xóa!" -ForegroundColor Green
    exit
}

Write-Host "`n3. Xóa tất cả GameContents..." -ForegroundColor Yellow
& $sqlite $dbPath "DELETE FROM GameContents;"

Write-Host "`n4. Kiểm tra kết quả..." -ForegroundColor Yellow
$afterCount = & $sqlite $dbPath "SELECT COUNT(*) FROM GameContents;"
Write-Host "  - Số lượng GameContents sau khi xóa: $afterCount" -ForegroundColor Cyan

if ($afterCount -eq "0") {
    Write-Host "`n=== HOÀN TẤT ===" -ForegroundColor Green
    Write-Host ""
    Write-Host "✓ Đã xóa tất cả dữ liệu trò chơi!" -ForegroundColor Green
    Write-Host "✓ Admin có thể tạo thẻ trò chơi mới cho từng bài học" -ForegroundColor Green
    Write-Host ""
    Write-Host "Hướng dẫn tạo thẻ trò chơi:" -ForegroundColor Cyan
    Write-Host "  1. Đăng nhập admin" -ForegroundColor White
    Write-Host "  2. Vào 'Quản lý bài giảng'" -ForegroundColor White
    Write-Host "  3. Nhấn nút 'Trò chơi' (gamepad icon) ở mỗi bài học" -ForegroundColor White
    Write-Host "  4. Tạo các cặp thẻ (VD: '1 dm' <-> '10 cm')" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "`n✗ Có lỗi xảy ra. Vẫn còn $afterCount GameContents!" -ForegroundColor Red
}

