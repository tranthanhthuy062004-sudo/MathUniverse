-- Script để kiểm tra và xóa tất cả user (trừ admin)
-- Chạy script này bằng SQLite browser hoặc command line

-- 1. Kiểm tra tất cả users hiện tại
SELECT Id, Email, UserName FROM AspNetUsers;

-- 2. Kiểm tra vai trò của users
SELECT u.Email, r.Name as Role
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id;

-- 3. Xóa tất cả students và dữ liệu liên quan (CẢNH BÁO: KHÔNG THỂ HOÀN TÁC)
-- Bước 1: Xóa ExerciseResults
DELETE FROM ExerciseResults 
WHERE StudentId IN (SELECT StudentId FROM Students WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
));

-- Bước 2: Xóa StudentProgress
DELETE FROM StudentProgress
WHERE StudentId IN (SELECT StudentId FROM Students WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
));

-- Bước 3: Xóa Notifications
DELETE FROM Notifications
WHERE StudentId IN (SELECT StudentId FROM Students WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
));

-- Bước 4: Xóa Students
DELETE FROM Students
WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
);

-- Bước 5: Xóa ActivityLogs
DELETE FROM ActivityLogs
WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
);

-- Bước 6: Xóa AspNetUserRoles (student roles)
DELETE FROM AspNetUserRoles
WHERE UserId IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
);

-- Bước 7: Xóa AspNetUsers (students)
DELETE FROM AspNetUsers
WHERE Id IN (
    SELECT u.Id FROM AspNetUsers u
    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
    WHERE r.Name != 'Admin' OR r.Name IS NULL
);

-- 4. Kiểm tra lại sau khi xóa
SELECT Id, Email, UserName FROM AspNetUsers;

