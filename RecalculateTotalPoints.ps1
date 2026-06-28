# PowerShell script to recalculate TotalPoints for all students
# This uses the C# code directly through the application

Write-Host "=== Recalculating TotalPoints for All Students ===" -ForegroundColor Green
Write-Host ""

# Build the project first
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix errors first." -ForegroundColor Red
    exit 1
}

# Create a simple C# script to recalculate points
$scriptContent = @"
using Microsoft.EntityFrameworkCore;
using MathUniverse.Data;
using MathUniverse.Models;
using MathUniverse.Services;

var connectionString = "Data Source=MathUniverse.db";
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlite(connectionString)
    .Options;

using (var context = new ApplicationDbContext(options))
{
    var exerciseService = new ExerciseService(context);
    
    var students = await context.Students.ToListAsync();
    
    Console.WriteLine($"Found {students.Count} students to recalculate...");
    Console.WriteLine();
    
    foreach (var student in students)
    {
        var oldPoints = student.TotalPoints;
        
        // Recalculate using the updated logic
        await exerciseService.RecalculateStudentTotalPointsAsync(student.StudentId);
        
        // Reload student to get new points
        await context.Entry(student).ReloadAsync();
        
        Console.WriteLine($"{student.StudentCode} - {student.FullName}:");
        Console.WriteLine($"  Old Points: {oldPoints}");
        Console.WriteLine($"  New Points: {student.TotalPoints}");
        Console.WriteLine();
    }
    
    Console.WriteLine("Recalculation completed!");
}
"@

# Save the script
$scriptPath = "RecalculatePoints.csx"
$scriptContent | Out-File -FilePath $scriptPath -Encoding UTF8

Write-Host "Executing recalculation..." -ForegroundColor Yellow
Write-Host ""

# Run the script using dotnet-script
dotnet script $scriptPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "=== Recalculation completed successfully! ===" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "=== Recalculation failed! ===" -ForegroundColor Red
}

# Clean up
Remove-Item $scriptPath -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

