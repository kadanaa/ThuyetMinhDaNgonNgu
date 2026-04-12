# 📚 Hướng Dẫn Triển Khai Toàn Diện - Thuyết Minh Dạo Ngôn Ngữ

## 🎯 Giới Thiệu Project

**Thuyết Minh Dạo Ngôn Ngữ** là một hệ thống toàn diện gồm 3 ứng dụng chính:

1. **Tourist App** (.NET MAUI - Android)
   - Xem bản đồ các POI
   - Chọn vị trí hiện tại
   - Nhấn vào POI để xem thông tin
   - Dịch thuyết minh sang các ngôn ngữ khác
   - Text-to-Speech đọc lên thông tin POI

2. **Admin App** (Windows Form)
   - Đăng nhập Admin
   - Quản lý tất cả POI
   - Duyệt/từ chối yêu cầu đăng ký POI từ POI Owner
   - Xem lịch sử hoạt động

3. **POI Owner App** (Windows Form)
   - Đăng ký tài khoản
   - Đăng ký POI mới
   - Chỉnh sửa thông tin POI của mình
   - Theo dõi trạng thái duyệt

---

## 🗄️ Cấu Trúc Database

### Cây Thư Mục
```
ThuyetMinhDaNgonNgu/
├── Database/
│   ├── ThuyetMinhDb.sql              (SQL Server script)
│   ├── README_DATABASE.md            (Tài liệu database)
│   ├── EntityModels.cs               (Entity Framework models)
│   └── appsettings.template.json     (Connection string template)
├── Tourist/                          (.NET MAUI App)
├── Admin/                            (Windows Form App)
├── POIOwner/                         (Windows Form App)
└── ThuyetMinhDaNgonNgu.slnx          (Solution file)
```

### Bảng Chính
| Bảng | Mục Đích | Ghi Chú |
|------|---------|--------|
| Users | Lưu Admin & POI Owner | Với mã hóa password |
| PointsOfInterest | Lưu POI (quán ăn, cafe, v.v.) | Bao gồm tọa độ, bán kính |
| POITranslations | Lưu bản dịch POI | Hỗ trợ đa ngôn ngữ |
| POIApprovalRequests | Quản lý yêu cầu duyệt | Track trạng thái duyệt |
| POIMedia | Lưu hình ảnh/video | Cho mỗi POI |
| Languages | Danh sách ngôn ngữ hỗ trợ | En, Vi, Es, Fr, v.v. |
| AuditLogs | Ghi log hoạt động | Cho audit trail |
| UserPreferences | Tùy chọn người dùng | Lưu cài đặt cá nhân |

---

## 🛠️ Cài Đặt Database

### 1️⃣ **Chuẩn Bị**
- SQL Server 2016 hoặc cao hơn
- SQL Server Management Studio (SSMS)

### 2️⃣ **Tạo Database**

**Cách 1: Dùng SSMS**
```
1. Mở SQL Server Management Studio
2. Kết nối đến SQL Server
3. Nhấp chuột phải > New Query
4. Mở file Database/ThuyetMinhDb.sql
5. Nhấn F5 để chạy
6. Database "ThuyetMinhDaNgonNgu" sẽ được tạo
```

**Cách 2: Dùng PowerShell**
```powershell
sqlcmd -S YOUR_SERVER_NAME -U sa -P YOUR_PASSWORD -i "D:\dotnet\ThuyetMinhDaNgonNgu\Database\ThuyetMinhDb.sql"
```

### 3️⃣ **Xác Minh Cài Đặt**
```sql
USE ThuyetMinhDaNgonNgu;

-- Kiểm tra số lượng bảng
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
-- Kết quả mong đợi: 8 bảng

-- Kiểm tra dữ liệu
SELECT * FROM [dbo].[Users];
SELECT * FROM [dbo].[Languages];
SELECT * FROM [dbo].[PointsOfInterest];
```

---

## 👥 Tài Khoản Mặc Định

### Admin
- **Username**: `admin`
- **Password**: `Admin@123`
- **Email**: `admin@thuyetminh.vn`

### POI Owner (Mẫu)
- **Username**: `poiowner01`
- **Password**: `Admin@123` (cùng mật khẩu, hãy đổi sau)
- **Email**: `owner1@thuyetminh.vn`

---

## 🔗 Chuỗi Kết Nối (Connection Strings)

### For .NET Applications

**Integrated Security (Windows Authentication):**
```json
{
  "ConnectionStrings": {
    "ThuyetMinhDb": "Server=YOUR_SERVER_NAME;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

**SQL Server Authentication:**
```json
{
  "ConnectionStrings": {
    "ThuyetMinhDb": "Server=YOUR_SERVER_NAME;Database=ThuyetMinhDaNgonNgu;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

### Lưu vào: `appsettings.json` mỗi project

---

## 🚀 Triển Khai Các Project

### **Admin Project**

**Bước 1:** Cài đặt NuGet packages
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package EntityFramework
# hoặc dùng Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**Bước 2:** Tạo DbContext
```csharp
using Microsoft.EntityFrameworkCore;
using ThuyetMinhDaNgonNgu.Models;

public class ThuyetMinhDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }
    public DbSet<POIApprovalRequest> POIApprovalRequests { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    // ... more DbSets

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=YOUR_SERVER;Database=ThuyetMinhDaNgonNgu;...");
    }
}
```

**Bước 3:** Tạo login form
- Xác thực người dùng Admin từ bảng Users
- So sánh password (sử dụng bcrypt)

**Bước 4:** Tạo management forms
- Danh sách POI, chỉnh sửa, xóa
- Duyệt/từ chối yêu cầu POI từ POI Owner
- Xem lịch sử hoạt động

---

### **POI Owner Project**

**Chức Năng Chính:**
1. Đăng ký tài khoản (Form đăng ký)
2. Đăng nhập (Form đăng nhập)
3. Đăng ký POI mới:
   - Nhập tên POI, mô tả (tiếng Việt)
   - Chọn vị trí trên bản đồ hoặc nhập tọa độ
   - Nhập bán kính
   - Upload hình ảnh
   - Gửi yêu cầu duyệt
4. Quản lý POI của mình:
   - Xem danh sách POI
   - Chỉnh sửa thông tin
   - Xem trạng thái duyệt

**Database Interactions:**
- INSERT vào `Users` (khi đăng ký)
- INSERT/UPDATE vào `PointsOfInterest`
- INSERT vào `POIApprovalRequests`
- INSERT vào `POIMedia`

---

### **Tourist App** (.NET MAUI)

**Chức Năng Chính:**
1. Chọn vị trí giả lập hiện tại
2. Hiển thị bản đồ Google Maps
3. Hiển thị POI gần vị trí hiện tại:
   - Gọi `sp_GetPOINearLocation`
   - Hiển thị trên bản đồ
4. Click chọn POI:
   - Hiển thị cửa sổ thông tin POI
   - Hiển thị mô tả bằng tiếng Việt
   - Chọn ngôn ngữ để dịch
   - Nút "Thuyết minh" (Text-to-Speech)

**Database Interactions:**
- SELECT từ `PointsOfInterest` (những POI đã được duyệt)
- SELECT từ `POITranslations` (lấy bản dịch)
- SELECT từ `Languages` (danh sách ngôn ngữ)
- Có thể cập nhật `UserPreferences` (lưu ngôn ngữ được chọn)

**API Endpoints cần:**
```
GET /api/poi/nearby?latitude=21.028&longitude=105.854&distance=5000
GET /api/poi/{id}
GET /api/poi/{id}/translations?language=en
GET /api/languages
```

---

## 🌐 Quy Ước Tọa Độ

**Hệ Tọa Độ:** WGS 84 (GPS)
- **Latitude (X):** Khoảng -90 đến +90° (Nam-Bắc)
- **Longitude (Y):** Khoảng -180 đến +180° (Tây-Đông)
- **Radius:** Tính bằng **mét (m)**

**Ví dụ:**
- Hà Nội: 21.0285°N, 105.8542°E
- TP.HCM: 10.7769°N, 106.6869°E

---

## 📱 Google Maps Integration

### For Tourist App (MAUI)

**1. Cài đặt NuGet:**
```bash
dotnet add package Microsoft.Maui.Controls.Maps
dotnet add package GoogleMaps
```

**2. Kết nối API:**
```csharp
public partial class MapPage : ContentPage
{
    private Map map;
    private decimal _currentLatitude = 21.0285m;
    private decimal _currentLongitude = 105.8542m;

    public MapPage()
    {
        InitializeComponent();
        InitializeMap();
        LoadNearbyPOIs();
    }

    private void InitializeMap()
    {
        map = new Map();
        map.IsShowingUser = true;
        map.MoveToRegion(MapSpan.FromCenterAndRadius(
            new Location(_currentLatitude, _currentLongitude),
            Distance.FromMiles(1)));
        Content = map;
    }

    private async void LoadNearbyPOIs()
    {
        var response = await HttpClient.GetAsync(
            $"/api/poi/nearby?lat={_currentLatitude}&lon={_currentLongitude}");
        var pois = await response.Content.ReadAsAsync<List<PointOfInterest>>();

        foreach (var poi in pois)
        {
            var pin = new Pin
            {
                Location = new Location(poi.Latitude, poi.Longitude),
                Title = poi.POIName,
                Address = poi.Address
            };
            map.Pins.Add(pin);
        }
    }
}
```

---

## 🗣️ Text-to-Speech & Translation

### Dịch (Translation)
- Gọi **Google Translate API** hoặc **Azure Translator**
- Input: Mô tả tiếng Việt
- Output: Mô tả ngôn ngữ được chọn
- Lưu vào bảng `POITranslations`

### Text-to-Speech (TTS)
```csharp
public async Task SpeakDescription(string text, string language)
{
    var settings = new SpeechOptions 
    { 
        Locale = GetLocaleCode(language) // 'en-US', 'es-ES', v.v.
    };

    await TextToSpeech.SpeakAsync(text, settings);
}
```

---

## 🔐 Bảo Mật

### Password Hashing
```csharp
using BCrypt.Net;

// Hashing password
string passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

// Verifying password
bool isValid = BCrypt.Net.BCrypt.Verify("Admin@123", passwordHash);
```

### Authentication
- JWT Token cho API
- Session management cho Windows Forms
- HTTPS enforced

---

## 📊 Dữ Liệu Mẫu

**POI mẫu đã được thêm vào:**

1. **Phở Bắc Hà**
   - Loại: Restaurant
   - Vị trí: 21.028511, 105.854100
   - Bán kính: 500m

2. **Cafe Trời Xanh**
   - Loại: Cafe
   - Vị trí: 21.027500, 105.853800
   - Bán kính: 400m

3. **Tạp Hóa Minh Phúc**
   - Loại: Shop
   - Vị trí: 21.029000, 105.855200
   - Bán kính: 300m

---

## ✅ Checklist Triển Khai

- [ ] Cài đặt SQL Server
- [ ] Chạy script `ThuyetMinhDb.sql`
- [ ] Xác minh database tạo thành công
- [ ] Cập nhật connection string trong mỗi project
- [ ] Admin Project:
  - [ ] Cài NuGet packages
  - [ ] Tạo DbContext
  - [ ] Tạo login form
  - [ ] Tạo POI management UI
  - [ ] Tạo approval management UI
- [ ] POI Owner Project:
  - [ ] Tạo registration form
  - [ ] Tạo login form
  - [ ] Tạo POI registration form
  - [ ] Tạo POI management UI
- [ ] Tourist App:
  - [ ] Cài Google Maps
  - [ ] Tạo map UI
  - [ ] Tích hợp API lấy POI
  - [ ] Tạo POI detail dialog
  - [ ] Cài Translation & TTS
- [ ] Testing & Deployment
  - [ ] Unit tests
  - [ ] Integration tests
  - [ ] UAT (User Acceptance Testing)

---

## 📞 Hỗ Trợ & Liên Hệ

Nếu gặp vấn đề:
1. Kiểm tra connection string
2. Đảm bảo SQL Server đang chạy
3. Kiểm tra logs trong `AuditLogs`
4. Tham khảo documentation trong mỗi project

---

**Version**: 1.0
**Last Updated**: 2024
