# نظام إدارة العقارات - Property Management System

نظام شامل لإدارة العقارات يدعم اللغة العربية والإنجليزية والفرنسية، مبني باستخدام .NET 8 و Entity Framework Core.

## المميزات

- 🏠 إدارة العقارات والملاك
- 📋 إدارة العقود والمستأجرين
- 💰 نظام المدفوعات والفواتير
- 🔧 إدارة طلبات الصيانة
- 📊 تقارير مالية وإحصائيات
- 🌐 دعم متعدد اللغات (عربي/إنجليزي/فرنسي)
- 📱 تصميم متجاوب

## التقنيات المستخدمة

- **Backend**: .NET 8, ASP.NET Core Web API
- **Frontend**: ASP.NET Core MVC, Bootstrap 5
- **Database**: SQL Server, Entity Framework Core
- **UI**: Bootstrap RTL, Font Awesome, Cairo Font

## هيكل المشروع

```
PropertyManagement/
├── src/
│   ├── PropertyManagement.Core/          # الكيانات والواجهات الأساسية
│   ├── PropertyManagement.Infrastructure/ # قاعدة البيانات والخدمات
│   ├── PropertyManagement.API/           # Web API
│   └── PropertyManagement.Web/           # واجهة الويب
├── docs/                                 # الوثائق
└── tests/                               # الاختبارات
```

## التشغيل

### المتطلبات
- .NET 8 SDK
- SQL Server (LocalDB أو Express)
- Visual Studio 2022 أو VS Code

### خطوات التشغيل

1. **استنساخ المشروع**
```bash
git clone https://github.com/your-repo/property-management.git
cd property-management
```

2. **تحديث قاعدة البيانات**
```bash
cd src/PropertyManagement.Infrastructure
dotnet ef database update
```

3. **تشغيل API**
```bash
cd src/PropertyManagement.API
dotnet run
```

4. **تشغيل واجهة الويب**
```bash
cd src/PropertyManagement.Web
dotnet run
```

## API Documentation

الـ API متاح على: `https://localhost:7001/swagger`

### نقاط النهاية الرئيسية

- `GET /api/v1/properties` - قائمة العقارات
- `POST /api/v1/properties` - إضافة عقار جديد
- `GET /api/v1/contracts` - قائمة العقود
- `GET /api/v1/payments` - قائمة المدفوعات

## المساهمة

نرحب بالمساهمات! يرجى قراءة [دليل المساهمة](CONTRIBUTING.md) قبل البدء.

## الترخيص

هذا المشروع مرخص تحت رخصة MIT - راجع ملف [LICENSE](LICENSE) للتفاصيل.