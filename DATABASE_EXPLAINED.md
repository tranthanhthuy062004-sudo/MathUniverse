﻿# 🗄️ KIẾN TRÚC VÀ HOẠT ĐỘNG CỦA DATABASE - MATHUNIVERSE

## 📋 MỤC LỤC

1. [Tổng quan Database](#tổng-quan-database)
2. [Công nghệ sử dụng](#công-nghệ-sử-dụng)
3. [Cấu trúc các bảng](#cấu-trúc-các-bảng)
4. [Relationships (Quan hệ)](#relationships-quan-hệ)
5. [Cách dữ liệu được lưu](#cách-dữ-liệu-được-lưu)
6. [Migrations](#migrations)
7. [Entity Framework Core](#entity-framework-core)
8. [Ví dụ thực tế](#ví-dụ-thực-tế)

---

## 🎯 TỔNG QUAN DATABASE

### Database là gì?

**Database** (Cơ sở dữ liệu) là nơi lưu trữ tất cả thông tin của ứng dụng MathUniverse:
- Thông tin học sinh, giáo viên
- Bài giảng, bài tập
- Điểm số, tiến độ học tập
- Thông báo, lịch sử hoạt động

### Tại sao cần Database?

```
┌─────────────────────────────────────────┐
│   Không có Database (Bad)               │
├─────────────────────────────────────────┤
│  • Dữ liệu mất khi tắt máy              │
│  • Không tìm kiếm được                  │
│  • Không lưu lâu dài                    │
│  • Không quản lý được nhiều user        │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│   Có Database (Good)                    │
├─────────────────────────────────────────┤
│  ✅ Lưu vĩnh viễn (persistent)          │
│  ✅ Tìm kiếm nhanh                       │
│  ✅ Đồng bộ nhiều user                   │
│  ✅ Bảo mật, backup được                 │
└─────────────────────────────────────────┘
```

---

## 💻 CÔNG NGHỆ SỬ DỤNG

### SQLite

MathUniverse sử dụng **SQLite** - Một database nhẹ, không cần server riêng.

**Ưu điểm:**
- ✅ Không cần cài đặt server riêng (như MySQL, SQL Server)
- ✅ Toàn bộ database trong 1 file duy nhất (`MathUniverse.db`)
- ✅ Nhanh, nhẹ, phù hợp với ứng dụng vừa và nhỏ
- ✅ Dễ backup (copy file là xong)

**File database:**
```
D:\MathUniverse\
├── MathUniverse.db      ← Toàn bộ dữ liệu ở đây
├── MathUniverse.db-shm  ← File hỗ trợ (shared memory)
└── MathUniverse.db-wal  ← Write-Ahead Log
```

### Entity Framework Core (EF Core)

**EF Core** là công cụ giúp C# code giao tiếp với Database mà không cần viết SQL thủ công.

```csharp
// Không dùng EF Core (phải viết SQL)
string sql = "SELECT * FROM Students WHERE Grade = 5";
var students = ExecuteQuery(sql);

// Dùng EF Core (viết C# thuần)
var students = _context.Students.Where(s => s.Grade == 5).ToList();
```

**Lợi ích:**
- ✅ Viết code C# thay vì SQL
- ✅ Tự động tạo bảng, tự động update schema
- ✅ Type-safe (kiểm tra lỗi lúc compile)
- ✅ LINQ queries (dễ đọc, dễ viết)

---

## 📊 CẤU TRÚC CÁC BẢNG

### 1. AspNetUsers (Identity Framework)

Bảng gốc từ ASP.NET Identity, lưu thông tin đăng nhập.

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| Id | string (GUID) | ID duy nhất của user |
| UserName | string | Tên đăng nhập |
| Email | string | Email |
| PasswordHash | string | Mật khẩu đã mã hóa |
| PhoneNumber | string | Số điện thoại |
| EmailConfirmed | bool | Email đã xác thực? |

**Ví dụ dữ liệu:**
```
Id: "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
UserName: "nguyenvana"
Email: "nguyenvana@gmail.com"
PasswordHash: "$2a$11$KzPh8..." (đã hash)
```

---

### 2. Students (Học sinh)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| StudentId | int | ID tự động tăng |
| UserId | string | Liên kết với AspNetUsers |
| FullName | string | Họ và tên |
| StudentCode | string | Mã học sinh (unique) |
| Grade | int | Lớp (1-5) |
| DateOfBirth | DateTime | Ngày sinh |
| Gender | string | Giới tính |
| AvatarUrl | string | Đường dẫn avatar |
| TotalPoints | int | Tổng điểm tích lũy |
| BadgesEarned | int | Số huy hiệu |
| IsActive | bool | Tài khoản còn hoạt động? |
| CreatedDate | DateTime | Ngày tạo |

**Ví dụ dữ liệu:**
```
StudentId: 1
UserId: "a1b2c3d4-..." (liên kết AspNetUsers)
FullName: "Nguyễn Văn A"
StudentCode: "HS001"
Grade: 5
TotalPoints: 250
```

---

### 3. Admins (Giáo viên/Quản trị)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| AdminId | int | ID tự động tăng |
| UserId | string | Liên kết với AspNetUsers |
| FullName | string | Họ và tên |
| Role | string | Vai trò (Teacher, Admin) |
| CreatedDate | DateTime | Ngày tạo |

---

### 4. Lessons (Bài giảng)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| LessonId | int | ID tự động tăng |
| Title | string | Tiêu đề bài học |
| Description | string | Mô tả |
| Grade | int | Lớp (1-5) |
| Topic | string | Chủ đề (Số học, Hình học...) |
| VideoUrl | string | Link YouTube |
| PdfUrl | string? | Link file PDF bài giảng |
| VideoDuration | int | Độ dài video (giây) |
| TheoryContent | string | Nội dung lý thuyết (HTML) |
| ThumbnailUrl | string | Ảnh thumbnail |
| OrderIndex | int | Thứ tự bài học |
| IsPublished | bool | Đã công khai? |
| IsDeleted | bool | Đã xóa? (Soft delete) |
| PreviousLessonId | int? | ID bài học trước (unlock logic) |
| CreatedDate | DateTime | Ngày tạo |
| UpdatedDate | DateTime? | Ngày cập nhật |

**Soft Delete Pattern:**
```csharp
// Khi xóa bài giảng, không xóa hẳn khỏi DB
lesson.IsDeleted = true;
lesson.UpdatedDate = DateTime.Now;

// Query chỉ lấy bài chưa xóa
var lessons = await _context.Lessons
    .Where(l => !l.IsDeleted)
    .ToListAsync();
```

**Ví dụ dữ liệu - Bài học active:**
```
LessonId: 10
Title: "Khái niệm số thập phân"
Grade: 5
VideoUrl: "https://www.youtube.com/watch?v=..."
PdfUrl: "/uploads/pdfs/decimal-lesson.pdf"
VideoDuration: 600 (10 phút)
IsPublished: true
IsDeleted: false
OrderIndex: 1
```

**Ví dụ dữ liệu - Bài học đã xóa (soft delete):**
```
LessonId: 11
Title: "Bài học cũ"
IsDeleted: true
UpdatedDate: 2026-01-06 10:00:00
(Không hiển thị cho user nhưng vẫn giữ trong DB)
(Điểm số học sinh từ bài này vẫn còn!)
```

---

### 5. Exercises (Bài tập)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| ExerciseId | int | ID tự động tăng |
| LessonId | int | Thuộc bài học nào |
| Title | string | Tiêu đề bài tập |
| Description | string | Mô tả |
| Difficulty | int | Độ khó (0=Dễ, 1=TB, 2=Khó) |
| TimeLimit | int | Giới hạn thời gian (phút) |
| PassingScore | int | Điểm đạt (tối thiểu) |
| CreatedDate | DateTime | Ngày tạo |

**Ví dụ:**
```
ExerciseId: 5
LessonId: 10
Title: "Bài tập về số thập phân"
Difficulty: 0 (Dễ)
TimeLimit: 15 (phút)
PassingScore: 70 (70 điểm trở lên là đạt)
```

---

### 6. Questions (Câu hỏi)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| QuestionId | int | ID tự động tăng |
| ExerciseId | int | Thuộc bài tập nào |
| QuestionText | string | Nội dung câu hỏi |
| Type | int | 1=Trắc nghiệm, 2=Tự luận |
| ImageUrl | string? | Ảnh đi kèm (nếu có) |
| AudioUrl | string? | Audio đi kèm (nếu có) |
| Points | int | Điểm cho câu hỏi này |
| OrderIndex | int | Thứ tự hiển thị |

**QuestionType Enum:**
```csharp
public enum QuestionType
{
    MultipleChoice = 1,  // Trắc nghiệm - tự động chấm
    Essay = 2            // Tự luận - admin chấm thủ công
}
```

**Ví dụ - Trắc nghiệm:**
```
QuestionId: 101
ExerciseId: 5
QuestionText: "Số nào sau đây là số thập phân?"
Type: 1 (MultipleChoice)
Points: 10
```

**Ví dụ - Tự luận:**
```
QuestionId: 102
ExerciseId: 5
QuestionText: "Giải thích cách chuyển đổi phân số 3/4 thành số thập phân"
Type: 2 (Essay)
Points: 10
```

---

### 7. Answers (Đáp án)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| AnswerId | int | ID tự động tăng |
| QuestionId | int | Thuộc câu hỏi nào |
| AnswerText | string | Nội dung đáp án |
| IsCorrect | bool | Đáp án đúng? |
| OrderIndex | int | Thứ tự hiển thị |

**Ví dụ:**
```
# Câu hỏi: "Số nào sau đây là số thập phân?"

AnswerId: 401, QuestionId: 101, AnswerText: "3/5", IsCorrect: false
AnswerId: 402, QuestionId: 101, AnswerText: "2,4", IsCorrect: true  ✓
AnswerId: 403, QuestionId: 101, AnswerText: "7", IsCorrect: false
AnswerId: 404, QuestionId: 101, AnswerText: "5/10", IsCorrect: false
```

---

### 8. StudentProgress (Tiến độ học tập)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| ProgressId | int | ID tự động tăng |
| StudentId | int | Học sinh nào |
| LessonId | int | Bài học nào |
| Status | int | 0=Chưa bắt đầu, 1=Đang học, 2=Hoàn thành |
| CompletionPercentage | int | % hoàn thành (0-100) |
| VideoWatchedSeconds | int | Đã xem bao nhiêu giây video |
| HighestScore | int | Điểm cao nhất |
| LastAccessedDate | DateTime | Lần truy cập cuối |
| CompletedDate | DateTime? | Ngày hoàn thành |

**Ví dụ:**
```
StudentId: 1 (Nguyễn Văn A)
LessonId: 10 (Số thập phân)
Status: 2 (Hoàn thành)
CompletionPercentage: 100
VideoWatchedSeconds: 600 (10 phút)
HighestScore: 90
```

---

### 9. ExerciseResults (Kết quả bài tập)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| ResultId | int | ID tự động tăng |
| StudentId | int | Học sinh nào |
| ExerciseId | int | Bài tập nào |
| Score | double | Điểm số (0-10) |
| CorrectAnswers | int | Số câu trắc nghiệm đúng |
| TotalQuestions | int | Tổng số câu trắc nghiệm |
| TimeSpent | int | Thời gian làm bài (giây) |
| AttemptNumber | int | Lần thử thứ mấy |
| IsPassed | bool | Đạt hay không |
| GradingStatus | int | 0=Đã chấm, 1=Đang chờ chấm |
| CompletedDate | DateTime | Ngày làm bài |
| AnswersJson | string | JSON lưu đáp án đã chọn |

**GradingStatus Enum:**
```csharp
public enum GradingStatus
{
    Graded = 0,          // Đã chấm xong (không có tự luận hoặc đã chấm hết)
    PendingGrading = 1   // Đang chờ admin chấm tự luận
}
```

**Ví dụ - Bài trắc nghiệm (tự động chấm):**
```
StudentId: 1
ExerciseId: 5
Score: 9.0
CorrectAnswers: 9
TotalQuestions: 10
GradingStatus: 0 (Graded)
IsPassed: true
```

**Ví dụ - Bài có tự luận (chờ chấm):**
```
StudentId: 1
ExerciseId: 6
Score: 7.5 (điểm tạm từ phần trắc nghiệm)
CorrectAnswers: 3
TotalQuestions: 4 (chỉ tính trắc nghiệm)
GradingStatus: 1 (PendingGrading)
IsPassed: false (chưa chấm xong)
```

---

### 10. EssayAnswers (Câu trả lời tự luận)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| EssayAnswerId | int | ID tự động tăng |
| ExerciseResultId | int | Thuộc kết quả bài tập nào |
| QuestionId | int | Câu hỏi nào |
| AnswerText | string | Câu trả lời văn bản |
| ImageUrl | string? | Ảnh câu trả lời (nếu có) |
| Score | double? | Điểm admin chấm (null = chưa chấm) |
| Feedback | string? | Nhận xét từ admin |
| SubmittedDate | DateTime | Ngày nộp |
| GradedDate | DateTime? | Ngày chấm điểm |
| GradedByAdminId | int? | Admin nào chấm |

**Ví dụ - Chưa chấm:**
```
EssayAnswerId: 1
ExerciseResultId: 15
QuestionId: 102
AnswerText: "Để chuyển 3/4 thành số thập phân, ta lấy 3 chia 4 = 0.75"
ImageUrl: "/uploads/essays/abc123.jpg"
Score: null (chưa chấm)
Feedback: null
SubmittedDate: 2026-01-05 14:30:00
GradedDate: null
```

**Ví dụ - Đã chấm:**
```
EssayAnswerId: 1
ExerciseResultId: 15
QuestionId: 102
AnswerText: "Để chuyển 3/4 thành số thập phân..."
Score: 9.5
Feedback: "Bài làm rất tốt! Giải thích rõ ràng các bước."
SubmittedDate: 2026-01-05 14:30:00
GradedDate: 2026-01-05 16:45:00
GradedByAdminId: 1
```

---

### 11. GameContents (Nội dung trò chơi)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| GameContentId | int | ID tự động tăng |
| LessonId | int | Thuộc bài học nào |
| CardQuestion | string | Thẻ câu hỏi |
| CardAnswer | string | Thẻ đáp án |
| OrderIndex | int | Thứ tự hiển thị |
| CreatedDate | DateTime | Ngày tạo |

**Ví dụ:**
```
LessonId: 10
CardQuestion: "1 dm"
CardAnswer: "10 cm"
```

---

### 11. Notifications (Thông báo)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| NotificationId | int | ID tự động tăng |
| StudentId | int | Gửi cho học sinh nào |
| Title | string | Tiêu đề |
| Message | string | Nội dung |
| Type | int | Loại (0=Thông thường, 1=Thành tích...) |
| IsRead | bool | Đã đọc chưa |
| CreatedDate | DateTime | Ngày tạo |

---

### 12. ActivityLogs (Lịch sử hoạt động)

| Cột | Kiểu | Mô tả |
|-----|------|-------|
| LogId | int | ID tự động tăng |
| UserId | string | User nào |
| Action | string | Hành động gì |
| Description | string | Mô tả chi tiết |
| IpAddress | string | Địa chỉ IP |
| Timestamp | DateTime | Thời gian |

**Ví dụ:**
```
UserId: "a1b2c3d4..."
Action: "Login"
Description: "Đăng nhập thành công"
IpAddress: "192.168.1.100"
```

---

## 🔗 RELATIONSHIPS (QUAN HỆ)

### Mối quan hệ giữa các bảng

```
┌──────────────────────────────────────────────────────────┐
│                  DATABASE SCHEMA                         │
└──────────────────────────────────────────────────────────┘

AspNetUsers (1) ──────< (1) Student
                │
                └──────< (1) Admin

Student (1) ──────< (N) StudentProgress
        (1) ──────< (N) ExerciseResult
        (1) ──────< (N) Notification

Lesson (1) ──────< (N) Exercise
       (1) ──────< (N) StudentProgress
       (1) ──────< (N) GameContent

Exercise (1) ──────< (N) Question
         (1) ──────< (N) ExerciseResult

ExerciseResult (1) ──────< (N) EssayAnswer  ← NEW!

Question (1) ──────< (N) Answer (Trắc nghiệm)
         (1) ──────< (N) EssayAnswer (Tự luận)  ← NEW!
```

### Delete Behaviors (Hành vi xóa)

#### Cascade Delete:
```
Lesson → Exercise → Question → Answer
       → StudentProgress
       → GameContent
```
**Nghĩa là:** Xóa Lesson → tự động xóa tất cả Exercise, Question, Answer liên quan

#### Restrict Delete:
```
Exercise ←→ ExerciseResult (RESTRICT)
Student ←→ ExerciseResult (RESTRICT)
```
**Nghĩa là:** Không thể xóa Exercise nếu có ExerciseResult (giữ điểm học sinh)

#### Soft Delete (NEW!):
```
Lesson.IsDeleted = true
```
**Nghĩa là:** Đánh dấu xóa, không xóa hẳn → Giữ điểm học sinh!

### Giải thích các ký hiệu:

- **(1)** = One (Một)
- **(N)** = Many (Nhiều)
- **<** = "có nhiều"

**Ví dụ:**
- `Lesson (1) ──────< (N) Exercise` = Một bài học có nhiều bài tập
- `Question (1) ──────< (N) Answer` = Một câu hỏi có nhiều đáp án

---

## 💾 CÁCH DỮ LIỆU ĐƯỢC LƯU

### 1. Khi học sinh đăng ký tài khoản

```csharp
// 1. Tạo ApplicationUser (trong AspNetUsers)
var user = new ApplicationUser
{
    Id = Guid.NewGuid().ToString(),
    UserName = "nguyenvana",
    Email = "nguyenvana@gmail.com",
    PasswordHash = HashedPassword
};
await _userManager.CreateAsync(user, password);

// 2. Tạo Student (liên kết với User)
var student = new Student
{
    UserId = user.Id,  // Foreign Key
    FullName = "Nguyễn Văn A",
    StudentCode = "HS001",
    Grade = 5,
    TotalPoints = 0
};
_context.Students.Add(student);
await _context.SaveChangesAsync();
```

**Kết quả trong Database:**

**Bảng AspNetUsers:**
```
| Id (PK)        | UserName   | Email              | PasswordHash |
|----------------|------------|--------------------|--------------|
| a1b2c3d4-...   | nguyenvana | nguyenvana@gmail   | $2a$11$...  |
```

**Bảng Students:**
```
| StudentId (PK) | UserId (FK)  | FullName     | Grade | TotalPoints |
|----------------|--------------|--------------|-------|-------------|
| 1              | a1b2c3d4-... | Nguyễn Văn A | 5     | 0           |
```

---

### 2. Khi admin tạo bài giảng

```csharp
var lesson = new Lesson
{
    Title = "Khái niệm số thập phân",
    Description = "Giới thiệu về số thập phân",
    Grade = 5,
    Topic = "Số học",
    VideoUrl = "https://youtube.com/watch?v=...",
    VideoDuration = 600,
    OrderIndex = 1,
    IsPublished = true,
    CreatedDate = DateTime.Now
};

_context.Lessons.Add(lesson);
await _context.SaveChangesAsync();
```

**Kết quả:**

**Bảng Lessons:**
```
| LessonId | Title              | Grade | VideoUrl        | Duration | IsPublished |
|----------|--------------------|-------|-----------------|----------|-------------|
| 10       | Số thập phân       | 5     | youtube.com/... | 600      | true        |
```

---

### 3. Khi admin tạo bài tập cho bài giảng

```csharp
// Tạo Exercise
var exercise = new Exercise
{
    LessonId = 10,  // Thuộc bài học "Số thập phân"
    Title = "Bài tập số thập phân",
    Difficulty = 0,
    TimeLimit = 15,
    PassingScore = 70
};
_context.Exercises.Add(exercise);
await _context.SaveChangesAsync();

// Tạo Question
var question = new Question
{
    ExerciseId = exercise.ExerciseId,
    QuestionText = "Số nào sau đây là số thập phân?",
    Points = 10
};
_context.Questions.Add(question);
await _context.SaveChangesAsync();

// Tạo Answers
var answers = new List<Answer>
{
    new Answer { QuestionId = question.QuestionId, AnswerText = "3/5", IsCorrect = false },
    new Answer { QuestionId = question.QuestionId, AnswerText = "2,4", IsCorrect = true },
    new Answer { QuestionId = question.QuestionId, AnswerText = "7", IsCorrect = false },
    new Answer { QuestionId = question.QuestionId, AnswerText = "5/10", IsCorrect = false }
};
_context.Answers.AddRange(answers);
await _context.SaveChangesAsync();
```

**Kết quả:**

**Bảng Exercises:**
```
| ExerciseId | LessonId | Title                | Difficulty | PassingScore |
|------------|----------|----------------------|------------|--------------|
| 5          | 10       | BT số thập phân      | 0          | 70           |
```

**Bảng Questions:**
```
| QuestionId | ExerciseId | QuestionText                      | Points |
|------------|------------|-----------------------------------|--------|
| 101        | 5          | Số nào là số thập phân?           | 10     |
```

**Bảng Answers:**
```
| AnswerId | QuestionId | AnswerText | IsCorrect |
|----------|------------|------------|-----------|
| 401      | 101        | 3/5        | false     |
| 402      | 101        | 2,4        | true      | ← Đáp án đúng
| 403      | 101        | 7          | false     |
| 404      | 101        | 5/10       | false     |
```

---

### 4. Khi học sinh xem video bài giảng

```csharp
// 1. Tìm hoặc tạo Progress
var progress = await _context.StudentProgress
    .FirstOrDefaultAsync(sp => sp.StudentId == 1 && sp.LessonId == 10);

if (progress == null)
{
    progress = new StudentProgress
    {
        StudentId = 1,
        LessonId = 10,
        Status = 1,  // Đang học
        CompletionPercentage = 0
    };
    _context.StudentProgress.Add(progress);
}

// 2. Cập nhật tiến độ (gọi từ JavaScript mỗi 10 giây)
progress.VideoWatchedSeconds = 300;  // Đã xem 5 phút
progress.CompletionPercentage = (300 * 100) / 600;  // 50%
progress.LastAccessedDate = DateTime.Now;

await _context.SaveChangesAsync();
```

**Kết quả:**

**Bảng StudentProgress:**
```
| ProgressId | StudentId | LessonId | Status | Completion% | WatchedSec | LastAccessed |
|------------|-----------|----------|--------|-------------|------------|--------------|
| 1          | 1         | 10       | 1      | 50          | 300        | 2026-01-05   |
```

---

### 5. Khi học sinh làm bài tập

```csharp
// 1. Nhận đáp án từ form
var answers = new Dictionary<int, int>
{
    { 101, 402 },  // Câu 101 chọn đáp án 402
    { 102, 407 },
    // ...
};

// 2. Chấm điểm
int correctCount = 0;
foreach (var answer in answers)
{
    var correctAnswer = await _context.Answers
        .FirstOrDefaultAsync(a => a.QuestionId == answer.Key && a.IsCorrect);
    
    if (correctAnswer?.AnswerId == answer.Value)
        correctCount++;
}

int score = (correctCount * 100) / totalQuestions;

// 3. Lưu kết quả
var result = new ExerciseResult
{
    StudentId = 1,
    ExerciseId = 5,
    Score = score,
    CorrectAnswers = correctCount,
    TotalQuestions = totalQuestions,
    IsPassed = score >= 70,
    CompletedDate = DateTime.Now,
    Answers = JsonSerializer.Serialize(answers)
};

_context.ExerciseResults.Add(result);

// 4. Cộng điểm cho học sinh
var student = await _context.Students.FindAsync(1);
student.TotalPoints += score;

// 5. Cập nhật tiến độ bài học
var progress = await _context.StudentProgress
    .FirstOrDefaultAsync(sp => sp.StudentId == 1 && sp.LessonId == 10);
progress.HighestScore = Math.Max(progress.HighestScore, score);
progress.Status = 2;  // Hoàn thành
progress.CompletedDate = DateTime.Now;

await _context.SaveChangesAsync();
```

**Kết quả:**

**Bảng ExerciseResults:**
```
| ResultId | StudentId | ExerciseId | Score | Correct | Total | IsPassed | Answers        |
|----------|-----------|------------|-------|---------|-------|----------|----------------|
| 1        | 1         | 5          | 90    | 9       | 10    | true     | {"101":402,..} |
```

**Bảng Students (updated):**
```
| StudentId | FullName     | TotalPoints |
|-----------|--------------|-------------|
| 1         | Nguyễn Văn A | 90          | ← Tăng từ 0 lên 90
```

**Bảng StudentProgress (updated):**
```
| ProgressId | StudentId | LessonId | Status | HighestScore | CompletedDate |
|------------|-----------|----------|--------|--------------|---------------|
| 1          | 1         | 10       | 2      | 90           | 2026-01-05    |
```

---

## 🔄 MIGRATIONS

### Migrations là gì?

**Migrations** = "Phiên bản" của Database Schema. Mỗi khi thay đổi cấu trúc (thêm bảng, sửa cột), tạo 1 migration mới.

### Ví dụ thực tế:

#### Ban đầu: Có 2 bảng

```
Students: StudentId, FullName, Grade
Lessons: LessonId, Title
```

#### Thêm tính năng mới: Trò chơi

→ Cần thêm bảng `GameContents`

**Bước 1: Tạo Migration**
```powershell
dotnet ef migrations add AddGameContent
```

Tạo file `20260105_AddGameContent.cs`:
```csharp
public partial class AddGameContent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "GameContents",
            columns: table => new
            {
                GameContentId = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                LessonId = table.Column<int>(type: "INTEGER", nullable: false),
                CardQuestion = table.Column<string>(type: "TEXT", nullable: false),
                CardAnswer = table.Column<string>(type: "TEXT", nullable: false),
                // ...
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameContents", x => x.GameContentId);
                table.ForeignKey(
                    name: "FK_GameContents_Lessons_LessonId",
                    column: x => x.LessonId,
                    principalTable: "Lessons",
                    principalColumn: "LessonId",
                    onDelete: ReferentialAction.Cascade);
            });
    }
}
```

**Bước 2: Áp dụng Migration**
```powershell
dotnet ef database update
```

→ Tạo bảng `GameContents` trong database

---

## 🛠️ ENTITY FRAMEWORK CORE

### DbContext - "Cổng giao tiếp" với Database

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    // DbSet = Bảng trong database
    public DbSet<Student> Students { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    // ...
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cấu hình relationships, constraints, indexes
    }
}
```

### CRUD Operations

#### CREATE (Tạo mới)
```csharp
var student = new Student { FullName = "Nguyễn Văn A", Grade = 5 };
_context.Students.Add(student);
await _context.SaveChangesAsync();
```

#### READ (Đọc)
```csharp
// Lấy tất cả
var students = await _context.Students.ToListAsync();

// Lấy theo điều kiện
var grade5 = await _context.Students.Where(s => s.Grade == 5).ToListAsync();

// Lấy 1 record
var student = await _context.Students.FindAsync(1);

// Join bảng
var data = await _context.Students
    .Include(s => s.Progress)
    .Include(s => s.ExerciseResults)
    .FirstOrDefaultAsync(s => s.StudentId == 1);
```

#### UPDATE (Cập nhật)
```csharp
var student = await _context.Students.FindAsync(1);
student.TotalPoints += 10;
await _context.SaveChangesAsync();
```

#### DELETE (Xóa)
```csharp
var student = await _context.Students.FindAsync(1);
_context.Students.Remove(student);
await _context.SaveChangesAsync();
```

---

## 📖 VÍ DỤ THỰC TẾ

### Luồng hoàn chỉnh: Học sinh làm bài tập

```
┌─────────────────────────────────────────────────────────┐
│ 1. Student truy cập trang "Làm bài tập"                │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 2. Controller lấy dữ liệu từ Database                   │
│                                                         │
│    var exercise = await _context.Exercises              │
│        .Include(e => e.Questions)                       │
│            .ThenInclude(q => q.Answers)                 │
│        .FirstOrDefaultAsync(e => e.ExerciseId == 5);    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 3. Database trả về:                                     │
│                                                         │
│    Exercise {                                           │
│        ExerciseId: 5,                                   │
│        Questions: [                                     │
│            Question {                                   │
│                QuestionId: 101,                         │
│                QuestionText: "Số nào là số thập phân?", │
│                Answers: [                               │
│                    { AnswerId: 401, Text: "3/5", IsCorrect: false },│
│                    { AnswerId: 402, Text: "2,4", IsCorrect: true },│
│                    ...                                  │
│                ]                                        │
│            },                                           │
│            ...                                          │
│        ]                                                │
│    }                                                    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 4. View hiển thị câu hỏi + đáp án                       │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 5. Student chọn đáp án và submit                        │
│    {                                                    │
│        "101": 402,  ← Câu 101 chọn đáp án 402          │
│        "102": 407,                                      │
│        ...                                              │
│    }                                                    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 6. Controller chấm điểm                                 │
│                                                         │
│    foreach (var answer in submittedAnswers)             │
│    {                                                    │
│        var correct = await _context.Answers             │
│            .FirstOrDefaultAsync(a =>                    │
│                a.QuestionId == answer.Key &&            │
│                a.IsCorrect);                            │
│                                                         │
│        if (correct.AnswerId == answer.Value)            │
│            correctCount++;                              │
│    }                                                    │
│                                                         │
│    score = (correctCount * 100) / totalQuestions;       │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 7. Lưu kết quả vào Database                             │
│                                                         │
│    INSERT INTO ExerciseResults (                        │
│        StudentId, ExerciseId, Score, ...                │
│    ) VALUES (1, 5, 90, ...)                             │
│                                                         │
│    UPDATE Students                                      │
│    SET TotalPoints = TotalPoints + 90                   │
│    WHERE StudentId = 1                                  │
│                                                         │
│    UPDATE StudentProgress                               │
│    SET Status = 2, HighestScore = 90                    │
│    WHERE StudentId = 1 AND LessonId = 10                │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 8. Hiển thị kết quả cho Student                         │
│    "Bạn đạt 90 điểm! Chúc mừng!"                        │
└─────────────────────────────────────────────────────────┘
```

---

### Luồng chấm điểm bài tự luận (NEW!)

```
┌─────────────────────────────────────────────────────────┐
│ 1. Student làm bài có câu tự luận                       │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 2. Student nhập câu trả lời văn bản hoặc upload ảnh     │
│    AnswerText: "Để chuyển 3/4 thành số thập phân..."   │
│    ImageUrl: "/uploads/essays/abc123.jpg"              │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 3. Submit bài → Tạo ExerciseResult                      │
│                                                         │
│    INSERT INTO ExerciseResults (                        │
│        StudentId: 1,                                    │
│        ExerciseId: 6,                                   │
│        Score: 7.5,  ← Điểm tạm từ phần trắc nghiệm     │
│        GradingStatus: 1,  ← PendingGrading             │
│        IsPassed: false  ← Chưa chấm xong               │
│    )                                                    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 4. Lưu câu trả lời tự luận                              │
│                                                         │
│    INSERT INTO EssayAnswers (                           │
│        ExerciseResultId: 15,                            │
│        QuestionId: 102,                                 │
│        AnswerText: "...",                               │
│        ImageUrl: "...",                                 │
│        Score: null,  ← Chưa chấm                        │
│        SubmittedDate: NOW()                             │
│    )                                                    │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 5. Notification: "Đang chờ giáo viên chấm"              │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 6. Admin vào ViewExerciseResult, xem câu trả lời        │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 7. Admin chấm điểm: Score = 9.5, Feedback = "Tốt!"     │
│                                                         │
│    UPDATE EssayAnswers                                  │
│    SET Score = 9.5,                                     │
│        Feedback = "Bài làm rất tốt!",                   │
│        GradedDate = NOW(),                              │
│        GradedByAdminId = 1                              │
│    WHERE EssayAnswerId = 1                              │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 8. Tính điểm tổng kết (MC + Essay)                      │
│                                                         │
│    finalScore = (7.5 * 3 + 9.5 * 1) / 4 = 8.125        │
│    (3 câu MC điểm 7.5, 1 câu Essay điểm 9.5)           │
│                                                         │
│    UPDATE ExerciseResults                               │
│    SET Score = 8.1,                                     │
│        GradingStatus = 0,  ← Graded                     │
│        IsPassed = true     ← Pass (>= 7.0)             │
│    WHERE ResultId = 15                                  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 9. Cập nhật TotalPoints cho Student                     │
│                                                         │
│    UPDATE Students                                      │
│    SET TotalPoints = TotalPoints + 8.1                  │
│    WHERE StudentId = 1                                  │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 10. Gửi Notification: "Bài đã được chấm: 8.1/10"       │
│     Unlock bài tiếp theo (nếu Pass)                     │
└─────────────────────────────────────────────────────────┘
```

---

### Soft Delete Workflow (NEW!)

```
┌─────────────────────────────────────────────────────────┐
│ 1. Admin click "Xóa bài giảng"                          │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 2. KHÔNG xóa hẳn, chỉ đánh dấu                          │
│                                                         │
│    UPDATE Lessons                                       │
│    SET IsDeleted = 1,                                   │
│        UpdatedDate = NOW()                              │
│    WHERE LessonId = 10                                  │
│                                                         │
│    ✅ Exercises vẫn còn                                 │
│    ✅ ExerciseResults vẫn còn                           │
│    ✅ Điểm học sinh KHÔNG mất                           │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ 3. Query chỉ lấy bài chưa xóa                           │
│                                                         │
│    SELECT * FROM Lessons                                │
│    WHERE IsDeleted = 0  ← Lọc bài đã xóa               │
│                                                         │
│    → User không thấy bài đã xóa                         │
│    → Database vẫn giữ nguyên data                       │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 TÓM TẮT

### Database hoạt động như thế nào?

1. **Code C# → EF Core → SQL → SQLite**
   - Bạn viết code C# (LINQ)
   - EF Core tự động chuyển thành SQL
   - SQLite thực thi SQL và lưu vào file `.db`

2. **Dữ liệu được lưu dạng bảng**
   - Mỗi bảng = 1 thực thể (Student, Lesson, Exercise...)
   - Các bảng liên kết với nhau qua Foreign Keys

3. **Migrations quản lý phiên bản schema**
   - Mỗi thay đổi = 1 migration
   - Có thể rollback về phiên bản cũ

4. **Relationships đảm bảo tính toàn vẹn**
   - Cascade Delete: Xóa Lesson → Tự động xóa Exercise liên quan
   - Foreign Key: Đảm bảo StudentId phải tồn tại trong bảng Students

---

---

## 📊 Thống kê Database

### Bảng dữ liệu:
- ✅ **13 bảng chính** (AspNetUsers, Students, Admins, Lessons, Exercises, Questions, Answers, ExerciseResults, EssayAnswers, StudentProgress, GameContents, Notifications, ActivityLogs)
- ✅ **7+ bảng Identity** (AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.)

### Migrations:
- ✅ `InitialCreate` - Setup ban đầu
- ✅ `AddGameContent` - Thêm game
- ✅ `AddPdfAndEssaySupport` - Hỗ trợ PDF và tự luận
- ✅ `AddImageUrlToEssayAnswer` - Thêm ảnh cho câu trả lời
- ✅ `AddGradingStatusToExerciseResult` - Trạng thái chấm điểm
- ✅ `AddIsDeletedToLesson` - Soft delete cho Lesson

### Tính năng nổi bật:
1. ✅ **Essay Grading** - Hệ thống chấm điểm tự luận
2. ✅ **Soft Delete** - Xóa an toàn, giữ điểm học sinh
3. ✅ **Score Calculation** - Tính điểm chính xác (MC + Essay)
4. ✅ **Real-time Notifications** - Thông báo tức thì
5. ✅ **Audit Trail** - Theo dõi lịch sử thay đổi

---

**Ngày cập nhật:** 06/01/2026  
**Tác giả:** MathUniverse Dev Team  
**Phiên bản:** 2.0  
**Tính năng mới:** Essay Grading, Soft Delete, Enhanced Score System

