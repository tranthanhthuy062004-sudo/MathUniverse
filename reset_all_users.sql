-- Script SQL để xóa TẤT CẢ tài khoản và tạo lại admin mới
-- CẢNH BÁO: Script này sẽ xóa toàn bộ users, admins, students
-- Dữ liệu bài học (Lessons, Exercises, Questions) sẽ được giữ nguyên

-- 1. Kiểm tra trước khi xóa
SELECT 'Danh sách users hiện tại:' as Info;
SELECT Id, Email, UserName FROM AspNetUsers;

-- 2. Xóa tất cả dữ liệu người dùng (KHÔNG XÓA BÀI HỌC)

-- Bước 1: Xóa EssayAnswers
DELETE FROM EssayAnswers;

-- Bước 2: Xóa ExerciseResults
DELETE FROM ExerciseResults;

-- Bước 3: Xóa StudentProgress
DELETE FROM StudentProgress;

-- Bước 4: Xóa Notifications
DELETE FROM Notifications;

-- Bước 5: Xóa Students
DELETE FROM Students;

-- Bước 6: Xóa Admins
DELETE FROM Admins;

-- Bước 7: Xóa ActivityLogs
DELETE FROM ActivityLogs;

-- Bước 8: Xóa AspNetUserRoles
DELETE FROM AspNetUserRoles;

-- Bước 9: Xóa AspNetUserTokens
DELETE FROM AspNetUserTokens;

-- Bước 10: Xóa AspNetUserLogins
DELETE FROM AspNetUserLogins;

-- Bước 11: Xóa AspNetUserClaims
DELETE FROM AspNetUserClaims;

-- Bước 12: Xóa tất cả AspNetUsers
DELETE FROM AspNetUsers;

-- 3. Tạo tài khoản admin mới
-- Email: admin@mathuniverse.com
-- Password: Admin@123
-- (Hash của mật khẩu Admin@123)

-- Tạo user mới (ID sẽ tự động tăng)
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

-- Lấy ID của user vừa tạo
-- SQLite sử dụng last_insert_rowid()

-- Tạo profile Admin
INSERT INTO Admins (UserId, Department, CreatedDate)
SELECT Id, 'Quản trị hệ thống', datetime('now')
FROM AspNetUsers 
WHERE Email = 'admin@mathuniverse.com';

-- Gán vai trò Admin cho user
-- Role "Admin" có RoleId = 1 (từ DbInitializer)
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, 1
FROM AspNetUsers 
WHERE Email = 'admin@mathuniverse.com';

-- 4. Kiểm tra kết quả
SELECT 'Kết quả sau khi tạo lại:' as Info;
SELECT Id, Email, UserName, FullName FROM AspNetUsers;

SELECT 'Admin profile:' as Info;
SELECT * FROM Admins;

SELECT 'User roles:' as Info;
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- 5. Kiểm tra bài học còn nguyên
SELECT 'Số lượng bài học:' as Info;
SELECT COUNT(*) as TotalLessons FROM Lessons;

SELECT 'Số lượng bài tập:' as Info;
SELECT COUNT(*) as TotalExercises FROM Exercises;

-- HOÀN TẤT
-- Tài khoản mới:
-- Email: admin@mathuniverse.com
-- Password: Admin@123

