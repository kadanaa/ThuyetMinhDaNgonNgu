# 📋 Tourist App - Setup Guide

## 🎯 Hướng Dẫn Cài Đặt & Chạy Ứng Dụng

## Bước 1: Yêu Cầu Hệ Thống

### Phần Mềm Cần Thiết
- **Visual Studio 2022** (Community, Professional, hoặc Enterprise)
- **.NET 10 SDK** (hoặc cao hơn)
- **SQL Server 2016+** (Local hoặc Remote)
- **MAUI Workload** (cho Visual Studio)

### Kiểm Tra Phiên Bản

```powershell
# Kiểm tra .NET SDK version
dotnet --version

# Kiểm tra MAUI workload
dotnet workload list
```

## Bước 2: Chuẩn Bị Database

### 2.1 Tạo Database

Chạy file `Database/ThuyetMinhDb.sql` trên SQL Server:

```sql
-- Chạy từ SQL Server Management Studio hoặc Azure Data Studio
-- Mở: Database/ThuyetMinhDb.sql
-- Execute
```

### 2.2 Kiểm Tra Database

```sql
USE ThuyetMinhDaNgonNgu;

-- Check tables
SELECT * FROM PointsOfInterest;
SELECT * FROM Languages;
SELECT * FROM POITranslations;

-- Verify sample data exists
SELECT COUNT(*) as TotalPOIs FROM PointsOfInterest WHERE IsApproved = 1;
```

## Bước 3: Cấu Hình Connection String

### Phương Pháp 1: Trong MauiProgram.cs

File: `Tourist/MauiProgram.cs`

```csharp
private static string GetConnectionString()
{
    // Thay đổi nếu SQL Server không phải localhost
    return "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=30;";
}
```

**Các trường hợp khác:**

```csharp
// SQL Server Authentication (với username/password)
"Server=localhost;Database=ThuyetMinhDaNgonNgu;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"

// Remote Server
"Server=192.168.1.100;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;"

// Azure SQL Database
"Server=yourserver.database.windows.net;Database=ThuyetMinhDaNgonNgu;User Id=username;Password=password;"
```

### Phương Pháp 2: Sử dụng appsettings.json

File: `Tourist/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

## Bước 4: Cài Đặt NuGet Packages

Visual Studio sẽ tự động restore packages, nhưng có thể chạy thủ công:

```powershell
# Mở Package Manager Console
cd Tourist
dotnet restore
```

## Bước 5: Build & Run

### Phương Pháp 1: Visual Studio IDE

1. **Mở Solution**: `ThuyetMinhDaNgonNgu.sln`
2. **Set Startup Project**: `Tourist` project
3. **Build**: `Build → Build Solution` (Ctrl+Shift+B)
4. **Run**: `Debug → Start Debugging` (F5)

### Phương Pháp 2: Command Line

```powershell
# Navigate to Tourist directory
cd Tourist

# Build
dotnet build

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0

# Run on Android (requires emulator)
dotnet run -f net10.0-android

# Run on iOS (requires Mac + Xcode)
dotnet run -f net10.0-ios
```

## Bước 6: Kiểm Tra Ứng Dụng

### Kiểm Tra Khởi Động

✅ Ứng dụng khởi động thành công  
✅ Vị trí "Tháp Rùa - Hà Nội" được chọn mặc định  
✅ 10 ngôn ngữ được load (dropdown có dữ liệu)  
✅ Danh sách POI gần vị trí hiện tại được hiển thị

### Kiểm Tra Chức Năng

#### Test 1: Tìm Kiếm POI
1. Click nút "Tìm Địa Điểm Gần Đây"
2. Verify: Có ít nhất 1 POI trong danh sách
3. Expected: "Tìm thấy X địa điểm"

#### Test 2: Chọn Vị Trí Khác
1. Click dropdown "Chọn địa điểm..."
2. Chọn "Phố Cổ Hà Nội"
3. Click "Tìm Địa Điểm Gần Đây"
4. Verify: Danh sách cập nhật

#### Test 3: Xem Chi Tiết
1. Chọn POI từ danh sách
2. Click button "📋 Chi Tiết"
3. Verify: Dialog hiển thị:
   - Tên địa điểm
   - Danh mục
   - Địa chỉ
   - Số điện thoại
   - Mô tả

#### Test 4: Text-to-Speech
1. Chọn POI từ danh sách
2. Chọn ngôn ngữ (e.g., "English")
3. Click button "🔊 Nghe Mô Tả"
4. Verify: Âm thanh phát

#### Test 5: Làm Mới
1. Click button "🔄 Làm Mới"
2. Verify: Danh sách được tải lại

## 🚨 Xử Lý Lỗi Thường Gặp

### Lỗi 1: "The type or namespace name 'DbContext' could not be found"

**Nguyên nhân**: Entity Framework Core chưa được cài
**Giải pháp**:
```powershell
# Cài đặt NuGet packages
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore
```

### Lỗi 2: "Unable to connect to database"

**Nguyên nhân**: Connection string không chính xác hoặc SQL Server không chạy
**Giải pháp**:
```powershell
# Kiểm tra SQL Server đang chạy
Get-Service "MSSQLSERVER" | Select-Object Status

# Hoặc sử dụng sqlcmd
sqlcmd -S localhost -Q "SELECT @@version"

# Kiểm tra database tồn tại
sqlcmd -S localhost -Q "SELECT DB_ID('ThuyetMinhDaNgonNgu')"
```

### Lỗi 3: "Collection is empty"

**Nguyên nhân**: Database không có POI hoặc query sai
**Giải pháp**:
```sql
-- Kiểm tra trong SQL Server
USE ThuyetMinhDaNgonNgu;
SELECT COUNT(*) FROM PointsOfInterest WHERE IsApproved = 1;
SELECT * FROM PointsOfInterest;
```

### Lỗi 4: "TextToSpeech not working"

**Nguyên nhân**: Platform-specific permission chưa cấp
**Giải pháp**:

**Windows**: Kiểm tra Settings → Sound
**Android**: Manifest.xml permissions
```xml
<uses-permission android:name="android.permission.INTERNET" />
```

**iOS**: Info.plist permissions
```xml
<key>NSMicrophoneUsageDescription</key>
<string>App needs access to microphone for text-to-speech</string>
```

## 📊 Kiến Trúc Thư Mục

```
ThuyetMinhDaNgonNgu/
├── Database/
│   ├── ThuyetMinhDb.sql              # 🔴 Chạy trước
│   ├── EntityModels.cs
│   └── README_DATABASE.md
├── Tourist/
│   ├── Models/                       # Entity models
│   ├── Data/
│   │   └── ThuyetMinhDbContext.cs   # DbContext
│   ├── Services/                     # Business logic
│   │   ├── IServices.cs
│   │   ├── PoiService.cs
│   │   ├── LocationService.cs
│   │   ├── TranslationService.cs
│   │   └── TtsService.cs
│   ├── MainPage.xaml                # UI
│   ├── MainPage.xaml.cs             # UI Logic
│   ├── MauiProgram.cs               # DI Setup
│   ├── appsettings.json             # Config
│   ├── README.md                    # App doc
│   └── Tourist.csproj
├── Admin/                           # Admin app (Windows Forms)
├── POIOwner/                        # POI Owner app (Windows Forms)
└── ThuyetMinhDaNgonNgu.sln         # 🔴 Mở file này
```

## ✅ Checklist Trước Khi Chạy

- [ ] .NET 10 SDK đã cài
- [ ] Visual Studio 2022 đã cài MAUI workload
- [ ] SQL Server đang chạy
- [ ] Database `ThuyetMinhDaNgonNgu` đã được tạo
- [ ] Connection string trong `MauiProgram.cs` chính xác
- [ ] NuGet packages đã restore
- [ ] Solution compile thành công
- [ ] Tourist project là startup project

## 🎓 Tiếp Theo

1. **Thêm POI mới**:
   - Mở Database/ThuyetMinhDb.sql
   - Insert POI record
   - Run query
   - App sẽ hiển thị POI mới

2. **Tùy chỉnh giao diện**:
   - Edit `MainPage.xaml` cho layout
   - Edit `MainPage.xaml.cs` cho logic
   - Build & test

3. **Thêm ngôn ngữ mới**:
   - Insert Language record
   - Insert POITranslation records
   - Reload app

## 📞 Support

Nếu gặp vấn đề:
1. Kiểm tra Error Output window (Visual Studio)
2. Kiểm tra Debug console
3. Xem file logs (nếu có)
4. Verify database connection

---

**Status**: ✅ Ready to use  
**Last Updated**: January 2025  
**Version**: 1.0.0
