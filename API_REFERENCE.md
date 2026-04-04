# 📡 API Reference & Data Access Guide

## 🎯 Tổng Quan

Document này cung cấp tất cả các stored procedures, views, và cách truy cập dữ liệu từ các project.

---

## 🔓 AUTHENTICATION APIs

### 1. **Verify Login**
```sql
EXEC sp_VerifyLogin 
    @Username = 'admin',
    @PasswordHash = 'bcrypt_hash_here';
```

**Trả về:**
- UserId
- Username
- Email
- FullName
- Role ('Admin' or 'POIOwner')
- IsActive

**C# Example:**
```csharp
public async Task<User> VerifyLoginAsync(string username, string passwordHash)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_VerifyLogin", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);

            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                return new User
                {
                    UserId = (int)reader["UserId"],
                    Username = reader["Username"].ToString(),
                    Email = reader["Email"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Role = reader["Role"].ToString(),
                    IsActive = (bool)reader["IsActive"]
                };
            }
            return null;
        }
    }
}
```

---

## 🗺️ POI APIs

### 1. **Get All Approved POIs**
```sql
EXEC sp_GetAllApprovedPOIs;
```

**Trả về:** Tất cả POI đã được duyệt và đang hoạt động
- POIId, POIName, Description
- Latitude, Longitude, Radius
- Category, Address, PhoneNumber, Website

**Dùng cho:** Tourist app - Load toàn bộ POI khi khởi động

---

### 2. **Get POI By ID**
```sql
EXEC sp_GetPOIById @POIId = 1;
```

**Trả về:** Chi tiết đầy đủ của 1 POI
- Thông tin POI
- Tên & email chủ sở hữu
- Trạng thái duyệt

**Dùng cho:** Hiển thị popup chi tiết POI

**C# Example:**
```csharp
public async Task<PointOfInterest> GetPOIDetailAsync(int poiId)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_GetPOIById", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@POIId", poiId);

            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                return new PointOfInterest
                {
                    POIId = (int)reader["POIId"],
                    POIName = reader["POIName"].ToString(),
                    Description = reader["Description"].ToString(),
                    Latitude = (decimal)reader["Latitude"],
                    Longitude = (decimal)reader["Longitude"],
                    Radius = (float)reader["Radius"],
                    // ... more properties
                };
            }
            return null;
        }
    }
}
```

---

### 3. **Get POI Near Location** (QUAN TRỌNG)
```sql
EXEC sp_GetPOINearLocationAdvanced 
    @Latitude = 21.028511,
    @Longitude = 105.854100,
    @DistanceMeters = 5000;
```

**Parameters:**
- @Latitude: Vĩ độ (Y)
- @Longitude: Kinh độ (X)
- @DistanceMeters: Bán kính tìm kiếm (mét)

**Trả về:**
- Tất cả POI trong bán kính (sắp xếp theo khoảng cách gần nhất)
- DistanceMeters: Khoảng cách thực tế tính từ vị trí hiện tại

**Dùng cho:** Tourist app - Lấy POI gần vị trí hiện tại

**C# MAUI Example:**
```csharp
public async Task<List<PointOfInterest>> GetNearbyPOIsAsync(decimal latitude, decimal longitude, float distance = 5000)
{
    var pois = new List<PointOfInterest>();

    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_GetPOINearLocationAdvanced", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Latitude", latitude);
            command.Parameters.AddWithValue("@Longitude", longitude);
            command.Parameters.AddWithValue("@DistanceMeters", distance);

            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                pois.Add(new PointOfInterest
                {
                    POIId = (int)reader["POIId"],
                    POIName = reader["POIName"].ToString(),
                    Latitude = (decimal)reader["Latitude"],
                    Longitude = (decimal)reader["Longitude"],
                    Category = reader["Category"].ToString(),
                    Address = reader["Address"].ToString(),
                    // ... more properties
                });
            }
        }
    }
    return pois;
}
```

---

### 4. **Get POI By Category**
```sql
EXEC sp_GetPOIByCategory @Category = 'Restaurant';
```

**Trả về:** Tất cả POI trong danh mục được chọn

**Dùng cho:** Filter POI theo loại (Restaurant, Cafe, Shop, v.v.)

---

### 5. **Create POI**
```sql
EXEC sp_CreatePOI
    @POIName = 'Nhà hàng mới',
    @Description = 'Mô tả tiếng Việt',
    @Latitude = 21.028511,
    @Longitude = 105.854100,
    @Radius = 500,
    @Category = 'Restaurant',
    @Address = '123 Đường ABC',
    @PhoneNumber = '0123456789',
    @Website = 'example.com',
    @OwnerId = 2,
    @IsApproved = 0;
```

**Trả về:** NewPOIId (ID của POI mới tạo)

**Dùng cho:** POI Owner - Đăng ký POI mới

---

### 6. **Update POI**
```sql
EXEC sp_UpdatePOI
    @POIId = 1,
    @POIName = 'Tên mới',
    @Description = 'Mô tả mới',
    @Latitude = 21.0285,
    @Longitude = 105.8542,
    @Radius = 600,
    @Category = 'Restaurant',
    @Address = 'Địa chỉ mới',
    @PhoneNumber = '0987654321',
    @Website = 'newsite.com';
```

**Dùng cho:** POI Owner - Chỉnh sửa POI của mình

---

### 7. **Delete POI**
```sql
EXEC sp_DeletePOI @POIId = 1;
```

**Lưu ý:** Tự động xóa related data (media, translations, requests)

**Dùng cho:** Admin - Xóa POI

---

## 🌐 TRANSLATION APIs

### 1. **Get All Translations for POI**
```sql
EXEC sp_GetPOITranslations @POIId = 1;
```

**Trả về:**
- TranslationId
- LanguageCode, LanguageName
- TranslatedDescription, TranslatedName

**Dùng cho:** Tải tất cả bản dịch của 1 POI

---

### 2. **Get Translation by Language**
```sql
EXEC sp_GetPOITranslationByLanguage 
    @POIId = 1,
    @LanguageCode = 'en';
```

**Trả về:**
- TranslatedDescription
- TranslatedName

**Dùng cho:** Tourist app - Lấy bản dịch cho ngôn ngữ được chọn

**C# Example:**
```csharp
public async Task<string> GetTranslationAsync(int poiId, string languageCode)
{
    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_GetPOITranslationByLanguage", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@POIId", poiId);
            command.Parameters.AddWithValue("@LanguageCode", languageCode);

            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                return reader["TranslatedDescription"].ToString();
            }
            return null;
        }
    }
}
```

---

### 3. **Add/Update Translation**
```sql
EXEC sp_UpsertPOITranslation
    @POIId = 1,
    @LanguageCode = 'en',
    @LanguageName = 'English',
    @TranslatedDescription = 'Translated description here',
    @TranslatedName = 'Translated Name';
```

**Dùng cho:** Admin hoặc AI service - Thêm/cập nhật bản dịch

---

## ✅ APPROVAL REQUEST APIs

### 1. **Get Pending Approval Requests**
```sql
EXEC sp_GetPendingApprovalRequests;
```

**Trả về:**
- RequestId, POIId, OwnerId
- OwnerName, RequestType, Status
- RequestData, RequestedDate, POIName

**Dùng cho:** Admin - Xem danh sách yêu cầu chờ duyệt

**C# Example:**
```csharp
public async Task<List<POIApprovalRequest>> GetPendingRequestsAsync()
{
    var requests = new List<POIApprovalRequest>();

    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_GetPendingApprovalRequests", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            while (reader.Read())
            {
                requests.Add(new POIApprovalRequest
                {
                    RequestId = (int)reader["RequestId"],
                    POIId = (int)reader["POIId"],
                    OwnerId = (int)reader["OwnerId"],
                    // ... more properties
                });
            }
        }
    }
    return requests;
}
```

---

### 2. **Approve POI Request**
```sql
EXEC sp_ApprovePOIRequest
    @RequestId = 1,
    @AdminId = 1,
    @Comments = 'POI looks good. Approved!';
```

**Tác động:**
- Cập nhật request status = 'Approved'
- Cập nhật POI: IsApproved = 1, Status = 'Active'
- Ghi log hoạt động

**Dùng cho:** Admin - Duyệt yêu cầu POI

---

### 3. **Reject POI Request**
```sql
EXEC sp_RejectPOIRequest
    @RequestId = 1,
    @AdminId = 1,
    @Comments = 'Address is not clear. Please resubmit with more details.';
```

**Tác động:**
- Cập nhật request status = 'Rejected'
- POI vẫn ở trạng thái Pending

---

### 4. **Create POI Approval Request**
```sql
EXEC sp_CreatePOIApprovalRequest
    @POIId = 5,
    @OwnerId = 2,
    @RequestType = 'Create',
    @RequestData = '{"name":"New POI","lat":21.0285,...}';
```

**Dùng cho:** POI Owner - Gửi yêu cầu duyệt POI mới

---

## 👥 USER APIs

### 1. **Create New User**
```sql
EXEC sp_CreateUser
    @Username = 'newowner',
    @PasswordHash = 'bcrypt_hash',
    @Email = 'owner@example.com',
    @FullName = 'Trần Văn A',
    @Role = 'POIOwner',
    @PhoneNumber = '0987654321';
```

**Trả về:** NewUserId

**Dùng cho:** POI Owner app - Đăng ký tài khoản mới

---

### 2. **Get User by Username**
```sql
EXEC sp_GetUserByUsername @Username = 'admin';
```

**Trả về:** Thông tin user (không trả password)

---

### 3. **Get All POI Owners**
```sql
EXEC sp_GetAllPOIOwners;
```

**Trả về:** Danh sách tất cả POI Owner

**Dùng cho:** Admin - Quản lý POI Owners

---

## 📊 STATISTICS APIs

### 1. **Get Dashboard Statistics**
```sql
EXEC sp_GetDashboardStats;
```

**Trả về:**
- ApprovedPOICount
- PendingPOICount
- POIOwnerCount
- PendingRequestsCount
- CategoriesCount

**Dùng cho:** Admin - Dashboard statistics

**C# Example:**
```csharp
public async Task<DashboardStats> GetDashboardStatsAsync()
{
    using (var connection = new SqlConnection(_connectionString))
    {
        using (var command = new SqlCommand("sp_GetDashboardStats", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            connection.Open();
            var reader = await command.ExecuteReaderAsync();

            if (reader.Read())
            {
                return new DashboardStats
                {
                    ApprovedPOICount = (int)reader["ApprovedPOICount"],
                    PendingPOICount = (int)reader["PendingPOICount"],
                    POIOwnerCount = (int)reader["POIOwnerCount"],
                    PendingRequestsCount = (int)reader["PendingRequestsCount"],
                    CategoriesCount = (int)reader["CategoriesCount"]
                };
            }
        }
    }
    return null;
}
```

---

### 2. **Get POI Statistics by Category**
```sql
EXEC sp_GetPOIStatisticsByCategory;
```

**Trả về:**
- Category, Count, ApprovedCount, PendingCount

---

## 🔍 VIEWS

### View: vw_POIWithOwner
```sql
SELECT * FROM [dbo].[vw_POIWithOwner];
```

**Trả về:** POI với thông tin chủ sở hữu

```sql
SELECT * FROM [dbo].[vw_POIWithOwner] 
WHERE [Status] = 'Active' AND [IsApproved] = 1;
```

---

## 📝 AUDIT LOG APIs

### 1. **Add Audit Log**
```sql
EXEC sp_AddAuditLog
    @UserId = 1,
    @Action = 'Update',
    @TableName = 'PointsOfInterest',
    @RecordId = 5,
    @OldValue = '{"name":"Old"}',
    @NewValue = '{"name":"New"}',
    @Description = 'Updated POI name',
    @IPAddress = '192.168.1.1';
```

**Dùng cho:** Log tất cả hoạt động người dùng

---

### 2. **Get Audit Logs by User**
```sql
EXEC sp_GetAuditLogsByUser
    @UserId = 1,
    @Days = 30;
```

**Trả về:** Hoạt động của user trong 30 ngày gần nhất

---

## 🌐 SUPPORTED LANGUAGES

Database hỗ trợ 10 ngôn ngữ:

| Code | Language | Native Name |
|------|----------|-------------|
| en | English | English |
| vi | Vietnamese | Tiếng Việt |
| es | Spanish | Español |
| fr | French | Français |
| de | German | Deutsch |
| ja | Japanese | 日本語 |
| zh | Chinese | 中文 |
| ko | Korean | 한국어 |
| th | Thai | ไทย |
| id | Indonesian | Bahasa Indonesia |

---

## 💡 Tips & Best Practices

### 1. **Caching Translations**
```csharp
var cache = new Dictionary<(int, string), string>();

public async Task<string> GetCachedTranslationAsync(int poiId, string lang)
{
    var key = (poiId, lang);
    if (!cache.ContainsKey(key))
    {
        cache[key] = await GetTranslationAsync(poiId, lang);
    }
    return cache[key];
}
```

### 2. **Batch POI Loading**
```csharp
public async Task<List<PointOfInterest>> GetPOIsByIdsAsync(List<int> poiIds)
{
    // Tối ưu hơn gọi sp_GetPOIById nhiều lần
    using (var context = new ThuyetMinhDbContext())
    {
        return await context.PointsOfInterest
            .Where(p => poiIds.Contains(p.POIId))
            .ToListAsync();
    }
}
```

### 3. **Connection Pooling**
```json
{
  "ConnectionStrings": {
    "ThuyetMinhDb": "Server=...;Database=...;Min Pool Size=5;Max Pool Size=100;Pool Blocking Timeout=5"
  }
}
```

---

**Version**: 1.0
**Last Updated**: 2024
