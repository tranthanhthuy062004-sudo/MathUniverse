# 🎓 ỨNG DỤNG OOP TRONG MATHUNIVERSE

## 📋 MỤC LỤC

1. [OOP là gì?](#oop-là-gì)
2. [4 Tính chất cơ bản của OOP](#4-tính-chất-cơ-bản-của-oop)
3. [Encapsulation (Đóng gói)](#encapsulation-đóng-gói)
4. [Inheritance (Kế thừa)](#inheritance-kế-thừa)
5. [Polymorphism (Đa hình)](#polymorphism-đa-hình)
6. [Abstraction (Trừu tượng)](#abstraction-trừu-tượng)
7. [Các Design Patterns](#các-design-patterns)
8. [Dependency Injection](#dependency-injection)
9. [Tổng kết](#tổng-kết)

---

## 🎯 OOP LÀ GÌ?

**OOP (Object-Oriented Programming)** - Lập trình Hướng Đối tượng là phương pháp lập trình dựa trên khái niệm **"đối tượng" (objects)** chứa:
- **Dữ liệu** (properties/fields)
- **Hành vi** (methods/functions)

### Tại sao dùng OOP?

- ✅ **Code dễ đọc, dễ hiểu** - Mô phỏng thế giới thực
- ✅ **Tái sử dụng** - Viết 1 lần, dùng nhiều nơi
- ✅ **Dễ bảo trì** - Thay đổi 1 chỗ, không ảnh hưởng toàn bộ
- ✅ **Mở rộng dễ dàng** - Thêm tính năng không phá code cũ
- ✅ **Làm việc nhóm** - Chia module rõ ràng

---

## 🔷 4 TÍNH CHẤT CƠ BẢN CỦA OOP

```
┌─────────────────────────────────────┐
│         4 TRỤ CỘT CỦA OOP          │
├─────────────────────────────────────┤
│  1. ENCAPSULATION (Đóng gói)       │
│  2. INHERITANCE (Kế thừa)          │
│  3. POLYMORPHISM (Đa hình)         │
│  4. ABSTRACTION (Trừu tượng)       │
└─────────────────────────────────────┘
```

---

## 1️⃣ ENCAPSULATION (Đóng gói)

### Định nghĩa

**Đóng gói** = Gom dữ liệu và phương thức xử lý vào 1 đơn vị (class), che giấu chi tiết bên trong.

### Trong MathUniverse

#### Ví dụ 1: Class Student

```csharp
public class Student
{
    // PRIVATE fields - Che giấu bên trong
    private int studentId;
    private string studentCode;
    private int totalPoints;
    
    // PUBLIC properties - Interface cho bên ngoài
    public int StudentId 
    { 
        get => studentId; 
        set => studentId = value; 
    }
    
    public string StudentCode 
    { 
        get => studentCode;
        private set => studentCode = value;  // Chỉ set từ bên trong
    }
    
    public int TotalPoints 
    { 
        get => totalPoints;
        private set => totalPoints = value;  // Không cho set từ ngoài
    }
    
    // PUBLIC methods - Hành vi
    public void AddPoints(int points)
    {
        if (points > 0)  // Validation
        {
            totalPoints += points;
        }
    }
    
    public void EarnBadge()
    {
        BadgesEarned++;
        // Logic phức tạp che giấu bên trong
    }
}
```

**Lợi ích:**
- ✅ Không ai có thể set `TotalPoints = -100` từ bên ngoài
- ✅ Phải dùng `AddPoints()` → Đảm bảo validation
- ✅ Thay đổi logic bên trong không ảnh hưởng code bên ngoài

#### Ví dụ 2: Class Lesson

```csharp
public class Lesson
{
    // Properties với validation
    private string _title;
    public string Title 
    { 
        get => _title;
        set 
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tiêu đề không được rỗng");
            _title = value;
        }
    }
    
    // Read-only property
    public int QuestionCount => Questions?.Count ?? 0;
    
    // Collection được đóng gói
    private List<Exercise> _exercises = new List<Exercise>();
    public ICollection<Exercise> Exercises => _exercises.AsReadOnly();
    
    // Method để thêm Exercise (kiểm soát cách thêm)
    public void AddExercise(Exercise exercise)
    {
        if (exercise == null)
            throw new ArgumentNullException(nameof(exercise));
            
        if (_exercises.Count >= 10)
            throw new InvalidOperationException("Tối đa 10 bài tập/bài học");
            
        _exercises.Add(exercise);
    }
}
```

**Lợi ích:**
- ✅ Không ai có thể thêm `null` vào Exercises
- ✅ Giới hạn tối đa 10 bài tập được kiểm soát
- ✅ `QuestionCount` tự động tính, không cần set thủ công

---

## 2️⃣ INHERITANCE (Kế thừa)

### Định nghĩa

**Kế thừa** = Class con nhận properties/methods từ class cha, tránh code trùng lặp.

### Trong MathUniverse

#### Ví dụ 1: IdentityUser → ApplicationUser → Student/Admin

```csharp
// Base class từ ASP.NET Identity
public class IdentityUser
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}

// Kế thừa và mở rộng
public class ApplicationUser : IdentityUser
{
    // Inherited từ IdentityUser:
    // - Id, UserName, Email, PasswordHash
    
    // Thêm properties riêng
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

**Lợi ích:**
- ✅ Không cần viết lại `Id`, `Email`, `PasswordHash`
- ✅ Tận dụng sẵn logic authentication của Identity
- ✅ Chỉ thêm những gì cần thiết

#### Ví dụ 2: Controller base class

```csharp
// Base Controller (nếu có)
public class BaseController : Controller
{
    protected readonly ApplicationDbContext _context;
    
    public BaseController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Common method cho tất cả controllers
    protected async Task<Student?> GetCurrentStudentAsync()
    {
        // Logic chung
    }
}

// StudentController kế thừa
public class StudentController : BaseController
{
    // Inherited: _context, GetCurrentStudentAsync()
    
    public async Task<IActionResult> Dashboard()
    {
        var student = await GetCurrentStudentAsync();  // Dùng method từ base
        // ...
    }
}
```

#### Ví dụ 3: DbContext kế thừa

```csharp
// Base class từ EF Core
public class DbContext { ... }

// Base class từ Identity
public class IdentityDbContext<TUser> : DbContext { ... }

// Class của chúng ta kế thừa
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // Inherited:
    // - SaveChanges(), SaveChangesAsync()
    // - Database connection management
    // - Change tracking
    // - Users, Roles tables
    
    // Thêm DbSets riêng
    public DbSet<Student> Students { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    // ...
}
```

---

## 3️⃣ POLYMORPHISM (Đa hình)

### Định nghĩa

**Đa hình** = Cùng 1 interface/method nhưng hành vi khác nhau tùy context.

### Trong MathUniverse

#### Ví dụ 1: Interface ILessonService

```csharp
// Interface định nghĩa "hợp đồng"
public interface ILessonService
{
    Task<IEnumerable<Lesson>> GetLessonsByGradeAsync(int grade);
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task CreateLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task<bool> DeleteLessonAsync(int id);
}

// Implementation 1: Lấy từ Database
public class LessonService : ILessonService
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IEnumerable<Lesson>> GetLessonsByGradeAsync(int grade)
    {
        return await _context.Lessons
            .Where(l => l.Grade == grade)
            .ToListAsync();
    }
    // ...
}

// Implementation 2: Lấy từ Cache (nếu cần)
public class CachedLessonService : ILessonService
{
    private readonly IMemoryCache _cache;
    private readonly LessonService _innerService;
    
    public async Task<IEnumerable<Lesson>> GetLessonsByGradeAsync(int grade)
    {
        var cacheKey = $"lessons_grade_{grade}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Lesson> cached))
            return cached;
            
        var lessons = await _innerService.GetLessonsByGradeAsync(grade);
        _cache.Set(cacheKey, lessons, TimeSpan.FromMinutes(10));
        return lessons;
    }
    // ...
}

// Controller không cần biết implementation nào
public class StudentController : Controller
{
    private readonly ILessonService _lessonService;  // Interface!
    
    // DI sẽ inject implementation phù hợp
    public StudentController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }
    
    public async Task<IActionResult> Lessons()
    {
        // Dùng interface, không quan tâm implementation
        var lessons = await _lessonService.GetLessonsByGradeAsync(5);
        return View(lessons);
    }
}
```

**Lợi ích:**
- ✅ Đổi implementation không cần sửa Controller
- ✅ Dễ test (mock ILessonService)
- ✅ Tuân theo SOLID principles

#### Ví dụ 2: Method Overloading

```csharp
public class ExerciseService
{
    // Cùng tên method, khác parameters
    public async Task<IEnumerable<Exercise>> GetExercisesAsync()
    {
        return await _context.Exercises.ToListAsync();
    }
    
    public async Task<IEnumerable<Exercise>> GetExercisesAsync(int grade)
    {
        return await _context.Exercises
            .Where(e => e.Lesson.Grade == grade)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Exercise>> GetExercisesAsync(int grade, DifficultyLevel difficulty)
    {
        return await _context.Exercises
            .Where(e => e.Lesson.Grade == grade && e.Difficulty == difficulty)
            .ToListAsync();
    }
}

// Gọi:
var all = await _exerciseService.GetExercisesAsync();              // Tất cả
var grade5 = await _exerciseService.GetExercisesAsync(5);          // Lớp 5
var grade5Easy = await _exerciseService.GetExercisesAsync(5, DifficultyLevel.Easy);  // Lớp 5, Dễ
```

#### Ví dụ 3: Enum Polymorphism

```csharp
public enum ExerciseType
{
    MultipleChoice = 0,
    DragAndDrop = 1,
    Matching = 2,
    Interactive = 3
}

// Hiển thị khác nhau dựa trên Type
@switch (exercise.Type)
{
    case ExerciseType.MultipleChoice:
        <span class="badge bg-primary">Trắc nghiệm</span>
        break;
    case ExerciseType.DragAndDrop:
        <span class="badge bg-secondary">Kéo thả</span>
        break;
    // ...
}
```

---

## 4️⃣ ABSTRACTION (Trừu tượng)

### Định nghĩa

**Trừu tượng** = Ẩn đi chi tiết phức tạp, chỉ để lộ những gì cần thiết.

### Trong MathUniverse

#### Ví dụ 1: Service Layer

```csharp
// Interface - Abstract layer
public interface IStudentProgressService
{
    Task<bool> UpdateVideoProgressAsync(int studentId, int lessonId, int watchedSeconds);
    Task<bool> MarkLessonCompletedAsync(int studentId, int lessonId);
    Task<StudentProgress?> GetProgressAsync(int studentId, int lessonId);
}

// Implementation - Che giấu chi tiết
public class StudentProgressService : IStudentProgressService
{
    public async Task<bool> UpdateVideoProgressAsync(int studentId, int lessonId, int watchedSeconds)
    {
        // Chi tiết phức tạp:
        // 1. Tìm/tạo progress record
        var progress = await FindOrCreateProgress(studentId, lessonId);
        
        // 2. Tính % hoàn thành
        var lesson = await _context.Lessons.FindAsync(lessonId);
        var percentage = CalculatePercentage(watchedSeconds, lesson.VideoDuration);
        
        // 3. Update status
        progress.CompletionPercentage = percentage;
        progress.Status = DetermineStatus(percentage);
        
        // 4. Save
        await _context.SaveChangesAsync();
        
        // 5. Trigger events (nếu cần)
        if (percentage >= 100)
            await OnVideoCompleted(studentId, lessonId);
            
        return true;
    }
    
    // Private helpers - Ẩn logic phức tạp
    private async Task<StudentProgress> FindOrCreateProgress(...) { ... }
    private int CalculatePercentage(...) { ... }
    private ProgressStatus DetermineStatus(...) { ... }
    private async Task OnVideoCompleted(...) { ... }
}
```

**Controller chỉ cần:**
```csharp
public async Task<IActionResult> CompleteVideo(int lessonId)
{
    // Đơn giản, không cần biết chi tiết bên trong
    var result = await _progressService.MarkLessonCompletedAsync(student.StudentId, lessonId);
    return Json(new { success = result });
}
```

#### Ví dụ 2: Repository Pattern (nếu dùng)

```csharp
// Abstract interface
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Generic implementation
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }
    
    // ...
}

// Sử dụng
public class LessonRepository : Repository<Lesson>
{
    public LessonRepository(ApplicationDbContext context) : base(context) { }
    
    // Thêm methods đặc thù cho Lesson
    public async Task<IEnumerable<Lesson>> GetByGradeAsync(int grade)
    {
        return await _dbSet.Where(l => l.Grade == grade).ToListAsync();
    }
}
```

---

## 🎨 CÁC DESIGN PATTERNS

### 1. **MVC Pattern** (Model-View-Controller)

Toàn bộ app dùng MVC:

```
┌──────────────────────────────────────┐
│              MVC FLOW                │
├──────────────────────────────────────┤
│                                      │
│  User Request                        │
│       │                              │
│       ▼                              │
│  ┌─────────────┐                     │
│  │ Controller  │ ← Logic, validation │
│  └──────┬──────┘                     │
│         │                            │
│         ├──────────┐                 │
│         │          │                 │
│         ▼          ▼                 │
│    ┌───────┐  ┌───────┐             │
│    │ Model │  │ View  │             │
│    └───────┘  └───────┘             │
│    Database   HTML/Razor            │
│                                      │
└──────────────────────────────────────┘
```

**Ví dụ:**
```csharp
// Model
public class Lesson { ... }

// Controller
public class StudentController : Controller
{
    public async Task<IActionResult> Lessons()
    {
        var lessons = await _lessonService.GetLessonsByGradeAsync(5);  // Model
        return View(lessons);  // View
    }
}

// View (Lessons.cshtml)
@model List<Lesson>
@foreach (var lesson in Model)
{
    <div>@lesson.Title</div>
}
```

---

### 2. **Repository Pattern**

Abstraction layer giữa Business Logic và Data Access.

```csharp
// Without Repository (trực tiếp dùng DbContext)
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IActionResult> Dashboard()
    {
        var student = await _context.Students
            .Include(s => s.Progress)
            .FirstOrDefaultAsync(s => s.UserId == userId);  // Trực tiếp query
    }
}

// With Repository (clean hơn)
public class StudentController : Controller
{
    private readonly IStudentRepository _studentRepo;
    
    public async Task<IActionResult> Dashboard()
    {
        var student = await _studentRepo.GetByUserIdAsync(userId);  // Abstract
    }
}
```

---

### 3. **Service Pattern**

Business logic được tách ra Services.

```csharp
// Service chứa business logic
public class ExerciseService : IExerciseService
{
    public async Task<ExerciseSubmissionResult> SubmitExerciseAsync(
        int studentId, int exerciseId, Dictionary<int, int> answers, int timeSpent)
    {
        // 1. Validate
        var exercise = await GetExerciseByIdAsync(exerciseId);
        if (exercise == null)
            throw new NotFoundException("Exercise not found");
            
        // 2. Check attempts
        var attempts = await GetAttemptsCountAsync(studentId, exerciseId);
        if (attempts >= exercise.MaxAttempts)
            throw new MaxAttemptsExceededException();
            
        // 3. Calculate score
        var score = await CalculateScoreAsync(exerciseId, answers);
        
        // 4. Save result
        var result = new ExerciseResult { ... };
        await _context.ExerciseResults.AddAsync(result);
        
        // 5. Update progress if passed
        if (score >= exercise.PassingScore)
            await UpdateStudentProgressAsync(studentId, exercise.LessonId);
            
        await _context.SaveChangesAsync();
        
        return new ExerciseSubmissionResult { Score = score, ... };
    }
}

// Controller chỉ gọi service
public class StudentController : Controller
{
    public async Task<IActionResult> SubmitExercise(SubmitExerciseViewModel model)
    {
        var result = await _exerciseService.SubmitExerciseAsync(...);
        return Json(result);
    }
}
```

---

### 4. **Factory Pattern** (trong Dependency Injection)

```csharp
// Startup.cs / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Factory tạo instances
    services.AddScoped<ILessonService, LessonService>();
    services.AddScoped<IExerciseService, ExerciseService>();
    services.AddScoped<IStudentProgressService, StudentProgressService>();
}

// Runtime tự động tạo và inject
public class StudentController : Controller
{
    // Constructor injection
    public StudentController(
        ILessonService lessonService,      // Factory tạo
        IExerciseService exerciseService,  // Factory tạo
        IStudentProgressService progressService)  // Factory tạo
    {
        // Không cần `new`, DI container tự tạo
    }
}
```

---

### 5. **ViewModel Pattern**

```csharp
// Model (Domain/Entity)
public class Student
{
    public int StudentId { get; set; }
    public string FullName { get; set; }
    public int TotalPoints { get; set; }
    // ... 20 properties khác
}

// ViewModel (chỉ những gì View cần)
public class StudentDashboardViewModel
{
    public string FullName { get; set; }
    public int TotalPoints { get; set; }
    public List<LessonViewModel> RecentLessons { get; set; }
    public StudentStatistics Statistics { get; set; }
}

// Controller
public async Task<IActionResult> Dashboard()
{
    var student = await GetCurrentStudentAsync();  // Entity
    
    var viewModel = new StudentDashboardViewModel  // ViewModel
    {
        FullName = student.FullName,
        TotalPoints = student.TotalPoints,
        RecentLessons = ...,
        Statistics = ...
    };
    
    return View(viewModel);  // Truyền ViewModel, không phải Entity
}
```

**Lợi ích:**
- ✅ View chỉ nhận data cần thiết
- ✅ Không expose sensitive data (PasswordHash, UserId...)
- ✅ Flatten complex object graphs
- ✅ Dễ validate với Data Annotations

---

## 💉 DEPENDENCY INJECTION

### DI là gì?

**Dependency Injection** = Truyền dependencies (objects cần thiết) từ bên ngoài vào, thay vì tự tạo bên trong.

### Bad Practice (Không dùng DI)

```csharp
public class StudentController : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        // Tự tạo dependencies - BAD!
        var context = new ApplicationDbContext();
        var lessonService = new LessonService(context);
        var progressService = new StudentProgressService(context);
        
        // ...
    }
    
    // Vấn đề:
    // - Tight coupling (phụ thuộc chặt)
    // - Khó test (không mock được)
    // - Quản lý lifecycle khó
    // - Duplicate code
}
```

### Good Practice (Dùng DI)

```csharp
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILessonService _lessonService;
    private readonly IStudentProgressService _progressService;
    
    // Constructor Injection
    public StudentController(
        ApplicationDbContext context,
        ILessonService lessonService,
        IStudentProgressService progressService)
    {
        _context = context;
        _lessonService = lessonService;
        _progressService = progressService;
    }
    
    public async Task<IActionResult> Dashboard()
    {
        // Dùng dependencies đã được inject
        var lessons = await _lessonService.GetLessonsByGradeAsync(5);
        // ...
    }
}
```

### Đăng ký trong Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Đăng ký DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký Services
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IStudentProgressService, StudentProgressService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

### Service Lifetimes

```csharp
// 1. TRANSIENT - Mỗi lần request tạo instance mới
services.AddTransient<IEmailSender, EmailSender>();

// 2. SCOPED - 1 instance cho 1 HTTP request
services.AddScoped<ILessonService, LessonService>();
services.AddScoped<ApplicationDbContext>();  // DbContext nên dùng Scoped

// 3. SINGLETON - 1 instance duy nhất suốt app lifetime
services.AddSingleton<IMemoryCache, MemoryCache>();
```

---

## 📦 CẤU TRÚC THƯMỤC OOP

```
MathUniverse/
│
├── Controllers/           ← Controllers (MVC)
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── StudentController.cs
│   └── GameController.cs
│
├── Models/               ← Domain Models (Entities)
│   ├── Student.cs
│   ├── Lesson.cs
│   ├── Exercise.cs
│   ├── Question.cs
│   ├── Answer.cs
│   └── ViewModels/       ← ViewModels
│       ├── StudentViewModels.cs
│       ├── LessonViewModels.cs
│       └── AdminViewModels.cs
│
├── Services/             ← Business Logic (Service Layer)
│   ├── ILessonService.cs          (Interface)
│   ├── LessonService.cs           (Implementation)
│   ├── IExerciseService.cs
│   ├── ExerciseService.cs
│   ├── IStudentProgressService.cs
│   └── StudentProgressService.cs
│
├── Data/                 ← Data Access Layer
│   ├── ApplicationDbContext.cs    (DbContext)
│   └── DbInitializer.cs          (Seeding)
│
├── Utilities/            ← Helper classes
│   └── DatabaseCleanup.cs
│
└── Views/                ← Views (MVC)
    ├── Student/
    ├── Admin/
    └── Shared/
```

---

## 🎯 TỔNG KẾT

### OOP trong MathUniverse được áp dụng qua:

#### 1. **Encapsulation** (Đóng gói)
- ✅ Properties với validation
- ✅ Private fields, Public methods
- ✅ Read-only properties
- ✅ Controlled access (getters/setters)

**Ví dụ:**
```csharp
public class Student
{
    public int TotalPoints { get; private set; }  // Encapsulated
    
    public void AddPoints(int points)  // Controlled access
    {
        if (points > 0) TotalPoints += points;
    }
}
```

#### 2. **Inheritance** (Kế thừa)
- ✅ `ApplicationUser : IdentityUser`
- ✅ `ApplicationDbContext : IdentityDbContext`
- ✅ Controllers extend `Controller`

**Ví dụ:**
```csharp
public class ApplicationUser : IdentityUser  // Kế thừa
{
    public string FullName { get; set; }  // Mở rộng
}
```

#### 3. **Polymorphism** (Đa hình)
- ✅ Interfaces (`ILessonService`, `IExerciseService`)
- ✅ Method overloading
- ✅ Dependency Injection

**Ví dụ:**
```csharp
// Interface
public interface ILessonService
{
    Task<Lesson?> GetLessonByIdAsync(int id);
}

// Implementation 1
public class LessonService : ILessonService { ... }

// Implementation 2
public class CachedLessonService : ILessonService { ... }

// Controller không cần biết implementation nào
public StudentController(ILessonService lessonService) { }
```

#### 4. **Abstraction** (Trừu tượng)
- ✅ Service layer che giấu business logic
- ✅ Repository pattern
- ✅ ViewModel pattern

**Ví dụ:**
```csharp
// Abstract
public interface IStudentProgressService
{
    Task<bool> MarkLessonCompletedAsync(int studentId, int lessonId);
}

// Implementation ẩn chi tiết phức tạp
public class StudentProgressService : IStudentProgressService
{
    public async Task<bool> MarkLessonCompletedAsync(...)
    {
        // 10+ lines of complex logic here
    }
}

// Controller chỉ cần 1 dòng
await _progressService.MarkLessonCompletedAsync(studentId, lessonId);
```

---

### Design Patterns sử dụng:

1. ✅ **MVC Pattern** - Toàn bộ architecture
2. ✅ **Repository Pattern** - Data access abstraction
3. ✅ **Service Pattern** - Business logic separation
4. ✅ **Dependency Injection** - Loose coupling
5. ✅ **ViewModel Pattern** - View-specific models
6. ✅ **Factory Pattern** - DI Container
7. ✅ **Strategy Pattern** - Essay grading với multiple choice
8. ✅ **Observer Pattern** - Notification system

---

### Tính năng mới (Updated Jan 2026):

#### 1. **Essay Grading System**
- ✅ Hỗ trợ câu hỏi tự luận (`QuestionType.Essay`)
- ✅ Admin chấm điểm qua `ViewExerciseResult`
- ✅ `GradingStatus` enum (Graded, PendingGrading)
- ✅ Tự động notification khi chấm xong

```csharp
public enum QuestionType
{
    MultipleChoice = 1,  // Trắc nghiệm - tự động chấm
    Essay = 2            // Tự luận - admin chấm
}

public enum GradingStatus
{
    Graded = 0,          // Đã chấm xong
    PendingGrading = 1   // Đang chờ admin chấm
}
```

#### 2. **Soft Delete Pattern**
- ✅ Xóa bài giảng không mất điểm học sinh
- ✅ `Lesson.IsDeleted` thay vì hard delete
- ✅ Giữ lại ExerciseResults và audit trail

```csharp
public class Lesson
{
    public bool IsDeleted { get; set; } = false;  // Soft delete
}

public async Task<bool> DeleteLessonAsync(int lessonId)
{
    lesson.IsDeleted = true;  // Đánh dấu xóa, không xóa hẳn
    lesson.UpdatedDate = DateTime.Now;
}
```

#### 3. **Score Calculation Fix**
- ✅ Tính điểm dựa trên `Score` thay vì `CorrectAnswers`
- ✅ Hỗ trợ bài tự luận và trắc nghiệm
- ✅ Weighted average cho bài mix

```csharp
// Tính TotalPoints chính xác
var highestScores = await _context.ExerciseResults
    .Where(er => er.StudentId == studentId && er.GradingStatus == GradingStatus.Graded)
    .GroupBy(er => er.ExerciseId)
    .Select(g => new { HighestScore = g.Max(er => er.Score) })
    .ToListAsync();

double totalPoints = highestScores.Sum(hs => hs.HighestScore);
```

#### 4. **Real-time UI Updates**
- ✅ Notification đánh dấu đã đọc ngay lập tức
- ✅ Xóa tất cả học sinh với fade animation
- ✅ No page reload required

---

### Lợi ích đạt được:

| Lợi ích | Trước OOP | Sau OOP |
|---------|-----------|---------|
| **Code reuse** | Copy-paste | Inheritance, Interface |
| **Maintainability** | Khó sửa | Dễ sửa, isolated changes |
| **Testability** | Khó test | Dễ mock, unit test |
| **Scalability** | Khó mở rộng | Dễ thêm features |
| **Team work** | Conflict nhiều | Chia module rõ ràng |
| **Code quality** | Spaghetti code | Clean, organized |
| **Data Safety** | Hard delete | Soft delete, audit trail |
| **Performance** | N/A | Optimized queries, caching |

---

### Architecture Highlights:

#### Service Layer Pattern
```csharp
// IExerciseService - Interface (Abstraction)
public interface IExerciseService
{
    Task<ExerciseResult> SubmitExerciseAsync(int studentId, int exerciseId, 
        Dictionary<int, int> answers, int timeSpent);
    Task RecalculateStudentTotalPointsAsync(int studentId);
}

// ExerciseService - Implementation (Encapsulation)
public class ExerciseService : IExerciseService
{
    private readonly ApplicationDbContext _context;
    
    // Constructor injection (Dependency Injection)
    public ExerciseService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    // Business logic encapsulated
    public async Task<ExerciseResult> SubmitExerciseAsync(...)
    {
        // Xử lý trắc nghiệm
        // Xử lý tự luận
        // Set GradingStatus
        // Return result
    }
}
```

#### Controller Layer (Polymorphism via DI)
```csharp
public class StudentController : Controller
{
    private readonly IExerciseService _exerciseService;
    private readonly INotificationService _notificationService;
    
    // Dependency Injection - không phụ thuộc vào implementation cụ thể
    public StudentController(
        IExerciseService exerciseService,
        INotificationService notificationService)
    {
        _exerciseService = exerciseService;
        _notificationService = notificationService;
    }
}
```

---

**Tác giả:** MathUniverse Dev Team  
**Ngày cập nhật:** 06/01/2026  
**Phiên bản:** 2.0  
**Tính năng mới:** Essay Grading, Soft Delete, Score Fix, Real-time UI

