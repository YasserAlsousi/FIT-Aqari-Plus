# دليل النشر - Deployment Guide

## متطلبات النشر

### الخادم
- Windows Server 2019+ أو Linux (Ubuntu 20.04+)
- .NET 8 Runtime
- SQL Server 2019+ أو SQL Server Express
- IIS (للـ Windows) أو Nginx (للـ Linux)

### قاعدة البيانات
- SQL Server 2019+
- حد أدنى 2GB مساحة فارغة
- صلاحيات إنشاء قواعد البيانات

## خطوات النشر

### 1. إعداد قاعدة البيانات

```sql
-- إنشاء قاعدة البيانات
CREATE DATABASE PropertyManagementDb;

-- إنشاء مستخدم للتطبيق
CREATE LOGIN PropertyManagementUser WITH PASSWORD = 'YourStrongPassword123!';
USE PropertyManagementDb;
CREATE USER PropertyManagementUser FOR LOGIN PropertyManagementUser;
ALTER ROLE db_owner ADD MEMBER PropertyManagementUser;
```

### 2. تحديث إعدادات الاتصال

في ملف `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YourServerName;Database=PropertyManagementDb;User Id=PropertyManagementUser;Password=YourStrongPassword123!;TrustServerCertificate=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### 3. بناء التطبيق

```bash
# بناء API
cd src/PropertyManagement.API
dotnet publish -c Release -o ./publish

# بناء Web App
cd ../PropertyManagement.Web
dotnet publish -c Release -o ./publish
```

### 4. نشر على IIS (Windows)

1. تثبيت .NET 8 Hosting Bundle
2. إنشاء موقع جديد في IIS
3. نسخ ملفات التطبيق إلى مجلد الموقع
4. تعيين Application Pool إلى "No Managed Code"
5. إعطاء صلاحيات للمجلد

### 5. نشر على Linux (Ubuntu)

```bash
# تثبيت .NET Runtime
wget https://packages.microsoft.com/