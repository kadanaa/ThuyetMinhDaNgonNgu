# 🚀 Quick Start Guide - Thuyết Minh Dạo Ngôn Ngữ

## ⚡ Bắt Đầu Nhanh Trong 5 Phút

### 📋 Yêu Cầu Tiên Quyết
- SQL Server 2016+
- Visual Studio 2022 (Community Edition hoặc cao hơn)
- .NET 10 SDK
- Visual Studio Extensions:
  - NuGet Package Manager
  - SQL Server Object Explorer

---

## 🗄️ Bước 1: Tạo Database (2 phút)

### Cách 1: Dùng Visual Studio (Dễ nhất)
```
1. Mở Visual Studio
2. View > Other Windows > SQL Server Object Explorer
3. Nhấp chuột phải trên "SQL Server"
4. Chọn "Add SQL Server..."
5. Kết nối đến SQL Server của bạn
6. Nhấp chuột phải > New Query
7. Mở file "Database/ThuyetMinhDb.sql"
8. Nhấn Ctrl+Shift+E để chạy
```

### Cách 2: Command Line
```powershell
cd D:\dotnet\ThuyetMinhDaNgonNgu\Database
sqlcmd -S YOUR_SERVER_NAME -U sa -P YOUR_PASSWORD -i ThuyetMinhDb.sql
```

### Cách 3: SQL Server Management Studio
```
1. Mở SSMS
2. Connect to Server
3. Right-click > New Query
4. Mở ThuyetMinhDb.sql
5. Nhấn F5
```

✅ Database được tạo thành công khi bạn thấy: `Database creation completed successfully!`

---

## 📁 Bước 2: Cập Nhật Connection String (1 phút)

### Trong mỗi project (Admin, POIOwner, Tourist):

**File: `appsettings.json`**
```json
{
  "ConnectionStrings": {
    "ThuyetMinhDb": "Server=YOUR_SERVER;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true"
  }
}
```

**Thay `YOUR_SERVER` bằng tên server của bạn:**
- Local: `.` hoặc `localhost`
- Named instance: `COMPUTERNAME\INSTANCENAME`
- Default: `.\SQLEXPRESS` (nếu dùng SQL Server Express)

---

## 🏗️ Bước 3: Cấu Hình Project (2 phút)

### Cho mỗi project, thêm NuGet packages:

#### Admin & POIOwner (Windows Forms):
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package EntityFramework
dotnet add package BCrypt.Net
# Hoặc nếu dùng Entity Framework Core:
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

#### Tourist (MAUI):
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.Maui.Controls.Maps
dotnet add package GoogleMaps
```

---

## 🔑 Bước 4: Xác Thực Login (1 phút)

### Tài Khoản Mặc Định

| Role | Username | Password | Email |
|------|----------|----------|-------|
| Admin | `admin` | `Admin@123` | `admin@thuyetminh.vn` |
| POI Owner | `poiowner01` | `Admin@123` | `owner1@thuyetminh.vn` |

⚠️ **NHẮC NHỠ**: Đổi mật khẩu sau khi triển khai!

---

## 📝 Bước 5: Tạo DbContext (Tùy chọn)

Nếu dùng Entity Framework Core, tạo file `DbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using ThuyetMinhDaNgonNgu.Models;

public class ThuyetMinhDbContext : DbContext
{
    public ThuyetMinhDbContext(DbContextOptions<ThuyetMinhDbContext> options) 
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<PointOfInterest> PointsOfInterest { get; set; }
    public DbSet<POITranslation> POITranslations { get; set; }
    public DbSet<POIApprovalRequest> POIApprovalRequests { get; set; }
    public DbSet<POIMedia> POIMedia { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
}
```

**Trong `Startup.cs` hoặc `Program.cs`:**
```csharp
services.AddDbContext<ThuyetMinhDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("ThuyetMinhDb")));
```

---

## 🔍 Xác Minh Cài Đặt

### Chạy câu lệnh kiểm tra:
```sql
USE ThuyetMinhDaNgonNgu;

-- 1. Kiểm tra số bảng (kết quả: 8)
SELECT COUNT(*) as TableCount FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';

-- 2. Kiểm tra dữ liệu Admin
SELECT * FROM [dbo].[Users] WHERE Role = 'Admin';

-- 3. Kiểm tra dữ liệu POI
SELECT * FROM [dbo].[PointsOfInterest];

-- 4. Kiểm tra ngôn ngữ (kết quả: 10 ngôn ngữ)
SELECT COUNT(*) as LanguageCount FROM [dbo].[Languages];

-- 5. Chạy test procedure
EXEC sp_GetAllApprovedPOIs;
```

✅ Nếu tất cả trả về kết quả, database của bạn đã sẵn sàng!

---

## 🎯 Ví Dụ: Sử Dụng API

### Admin - Lấy danh sách yêu cầu duyệt chưa xử lý:
```csharp
using (var context = new ThuyetMinhDbContext())
{
    var pendingRequests = context.POIApprovalRequests
        .Where(r => r.Status == "Pending")
        .Include(r => r.Owner)
        .Include(r => r.POI)
        .OrderByDescending(r => r.RequestedDate)
        .ToList();
}
```

### Tourist - Lấy POI gần vị trí hiện tại:
```csharp
using (var connection = new SqlConnection(connectionString))
{
    using (var command = new SqlCommand("sp_GetPOINearLocationAdvanced", connection))
    {
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@Latitude", 21.0285);
        command.Parameters.AddWithValue("@Longitude", 105.8542);
        command.Parameters.AddWithValue("@DistanceMeters", 5000);

        connection.Open();
        var reader = command.ExecuteReader();
        // Xử lý dữ liệu
    }
}
```

### POI Owner - Tạo yêu cầu duyệt POI:
```csharp
using (var context = new ThuyetMinhDbContext())
{
    var request = new POIApprovalRequest
    {
        POIId = newPOI.POIId,
        OwnerId = currentUserId,
        RequestType = "Create",
        RequestData = JsonConvert.SerializeObject(poiData),
        Status = "Pending",
        RequestedDate = DateTime.UtcNow
    };

    context.POIApprovalRequests.Add(request);
    context.SaveChanges();
}
```

---

## 📊 Dữ Liệu Mẫu Có Sẵn

Database đã được tạo sẵn với 3 POI mẫu:

1. **Phở Bắc Hà** - Quán phở nổi tiếng
   - 📍 21.028511, 105.854100
   - 💰 Khá phải, đông khách
   - ⏰ 5h - 23h hàng ngày

2. **Cafe Trời Xanh** - Cafe học tập
   - 📍 21.027500, 105.853800
   - ☕ Cà phê rang tại chỗ
   - 📶 WiFi miễn phí

3. **Tạp Hóa Minh Phúc** - Cửa hàng tiện lợi
   - 📍 21.029000, 105.855200
   - 🛒 Đầy đủ hàng hóa
   - ⏰ 6h - 23h hàng ngày

---

## 🐛 Troubleshooting

### ❌ Lỗi: "Cannot connect to database"
```
Giải pháp:
1. Kiểm tra SQL Server đang chạy: Services > SQL Server (MSSQLSERVER)
2. Kiểm tra connection string
3. Kiểm tra tên server đúng
4. Chạy lại script ThuyetMinhDb.sql
```

### ❌ Lỗi: "Database does not exist"
```
Giải pháp:
1. Xác minh script đã chạy thành công
2. Dùng SQL Server Management Studio kiểm tra:
   - View > Object Explorer
   - Tìm database "ThuyetMinhDaNgonNgu"
3. Nếu không có, chạy lại ThuyetMinhDb.sql
```

### ❌ Lỗi: "Login failed for user"
```
Giải pháp:
1. Dùng Windows Authentication:
   "Integrated Security=true"
2. Hoặc sử dụng SQL Server credentials:
   "User Id=sa;Password=YOUR_PASSWORD"
```

### ❌ Lỗi: "Access denied to stored procedure"
```
Giải pháp:
1. Chạy script StoredProcedures.sql
2. Kiểm tra permissions trong SQL Server:
   - Security > Logins > [Your Login]
   - User Mapping > [Database] > Public
```

---

## ✨ Tip & Tricks

### 1. Kiểm tra stored procedures:
```sql
SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'dbo';
```

### 2. Xem tất cả POI được duyệt:
```sql
EXEC sp_GetAllApprovedPOIs;
```

### 3. Tìm POI gần vị trí:
```sql
EXEC sp_GetPOINearLocationAdvanced 
    @Latitude = 21.028511,
    @Longitude = 105.854100,
    @DistanceMeters = 2000;
```

### 4. Xem thống kê dashboard:
```sql
EXEC sp_GetDashboardStats;
```

### 5. Kiểm tra audit logs:
```sql
SELECT TOP 20 * FROM [dbo].[AuditLogs] ORDER BY [Timestamp] DESC;
```

---

## 📞 Cần Giúp?

Nếu gặp vấn đề:

1. 📖 Kiểm tra `README_DATABASE.md` để chi tiết
2. 📋 Xem `IMPLEMENTATION_GUIDE.md` để hướng dẫn đầy đủ
3. 🔍 Xem logs trong SQL:
   ```sql
   SELECT * FROM [dbo].[AuditLogs] WHERE [Timestamp] > GETUTCDATE() - 1
   ```
4. 🧪 Chạy test queries để xác minh

---

## 🎉 Đã Sẵn Sàng!

Bây giờ bạn đã có:
- ✅ Database hoàn chỉnh với 8 bảng
- ✅ 10 ngôn ngữ được hỗ trợ
- ✅ 3 POI mẫu
- ✅ Admin & POI Owner tài khoản
- ✅ 20+ stored procedures
- ✅ Audit logging
- ✅ Toàn bộ infrastructure

**Tiếp theo:** Bắt đầu phát triển các project Admin, POIOwner, và Tourist!

---

**Version**: 1.0
**Last Updated**: 2024
