@echo off
echo ========================================
echo    نظام إدارة العقارات
echo    Property Management System
echo ========================================
echo.

echo [1/3] Building the project...
dotnet build
if %errorlevel% neq 0 (
    echo Error: Build failed!
    pause
    exit /b 1
)

echo.
echo [2/3] Navigating to Web project...
cd src\PropertyManagement.Web

echo.
echo [3/3] Starting the application...
echo.
echo The application will be available at:
echo https://localhost:5001
echo http://localhost:5000
echo.
echo Press Ctrl+C to stop the application
echo.

dotnet run

pause
