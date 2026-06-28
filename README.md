# 🚀 Vũ Trụ Toán Học - MathUniverse

Nền tảng học Toán trực tuyến dành cho học sinh Tiểu học (Lớp 1-5)

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue.svg)

---

## 📖 Giới Thiệu

**Vũ trụ Toán học** là một hệ thống quản lý và học tập Toán học trực tuyến được thiết kế đặc biệt cho học sinh Tiểu học từ lớp 1 đến lớp 5. Dự án cung cấp:

- 🎥 **Video bài giảng** ngắn gọn, dễ hiểu
- 🧩 **Bài tập tương tác** vui nhộn
- 📊 **Theo dõi tiến độ** chi tiết
- 🏆 **Hệ thống điểm** và xếp hạng
- 👨‍🏫 **Dashboard quản trị** cho giáo viên

---

## ✨ Tính Năng Chính

### Dành cho Học Sinh:
- ✅ Xem video bài giảng theo từng lớp
- ✅ Làm bài tập trắc nghiệm có chấm điểm
- ✅ Theo dõi tiến độ học tập cá nhân
- ✅ Xem bảng xếp hạng lớp
- ✅ Nhận thông báo từ giáo viên
- ✅ Quản lý hồ sơ cá nhân

### Dành cho Admin/Giáo Viên:
- ✅ Quản lý học sinh (thêm, sửa, xóa)
- ✅ Tạo và quản lý bài giảng
- ✅ Tạo câu hỏi và bài tập
- ✅ Xem báo cáo chi tiết từng học sinh
- ✅ Gửi thông báo cho học sinh
- ✅ Thống kê tổng quan hệ thống

---

## 🛠️ Công Nghệ Sử Dụng

### Backend:
- **ASP.NET Core 8.0** - Framework chính
- **Entity Framework Core 8.0** - ORM
- **ASP.NET Core Identity** - Xác thực và phân quyền
- **SQL Server** - Database

### Frontend:
- **Razor Pages** - Template engine
- **Bootstrap 5** - CSS Framework
- **Font Awesome 6** - Icons
- **jQuery** - JavaScript library

### Database:
- **SQL Server LocalDB** (Development)
- **SQL Server Express/Standard** (Production)

---

## 📋 Yêu Cầu Hệ Thống

- **OS:** Windows 10/11 (64-bit)
- **IDE:** Visual Studio 2022 Community (trở lên)
- **.NET:** .NET 8.0 SDK
- **Database:** SQL Server LocalDB / SQL Server Express
- **RAM:** Tối thiểu 4GB (Khuyến nghị 8GB+)
- **Storage:** 10GB trống

---

## 🚀 Hướng Dẫn Cài Đặt

### Cài đặt nhanh (TL;DR):

```bash
# 1. Clone hoặc giải nén dự án
cd D:\MathUniverse

# 2. Restore packages
dotnet restore

# 3. Tạo database
dotnet ef database update

# 4. Chạy dự án
dotnet run
```

### Hướng dẫn chi tiết:

📄 Xem file **[SETUP_GUIDE.md](SETUP_GUIDE.md)** để có hướng dẫn từng bước chi tiết

📋 Hoặc sử dụng **[SETUP_CHECKLIST.md](SETUP_CHECKLIST.md)** để theo dõi quá trình cài đặt

---

## 📁 Cấu Trúc Dự Án

```
MathUniverse/
├── Controllers/              # API Controllers
│   ├── HomeController.cs     # Trang chủ, Giới thiệu
│   ├── AccountController.cs  # Đăng ký, Đăng nhập
│   ├── StudentController.cs  # Dashboard học sinh
│   └── AdminController.cs    # Dashboard admin
├── Data/
│   ├── ApplicationDbContext.cs   # Database context
│   └── DbInitializer.cs          # Seed data
├── Migrations/               # EF Core migrations
├── Models/                   # Data models
│   ├── User.cs              # ApplicationUser
│   ├── Student.cs           # Học sinh
│   ├── Lesson.cs            # Bài giảng
│   ├── Exercise.cs          # Bài tập
│   └── ViewModels/          # View models
├── Services/                 # Business logic
│   ├── LessonService.cs
│   ├── ExerciseService.cs
│   └── StudentProgressService.cs
├── Views/                    # Razor views
│   ├── Home/                # Trang chủ
│   ├── Student/             # Giao diện học sinh
│   ├── Admin/               # Giao diện admin
│   └── Shared/              # Layout chung
├── wwwroot/                  # Static files
│   ├── css/                 # Stylesheets
│   ├── js/                  # JavaScript
│   └── lib/                 # Libraries
├── appsettings.json          # Cấu hình
├── Program.cs                # Entry point
└── MathUniverse.csproj       # Project file
```

---

## 🔑 Tài Khoản Mặc Định

### Admin:
- **Email:** admin@mathuniverse.com
- **Password:** Admin@123

### Student (Tự đăng ký):
- Đăng ký tại: `/Account/Register`
- Hệ thống tự động tạo mã học sinh

---

## 📸 Screenshots

### Trang Chủ
![Home Page](docs/screenshots/homepage.png)

### Dashboard Học Sinh
![Student Dashboard](docs/screenshots/student-dashboard.png)

### Dashboard Admin
![Admin Dashboard](docs/screenshots/admin-dashboard.png)

---

## 🗄️ Database Schema

### Bảng chính:

- **AspNetUsers** - Tài khoản người dùng (Identity)
- **Students** - Thông tin học sinh
- **Admins** - Thông tin admin/giáo viên
- **Lessons** - Bài giảng
- **Exercises** - Bài tập
- **Questions** - Câu hỏi
- **ExerciseResults** - Kết quả làm bài
- **StudentProgress** - Tiến độ học tập
- **Notifications** - Thông báo
- **ActivityLogs** - Nhật ký hoạt động

---

## 🧪 Testing

### Test tài khoản Admin:
1. Chạy dự án
2. Truy cập `/Account/Login`
3. Đăng nhập với tài khoản admin mặc định
4. Kiểm tra các chức năng quản trị

### Test đăng ký Student:
1. Truy cập `/Account/Register`
2. Điền form đăng ký
3. Hệ thống tạo mã học sinh tự động
4. Đăng nhập và kiểm tra dashboard

## 🤝 Đóng Góp

Dự án này được phát triển bởi **Nhóm 08 - INT2204N1**

### Team Members:

---

## 📄 License

Dự án này được phát hành dưới giấy phép **MIT License**.

---

## 📞 Liên Hệ

- **Email:** INT2204N1nhom08@VuTruToanHoc.edu.vn
- **GitHub:** [MathUniverse Repository]
- **Documentation:** Xem thư mục `docs/`

---

## 🎯 Roadmap

### Version 1.0 (Current)
- ✅ Quản lý học sinh
- ✅ Quản lý bài giảng
- ✅ Hệ thống bài tập
- ✅ Dashboard admin và student

### Version 1.1 (Planned)
- 🔲 Thêm chơi game học toán
- 🔲 Chat trực tiếp với giáo viên
- 🔲 Báo cáo cho phụ huynh
- 🔲 Mobile app (React Native)

### Version 2.0 (Future)
- 🔲 AI đề xuất bài học
- 🔲 Video call 1-1 với giáo viên
- 🔲 Marketplace bài giảng
- 🔲 Multi-language support

---

## ⭐ Đánh Giá

Nếu bạn thấy dự án hữu ích, hãy cho chúng tôi một ⭐ trên GitHub!

---

**Made with 💜 by Nhóm 08 - INT2204N1**

*Học toán vui, dễ hiểu cho học sinh tiểu học* 🚀✨

