# دليل المساهمة - Contributing Guide

مرحباً بك في مشروع **FIT Aqari Plus**! 🎉  
نحن نرحب بجميع المساهمات من المطورين والمصممين ومختبري البرمجيات.

## 🤝 كيفية المساهمة

### 📋 أنواع المساهمات المرحب بها

- 🐛 **إصلاح الأخطاء (Bug Fixes)**
- ✨ **إضافة ميزات جديدة (New Features)**
- 📚 **تحسين الوثائق (Documentation)**
- 🎨 **تحسين التصميم والواجهة (UI/UX)**
- 🔧 **تحسين الأداء (Performance)**
- 🧪 **إضافة اختبارات (Tests)**
- 🌐 **تحسين الترجمة والمحتوى العربي**

## 🚀 البدء السريع

### 1. Fork المشروع
```bash
# انقر على زر Fork في GitHub
# أو استخدم GitHub CLI
gh repo fork YasserAlsousi/FIT-Aqari-Plus
```

### 2. استنساخ المشروع محلياً
```bash
git clone https://github.com/YOUR_USERNAME/FIT-Aqari-Plus.git
cd FIT-Aqari-Plus
```

### 3. إعداد البيئة المحلية
```bash
# تثبيت التبعيات
dotnet restore

# إنشاء قاعدة البيانات
cd src/PropertyManagement.Web
dotnet ef database update

# تشغيل المشروع
dotnet run
```

### 4. إنشاء فرع جديد
```bash
git checkout -b feature/your-feature-name
# أو
git checkout -b fix/bug-description
```

## 📝 معايير الكود

### 🎯 إرشادات التطوير

#### **C# Code Style**
- استخدم **PascalCase** للـ Classes والـ Methods
- استخدم **camelCase** للـ Variables والـ Parameters
- استخدم **kebab-case** للـ CSS classes
- اتبع معايير **Microsoft C# Coding Conventions**

#### **تسمية الملفات**
```
Controllers/     -> PascalCase (e.g., PropertiesController.cs)
Models/         -> PascalCase (e.g., PropertyViewModel.cs)
Views/          -> PascalCase (e.g., Index.cshtml)
wwwroot/css/    -> kebab-case (e.g., site.css)
wwwroot/js/     -> kebab-case (e.g., site.js)
```

#### **التعليقات والوثائق**
```csharp
/// <summary>
/// يقوم بإنشاء عقار جديد في النظام
/// Creates a new property in the system
/// </summary>
/// <param name="property">بيانات العقار - Property data</param>
/// <returns>معرف العقار الجديد - New property ID</returns>
public async Task<int> CreatePropertyAsync(Property property)
{
    // التحقق من صحة البيانات
    // Validate input data
    if (property == null)
        throw new ArgumentNullException(nameof(property));
    
    // إضافة العقار لقاعدة البيانات
    // Add property to database
    _context.Properties.Add(property);
    await _context.SaveChangesAsync();
    
    return property.Id;
}
```

### 🌐 إرشادات اللغة العربية

#### **النصوص في الواجهة**
- استخدم العربية الفصحى المبسطة
- تجنب المصطلحات المعقدة
- استخدم مصطلحات موحدة عبر النظام

#### **اتجاه النص (RTL)**
```css
/* استخدم هذه الفئات للنصوص العربية */
.rtl-text {
    direction: rtl;
    text-align: right;
}

/* للنماذج والمدخلات */
.form-rtl input,
.form-rtl textarea {
    text-align: right;
    direction: rtl;
}
```

## 🧪 الاختبارات

### تشغيل الاختبارات
```bash
# تشغيل جميع الاختبارات
dotnet test

# تشغيل اختبارات مشروع معين
dotnet test src/PropertyManagement.Tests/

# تشغيل مع تقرير التغطية
dotnet test --collect:"XPlat Code Coverage"
```

### كتابة اختبارات جديدة
```csharp
[Test]
public async Task CreateProperty_ValidData_ReturnsPropertyId()
{
    // Arrange - الإعداد
    var property = new Property
    {
        Title = "شقة للإيجار",
        Address = "الرياض، السعودية",
        MonthlyRent = 2000
    };

    // Act - التنفيذ
    var result = await _propertyService.CreatePropertyAsync(property);

    // Assert - التحقق
    Assert.That(result, Is.GreaterThan(0));
}
```

## 📋 عملية المراجعة

### 1. قبل إرسال Pull Request

#### ✅ قائمة التحقق
- [ ] الكود يعمل بدون أخطاء
- [ ] تم تشغيل جميع الاختبارات بنجاح
- [ ] تم إضافة اختبارات للميزات الجديدة
- [ ] تم تحديث الوثائق إذا لزم الأمر
- [ ] تم اتباع معايير الكود المحددة
- [ ] تم اختبار الواجهة على أجهزة مختلفة

#### 📝 رسالة الـ Commit
```bash
# استخدم هذا التنسيق
git commit -m "نوع: وصف مختصر بالعربية

وصف تفصيلي للتغييرات المطبقة...

- إضافة ميزة البحث المتقدم
- إصلاح مشكلة في عرض التقارير
- تحسين أداء الاستعلامات

Closes #123"

# أمثلة على أنواع الـ commits
feat: إضافة ميزة جديدة
fix: إصلاح خطأ
docs: تحديث الوثائق
style: تحسين التصميم
refactor: إعادة هيكلة الكود
test: إضافة اختبارات
chore: مهام صيانة
```

### 2. إرسال Pull Request

#### 📋 قالب Pull Request
```markdown
## 📝 وصف التغييرات
وصف مختصر للتغييرات المطبقة...

## 🎯 نوع التغيير
- [ ] إصلاح خطأ (Bug fix)
- [ ] ميزة جديدة (New feature)
- [ ] تحسين الأداء (Performance improvement)
- [ ] تحديث الوثائق (Documentation update)

## 🧪 الاختبارات
- [ ] تم اختبار التغييرات محلياً
- [ ] تم إضافة اختبارات جديدة
- [ ] جميع الاختبارات تعمل بنجاح

## 📱 اختبار الواجهة
- [ ] تم الاختبار على الكمبيوتر
- [ ] تم الاختبار على الهاتف
- [ ] تم الاختبار على التابلت

## 📋 قائمة التحقق
- [ ] الكود يتبع معايير المشروع
- [ ] تم تحديث الوثائق
- [ ] لا توجد تحذيرات في الكود
```

## 🐛 الإبلاغ عن الأخطاء

### قالب تقرير الخطأ
```markdown
**🐛 وصف الخطأ**
وصف واضح ومختصر للخطأ...

**🔄 خطوات إعادة الإنتاج**
1. اذهب إلى '...'
2. انقر على '...'
3. قم بالتمرير إلى '...'
4. شاهد الخطأ

**✅ السلوك المتوقع**
وصف لما كان يجب أن يحدث...

**📱 البيئة**
- نظام التشغيل: [e.g. Windows 11]
- المتصفح: [e.g. Chrome 91]
- إصدار .NET: [e.g. 8.0]

**📸 لقطات الشاشة**
إضافة لقطات شاشة إذا أمكن...
```

## 💡 اقتراح ميزات جديدة

### قالب اقتراح الميزة
```markdown
**🎯 هل اقتراحك مرتبط بمشكلة؟**
وصف واضح للمشكلة... [e.g. أواجه صعوبة في...]

**💡 الحل المقترح**
وصف واضح لما تريد أن يحدث...

**🔄 البدائل المدروسة**
وصف أي حلول أو ميزات بديلة فكرت فيها...

**📋 معلومات إضافية**
أي سياق أو لقطات شاشة أخرى حول طلب الميزة...
```

## 🏆 المساهمون

نشكر جميع المساهمين في هذا المشروع! 🙏

### كيفية إضافة اسمك لقائمة المساهمين
بعد قبول Pull Request الخاص بك، سيتم إضافة اسمك تلقائياً لقائمة المساهمين.

## 📞 التواصل

### 💬 قنوات التواصل
- **GitHub Issues**: للأخطاء والاقتراحات
- **GitHub Discussions**: للنقاشات العامة
- **Email**: yasserprogramer@hotmail.com

### 🕐 أوقات الاستجابة
- **الأخطاء الحرجة**: خلال 24 ساعة
- **الميزات الجديدة**: خلال 3-5 أيام
- **الأسئلة العامة**: خلال أسبوع

## 📄 الترخيص

بمساهمتك في هذا المشروع، فإنك توافق على أن مساهماتك ستكون مرخصة تحت نفس ترخيص المشروع (MIT License).

## 🛠️ إعداد بيئة التطوير المتقدمة

### Visual Studio Code Extensions
```json
{
  "recommendations": [
    "ms-dotnettools.csharp",
    "ms-dotnettools.vscode-dotnet-runtime",
    "bradlc.vscode-tailwindcss",
    "formulahendry.auto-rename-tag",
    "ms-vscode.vscode-json",
    "streetsidesoftware.code-spell-checker"
  ]
}
```

### إعدادات VS Code للمشروع
```json
{
  "editor.formatOnSave": true,
  "editor.codeActionsOnSave": {
    "source.fixAll": true
  },
  "dotnet.defaultSolution": "PropertyManagement.sln",
  "omnisharp.enableRoslynAnalyzers": true
}
```

## 🔧 أدوات التطوير المفيدة

### أدوات سطر الأوامر
```bash
# تثبيت أدوات Entity Framework
dotnet tool install --global dotnet-ef

# تثبيت أداة إنشاء الكود
dotnet tool install --global dotnet-aspnet-codegenerator

# تثبيت أداة فحص الأمان
dotnet tool install --global security-scan
```

### Scripts مفيدة
```bash
# تنظيف وإعادة بناء المشروع
./scripts/clean-build.sh

# تشغيل جميع الاختبارات مع التقارير
./scripts/run-tests.sh

# فحص جودة الكود
./scripts/code-quality.sh
```

## 📊 معايير جودة الكود

### Code Coverage
- الحد الأدنى للتغطية: **80%**
- تغطية الـ Unit Tests: **90%**
- تغطية الـ Integration Tests: **70%**

### Performance Benchmarks
- وقت تحميل الصفحة: < 2 ثانية
- وقت استجابة API: < 500ms
- استهلاك الذاكرة: < 100MB

## 🎨 إرشادات التصميم

### الألوان الأساسية
```css
:root {
  --primary-color: #2563eb;      /* أزرق أساسي */
  --secondary-color: #64748b;    /* رمادي ثانوي */
  --success-color: #059669;      /* أخضر نجاح */
  --warning-color: #d97706;      /* برتقالي تحذير */
  --danger-color: #dc2626;       /* أحمر خطر */
  --info-color: #0891b2;         /* أزرق معلومات */
}
```

### Typography
```css
/* الخطوط العربية */
.arabic-text {
  font-family: 'Noto Sans Arabic', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-weight: 400;
  line-height: 1.6;
}

/* الخطوط الإنجليزية */
.english-text {
  font-family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  font-weight: 400;
  line-height: 1.5;
}
```

## 🚀 نشر التحديثات

### مراحل النشر
1. **Development** - التطوير المحلي
2. **Staging** - بيئة الاختبار
3. **Production** - البيئة الإنتاجية

### Deployment Checklist
- [ ] تم اختبار جميع الميزات
- [ ] تم تحديث قاعدة البيانات
- [ ] تم فحص الأمان
- [ ] تم إنشاء نسخة احتياطية
- [ ] تم إشعار المستخدمين

---

**شكراً لك على اهتمامك بالمساهمة في FIT Aqari Plus! 🎉**

نتطلع لرؤية مساهماتك الرائعة! 🚀
