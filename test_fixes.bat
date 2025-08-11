@echo off
echo ========================================
echo اختبار الإصلاحات - نظام إدارة العقارات
echo ========================================
echo.

echo 1. بناء المشروع...
cd "src\PropertyManagement.Web"
dotnet build --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo خطأ في بناء المشروع!
    pause
    exit /b 1
)

echo.
echo ✅ تم بناء المشروع بنجاح!
echo.
echo 2. تشغيل النظام...
echo افتح المتصفح على: https://localhost:7001
echo.
echo الإصلاحات المطبقة:
echo ✅ إصلاح زر "إضافة صيانة جديد"
echo ✅ إصلاح صفحات التقارير (العقارات، المالي، الصيانة)
echo ✅ إصلاح مشكلة RTL التلقائي للعربية
echo ✅ إصلاح مشاكل ViewModels المكررة
echo.
echo اختبر الميزات التالية:
echo 1. اذهب لصفحة الصيانة واضغط "طلب صيانة جديد"
echo 2. اذهب لصفحة التقارير واختبر التقارير الثلاثة
echo 3. تأكد من ظهور النظام بـ RTL تلقائياً
echo.

dotnet run --configuration Release

pause
