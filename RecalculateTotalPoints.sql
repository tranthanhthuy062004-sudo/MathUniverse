-- Script to recalculate TotalPoints for all existing students
-- This script should be run after deploying the fix

-- Create a temporary table to hold the corrected scores
CREATE TEMP TABLE IF NOT EXISTS temp_scores AS
SELECT 
    er.StudentId,
    SUM(highest_scores.HighestScore) as NewTotalPoints
FROM (
    SELECT 
        StudentId,
        ExerciseId,
        MAX(Score) as HighestScore
    FROM ExerciseResults
    WHERE GradingStatus = 0  -- Graded status
    GROUP BY StudentId, ExerciseId
) as highest_scores
GROUP BY StudentId;

-- Update Students table with recalculated points
UPDATE Students
SET TotalPoints = (
    SELECT CAST(ROUND(ts.NewTotalPoints) AS INTEGER)
    FROM temp_scores ts
    WHERE ts.StudentId = Students.StudentId
)
WHERE StudentId IN (SELECT StudentId FROM temp_scores);

-- Verify the update
SELECT 
    s.StudentId,
    s.StudentCode,
    s.FullName,
    s.TotalPoints as NewTotalPoints,
    COUNT(DISTINCT er.ExerciseId) as ExercisesCompleted,
    SUM(CASE WHEN er.GradingStatus = 0 THEN 1 ELSE 0 END) as GradedExercises,
    SUM(CASE WHEN er.GradingStatus = 1 THEN 1 ELSE 0 END) as PendingExercises
FROM Students s
LEFT JOIN ExerciseResults er ON s.StudentId = er.StudentId
GROUP BY s.StudentId, s.StudentCode, s.FullName, s.TotalPoints
ORDER BY s.TotalPoints DESC;

-- Drop temporary table
DROP TABLE IF EXISTS temp_scores;

