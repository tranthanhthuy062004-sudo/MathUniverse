# 📖 Hướng dẫn cài đặt và chạy dự án MathUniverse

## 🎯 Mục tiêu
Hướng dẫn chi tiết từng bước để cài đặt và chạy dự án **Vũ trụ Toán học** trên máy tính mới với Visual Studio 2022 Community.

---

## 📋 Yêu cầu hệ thống

### Phần mềm cần thiết:
1. **Windows 10/11** (64-bit)
2. **Visual Studio 2022 Community** (miễn phí)
3. **.NET 8.0 SDK**
4. **SQL Server** (LocalDB hoặc SQL Server Express)
5. Kết nối Internet (để tải packages)

---

## 🚀 BƯỚC 1: Cài đặt Visual Studio 2022 Community

### 1.1. Tải Visual Studio 2022 Community
1. Truy cập: https://visualstudio.microsoft.com/vs/community/
2. Click nút **"Download Visual Studio"**
3. Tải file cài đặt (khoảng 3-4 MB)

### 1.2. Cài đặt Visual Studio
1. Chạy file cài đặt vừa tải (`VisualStudioSetup.exe`)
2. Khi cửa sổ **Visual Studio Installer** mở ra, chọn các workloads sau:

   ✅ **ASP.NET and web development** (BẮT BUỘC)
   - Đây là workload chính để phát triển web ASP.NET Core
   - Bao gồm cả .NET SDK, IIS Express, và các công cụ cần thiết

   ✅ **Data storage and processing** (BẮT BUỘC)
   - Bao gồm SQL Server Express LocalDB
   - Entity Framework Core tools

3. Trong tab **Individual components**, đảm bảo các component sau được chọn:
   - ✅ .NET 8.0 Runtime (Long-term support)
   - ✅ .NET SDK
   - ✅ SQL Server Express 2019 LocalDB
   - ✅ Entity Framework 8 tools

4. Click **"Install"** và đợi quá trình cài đặt hoàn tất (30-60 phút tùy tốc độ mạng)

5. Sau khi cài đặt xong, click **"Launch"** để mở Visual Studio

---

## 🚀 BƯỚC 2: Giải nén và mở dự án

### 2.1. Giải nén source code
1. Copy toàn bộ folder dự án (ví dụ: `MathUniverse.zip` hoặc folder `MathUniverse`)
2. Giải nén vào vị trí mong muốn, ví dụ:
   ```
   D:\Projects\MathUniverse\
   ```
   hoặc
   ```
   C:\Users\YourName\source\repos\MathUniverse\
   ```

### 2.2. Mở dự án trong Visual Studio
1. Mở **Visual Studio 2022**
2. Click **"Open a project or solution"**
3. Duyệt đến folder dự án và chọn file:
   ```
   MathUniverse.sln
   ```
4. Click **"Open"**

---

## 🚀 BƯỚC 3: Restore NuGet Packages

### 3.1. Tự động restore (Khuyến nghị)
Khi mở solution, Visual Studio sẽ tự động restore packages. Quan sát ở **Output Window** (View → Output):

```
Restoring NuGet packages...
Restore completed in 45.2 sec for MathUniverse.csproj
```

### 3.2. Restore thủ công (nếu cần)
Nếu không tự động restore:

1. Click chuột phải vào **Solution 'MathUniverse'** trong Solution Explorer
2. Chọn **"Restore NuGet Packages"**

Hoặc dùng Package Manager Console:
1. Menu: **Tools → NuGet Package Manager → Package Manager Console**
2. Chạy lệnh:
   ```powershell
   dotnet restore
   ```

---

## 🚀 BƯỚC 4: Cấu hình Database

### 4.1. Kiểm tra SQL Server LocalDB

Mở **Command Prompt** hoặc **PowerShell** và chạy:

```cmd
sqllocaldb info
```

Nếu thấy `MSSQLLocalDB` trong danh sách → OK!

Nếu không thấy, chạy:
```cmd
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### 4.2. Kiểm tra Connection String

Mở file `appsettings.json` và kiểm tra:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MathUniverse;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Lưu ý:** 
- Nếu dùng SQL Server Express thay vì LocalDB, đổi connection string thành:
  ```
  Server=localhost\\SQLEXPRESS;Database=MathUniverse;Trusted_Connection=true;
  ```

### 4.3. Tạo Database và chạy Migrations

#### Option 1: Dùng Package Manager Console (Khuyến nghị)

1. Mở **Package Manager Console**: 
   - Menu: **Tools → NuGet Package Manager → Package Manager Console**

2. Chạy lần lượt các lệnh:

   ```powershell
   # Kiểm tra migrations hiện có
   Get-Migration
   
   # Tạo database và chạy migrations
   Update-Database
   ```

3. Nếu thành công, bạn sẽ thấy:
   ```
   Applying migration '20260102174138_InitialCreate'.
   Done.
   ```

#### Option 2: Dùng .NET CLI

Mở **Terminal** trong Visual Studio (View → Terminal) hoặc cmd:

```bash
# Di chuyển vào folder dự án
cd D:\Projects\MathUniverse

# Kiểm tra migrations
dotnet ef migrations list

# Chạy migrations để tạo database
dotnet ef database update
```

### 4.4. Kiểm tra Database đã tạo thành công

1. Mở **SQL Server Object Explorer** trong Visual Studio:
   - Menu: **View → SQL Server Object Explorer**

2. Mở node:
   ```
   SQL Server → (localdb)\MSSQLLocalDB → Databases → MathUniverse
   ```

3. Bạn sẽ thấy các bảng:
   - AspNetUsers
   - AspNetRoles
   - Students
   - Admins
   - Lessons
   - Exercises
   - Questions
   - ...

---

## 🚀 BƯỚC 5: Seed Data (Tạo dữ liệu mẫu)

### 5.1. Kiểm tra DbInitializer

Dự án đã có sẵn `DbInitializer.cs` để tạo:
- ✅ Tài khoản Admin mặc định
- ✅ Roles (Admin, Student, Guest)

### 5.2. Seed data tự động khi chạy

File `Program.cs` đã được cấu hình để tự động seed data khi khởi động lần đầu:

```csharp
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        DbInitializer.Initialize(scope.ServiceProvider).Wait();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
```

**Tài khoản Admin mặc định:**
- Email: `admin@mathuniverse.com`
- Password: `Admin@123`

---

## 🚀 BƯỚC 6: Chạy dự án

### 6.1. Build dự án

1. Trong Visual Studio, click menu: **Build → Build Solution** (hoặc Ctrl+Shift+B)
2. Kiểm tra **Output Window** để đảm bảo build thành công:
   ```
   Build succeeded.
       0 Warning(s)
       0 Error(s)
   ```

### 6.2. Chạy dự án

**Option 1: Chạy với IIS Express (Khuyến nghị cho lần đầu)**

1. Đảm bảo ở thanh toolbar có dropdown hiển thị: **"IIS Express"**
2. Click nút **Play (▶)** màu xanh lá (hoặc F5)
3. Trình duyệt sẽ tự động mở với URL:
   ```
   https://localhost:7xxx (port ngẫu nhiên)
   hoặc
   http://localhost:5xxx
   ```

**Option 2: Chạy với .NET CLI**

Mở Terminal và chạy:
```bash
cd D:\Projects\MathUniverse
dotnet run
```

Sau đó mở trình duyệt và truy cập:
```
https://localhost:7000
hoặc
http://localhost:5000
```

### 6.3. Kiểm tra web đã chạy thành công

Khi web mở, bạn sẽ thấy:
- ✅ Trang chủ "Vũ trụ Toán học" với Hero section đẹp mắt
- ✅ Background gradient với stars, nebula
- ✅ Feature cards với gradient
- ✅ Nút "Đăng ký miễn phí" và "Đăng nhập"

---

## 🚀 BƯỚC 7: Đăng nhập và kiểm tra chức năng

### 7.1. Đăng nhập Admin

1. Click **"Đăng nhập"** ở góc trên bên phải
2. Nhập thông tin:
   - Email: `admin@mathuniverse.com`
   - Password: `Admin@123`
3. Click **"Đăng nhập"**
4. Bạn sẽ được chuyển đến **Admin Dashboard**

### 7.2. Đăng ký tài khoản Student (test)

1. Click **"Đăng ký miễn phí"**
2. Điền form đăng ký:
   - Họ tên: `Nguyễn Văn A`
   - Lớp: `3`
   - Ngày sinh: `01/01/2014`
   - Email phụ huynh: `parent@example.com`
   - Mật khẩu: `Student@123`
3. Click **"Đăng ký"**
4. Hệ thống tự động tạo mã học sinh (ví dụ: `26004001`)
5. Đăng nhập bằng email: `parent@example.com` và password: `Student@123`

---

## 🔧 XỬ LÝ LỖI THƯỜNG GẶP

### ❌ Lỗi: "The type or namespace name 'Microsoft' could not be found"

**Nguyên nhân:** NuGet packages chưa được restore

**Giải pháp:**
```powershell
# Trong Package Manager Console
dotnet restore

# Hoặc
Update-Package -reinstall
```

### ❌ Lỗi: "A network-related or instance-specific error occurred"

**Nguyên nhân:** SQL Server LocalDB chưa chạy hoặc connection string sai

**Giải pháp:**
```cmd
# Khởi động LocalDB
sqllocaldb start MSSQLLocalDB

# Kiểm tra trạng thái
sqllocaldb info MSSQLLocalDB
```

### ❌ Lỗi: "Unable to resolve service for type 'ApplicationDbContext'"

**Nguyên nhân:** Dependency Injection chưa được cấu hình

**Giải pháp:** Kiểm tra `Program.cs` có đoạn:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### ❌ Lỗi: "No migrations found"

**Nguyên nhân:** Migrations chưa được tạo

**Giải pháp:**
```powershell
# Tạo migration mới
Add-Migration InitialCreate

# Update database
Update-Database
```

### ❌ Lỗi: "Port 5000 is already in use"

**Nguyên nhân:** Port đã được sử dụng bởi app khác

**Giải pháp:** Đổi port trong `launchSettings.json`:
```json
"applicationUrl": "https://localhost:7001;http://localhost:5001"
```

### ❌ Lỗi: "Sequence contains more than one element" khi đăng nhập Admin

**Nguyên nhân:** Có duplicate admin accounts trong database

**Giải pháp:** Xem file `ADMIN_DUPLICATE_FIX.md` để cleanup database

---

## 📁 CẤU TRÚC THƯ MỤC DỰ ÁN

```
MathUniverse/
├── Controllers/           # Các controller (AdminController, StudentController...)
├── Data/                  # DbContext và DbInitializer
├── Migrations/            # Database migrations
├── Models/                # Models và ViewModels
├── Services/              # Business logic services
├── Views/                 # Razor views
│   ├── Home/             # Trang chủ, Giới thiệu
│   ├── Student/          # Dashboard, Lessons, Progress...
│   ├── Admin/            # Admin Dashboard, Quản lý...
│   └── Shared/           # Layout, _Layout.cshtml
├── wwwroot/              # Static files (CSS, JS, images)
│   ├── css/              # site.css
│   ├── js/               # site.js
│   └── lib/              # Bootstrap, jQuery, FontAwesome
├── appsettings.json      # Cấu hình chung
├── Program.cs            # Entry point
└── MathUniverse.csproj   # Project file
```

---

## 🎯 KIỂM TRA HOÀN TẤT

Sau khi hoàn thành tất cả các bước, hãy kiểm tra:

- ✅ Visual Studio 2022 đã cài đặt thành công
- ✅ Dự án mở và build không lỗi
- ✅ Database đã được tạo với đầy đủ bảng
- ✅ Tài khoản Admin đã được seed
- ✅ Web chạy được trên localhost
- ✅ Có thể đăng nhập Admin
- ✅ Có thể đăng ký Student mới
- ✅ Giao diện hiển thị đẹp (gradient, animations...)

---

## 📞 HỖ TRỢ

Nếu gặp vấn đề, tham khảo các file sau trong dự án:

- `ADMIN_DUPLICATE_FIX.md` - Fix lỗi duplicate admin
- `CLEANUP_INSTRUCTIONS.md` - Hướng dẫn cleanup database
- `ADMIN_GUIDE.md` - Hướng dẫn sử dụng cho Admin

---

## 🎉 HOÀN THÀNH!

Chúc mừng! Bạn đã cài đặt và chạy thành công dự án **Vũ trụ Toán học** 🚀

Giờ bạn có thể:
- 🎨 Tùy chỉnh giao diện trong `Views/` và `wwwroot/css/`
- 📊 Quản lý học sinh và bài giảng qua Admin Dashboard
- 🧪 Test các tính năng học tập qua Student Dashboard
- 💻 Phát triển thêm tính năng mới

**Happy Coding!** 💜✨

