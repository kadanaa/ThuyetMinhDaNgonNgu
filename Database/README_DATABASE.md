# Thuyết Minh Dạo Ngôn Ngữ - Database Documentation

## 📋 Mô Tả Tổng Quát

Database `ThuyetMinhDaNgonNgu` được thiết kế để hỗ trợ ứng dụng quản lý Điểm du lịch (POI - Points of Interest) với các chức năng:
- Người dùng du lịch (Tourist) có thể xem các POI trên bản đồ
- Người sở hữu POI (POI Owner) có thể đăng ký/chỉnh sửa POI
- Quản trị viên (Admin) duyệt và quản lý các POI

## 📊 Cấu Trúc Bảng Dữ Liệu

### 1. **Users** - Bảng người dùng
Lưu trữ thông tin tài khoản Admin và POI Owner

```
Cột chính:
- UserId (PK): ID duy nhất
- Username: Tên đăng nhập
- PasswordHash: Mật khẩu mã hóa
- Email: Email
- Role: 'Admin' hoặc 'POIOwner'
- IsActive: Trạng thái hoạt động
```

**Dữ liệu mặc định:**
- Admin: `username=admin`, `password=Admin@123` (hash đã mã hóa)
- POI Owner mẫu: `username=poiowner01`

### 2. **PointsOfInterest** - Bảng địa điểm
Chứa thông tin các POI (quán ăn, cafe, tiệm tạp hóa, v.v.)

```
Cột chính:
- POIId (PK): ID duy nhất
- POIName: Tên địa điểm
- Description: Mô tả bằng tiếng Việt (dùng cho TTS)
- Latitude, Longitude: Tọa độ (X, Y)
- Radius: Bán kính (mét)
- Category: Loại địa điểm ('Restaurant', 'Cafe', 'Shop', v.v.)
- OwnerId (FK): Người sở hữu
- IsApproved: Đã được duyệt hay chưa
- Status: 'Active', 'Inactive', 'Pending'
```

**Dữ liệu mẫu:**
- Phở Bắc Hà (Nhà hàng)
- Cafe Trời Xanh (Cafe)
- Tạp Hóa Minh Phúc (Cửa hàng)

### 3. **POITranslations** - Bảng dịch thuyết minh
Lưu trữ mô tả POI được dịch sang các ngôn ngữ khác nhau

```
Cột chính:
- TranslationId (PK): ID duy nhất
- POIId (FK): ID của POI
- LanguageCode: Mã ngôn ngữ ('en', 'es', 'fr', v.v.)
- TranslatedDescription: Mô tả được dịch
- TranslatedName: Tên được dịch
```

### 4. **POIApprovalRequests** - Bảng yêu cầu duyệt
Quản lý các yêu cầu tạo mới hoặc cập nhật POI từ POI Owner

```
Cột chính:
- RequestId (PK): ID yêu cầu
- POIId (FK): ID POI
- OwnerId (FK): ID chủ sở hữu
- RequestType: 'Create' hoặc 'Update'
- Status: 'Pending', 'Approved', 'Rejected'
- RequestData: JSON chứa dữ liệu chi tiết
- AdminComments: Ghi chú từ Admin
```

### 5. **POIMedia** - Bảng hình ảnh/video POI
Lưu trữ đường dẫn đến hình ảnh và video của POI

```
Cột chính:
- MediaId (PK): ID media
- POIId (FK): ID POI
- MediaType: 'Image' hoặc 'Video'
- FilePath: Đường dẫn file
- IsMainImage: Hình ảnh chính
```

### 6. **Languages** - Bảng danh sách ngôn ngữ
Quản lý các ngôn ngữ được hỗ trợ

```
Dữ liệu mặc định:
- en - English
- vi - Vietnamese (Tiếng Việt)
- es - Spanish (Tây Ban Nha)
- fr - French (Pháp)
- de - German (Đức)
- ja - Japanese (Nhật Bản)
- zh - Chinese (Trung Quốc)
- ko - Korean (Hàn Quốc)
- th - Thai (Thái Lan)
- id - Indonesian (Indonexia)
```

### 7. **AuditLogs** - Bảng ghi log hoạt động
Ghi lại tất cả các hoạt động trong hệ thống (Admin/POI Owner)

```
Cột chính:
- LogId (PK): ID log
- UserId (FK): ID người thực hiện
- Action: 'Create', 'Update', 'Delete', 'Approve', 'Reject', 'Login'
- TableName: Bảng bị ảnh hưởng
- OldValue, NewValue: Giá trị trước/sau khi thay đổi
- Timestamp: Thời gian thực hiện
```

### 8. **UserPreferences** - Bảng tùy chọn người dùng
Lưu trữ các tùy chọn cá nhân của người dùng

```
Cột chính:
- PreferenceId (PK): ID tùy chọn
- UserId (FK): ID người dùng
- PreferenceKey: Khóa tùy chọn
- PreferenceValue: Giá trị tùy chọn
```

## 🔧 Stored Procedures

### 1. **sp_GetPOINearLocation**
Lấy danh sách POI gần vị trí hiện tại (sử dụng công thức Haversine)

```sql
EXEC sp_GetPOINearLocation 
    @Latitude = 21.028511,
    @Longitude = 105.854100,
    @Distance = 5000; -- Khoảng cách tối đa (mét)
```

### 2. **sp_CreatePOIApprovalRequest**
Tạo yêu cầu duyệt POI từ POI Owner

```sql
EXEC sp_CreatePOIApprovalRequest 
    @POIId = 1,
    @OwnerId = 2,
    @RequestType = 'Create',
    @RequestData = '{...}'; -- JSON format
```

## 📐 Views

### **vw_POIWithOwner**
View kết hợp POI với thông tin chủ sở hữu

```sql
SELECT * FROM vw_POIWithOwner WHERE Status = 'Active';
```

## 🚀 Cài Đặt Database

### Bước 1: Mở SQL Server Management Studio
- Kết nối đến SQL Server của bạn

### Bước 2: Chạy Script
- Mở file `ThuyetMinhDb.sql`
- Chọn **Execute** (F5)
- Database sẽ được tạo tự động

### Bước 3: Xác Minh Cài Đặt
```sql
USE ThuyetMinhDaNgonNgu;
SELECT * FROM [dbo].[Users];
SELECT * FROM [dbo].[PointsOfInterest];
SELECT * FROM [dbo].[Languages];
```

## 🔐 Bảo Mật

1. **Password Hashing**: Sử dụng bcrypt (hiện tại script dùng hash mẫu)
   - Admin username: `admin`
   - Admin password: `Admin@123`

2. **Các khuyến cáo:**
   - Đổi mật khẩu admin sau khi triển khai
   - Sử dụng HTTPS cho kết nối database
   - Giới hạn quyền truy cập database

## 🗺️ Quy Ước Tọa Độ

- **Latitude (X)**: Khoảng -90 đến +90 độ (N-S)
- **Longitude (Y)**: Khoảng -180 đến +180 độ (E-W)
- **Radius**: Tính bằng mét (m)

**Ví dụ Hà Nội:**
- Latitude: 21.0285
- Longitude: 105.8542

## 📝 Mở Rộng Trong Tương Lai

Các bảng có thể được thêm vào:
1. **Bookings** - Để quản lý đặt bàn/vé
2. **Reviews** - Đánh giá từ người dùng
3. **Categories** - Chi tiết danh mục POI
4. **UserPreferences** - Để lưu tùy chọn người dùng

## 📞 Liên Hệ & Hỗ Trợ

Nếu có câu hỏi hoặc vấn đề liên quan đến database, vui lòng kiểm tra:
1. Kết nối SQL Server
2. Phiên bản SQL Server (2016 hoặc cao hơn)
3. Quyền tạo database

---

**Created**: 2024
**Version**: 1.0
