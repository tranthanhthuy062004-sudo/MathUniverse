-- Script SQL để xóa tài khoản abc@gmail.com
-- Chạy bằng DB Browser for SQLite hoặc sqlite3 command line

-- 1. Kiểm tra tài khoản trước khi xóa
SELECT 'Tài khoản hiện tại:' as Info;
SELECT Id, Email, UserName FROM AspNetUsers WHERE Email = 'abc@gmail.com';

-- 2. Kiểm tra Student ID
SELECT 'Student ID:' as Info;
SELECT StudentId, UserId FROM Students WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = 'abc@gmail.com');

-- 3. Xóa dữ liệu liên quan (theo thứ tự để tránh foreign key constraint)

-- Bước 1: Xóa ExerciseResults
DELETE FROM ExerciseResults 
WHERE StudentId IN (
    SELECT s.StudentId FROM Students s
    INNER JOIN AspNetUsers u ON s.UserId = u.Id
    WHERE u.Email = 'abc@gmail.com'
);

-- Bước 2: Xóa StudentProgress
DELETE FROM StudentProgress
WHERE StudentId IN (
    SELECT s.StudentId FROM Students s
    INNER JOIN AspNetUsers u ON s.UserId = u.Id
    WHERE u.Email = 'abc@gmail.com'
);

-- Bước 3: Xóa Notifications
DELETE FROM Notifications
WHERE StudentId IN (
    SELECT s.StudentId FROM Students s
    INNER JOIN AspNetUsers u ON s.UserId = u.Id
    WHERE u.Email = 'abc@gmail.com'
);

-- Bước 4: Xóa Students
DELETE FROM Students
WHERE UserId IN (
    SELECT Id FROM AspNetUsers WHERE Email = 'abc@gmail.com'
);

-- Bước 5: Xóa ActivityLogs
DELETE FROM ActivityLogs
WHERE UserId IN (
    SELECT CAST(Id AS TEXT) FROM AspNetUsers WHERE Email = 'abc@gmail.com'
);

-- Bước 6: Xóa AspNetUserRoles
DELETE FROM AspNetUserRoles
WHERE UserId IN (
    SELECT Id FROM AspNetUsers WHERE Email = 'abc@gmail.com'
);

-- Bước 7: Xóa AspNetUsers (cuối cùng)
DELETE FROM AspNetUsers
WHERE Email = 'abc@gmail.com';

-- 4. Kiểm tra kết quả
SELECT 'Kiểm tra sau khi xóa:' as Info;
SELECT COUNT(*) as 'Số tài khoản còn lại' FROM AspNetUsers WHERE Email = 'abc@gmail.com';

-- Nếu kết quả = 0 thì đã xóa thành công

