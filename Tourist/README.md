# 🗺️ Tourist App - .NET MAUI Du Lịch Thông Minh

## Tổng Quan

**Tourist App** là ứng dụng du lịch đa nền tảng được xây dựng bằng **.NET MAUI** cho phép du khách:
- 🗺️ Xem các địa điểm du lịch (POI) trên bản đồ
- 📍 Tìm các địa điểm gần vị trí hiện tại
- 🌐 Xem thông tin chi tiết bằng nhiều ngôn ngữ
- 🔊 Nghe mô tả địa điểm bằng text-to-speech

## ✨ Tính Năng Chính

### 1. **Chọn Vị Trí Dự Phòng**
- Chọn từ danh sách vị trí được định nghĩa sẵn
- Mặc định: Hà Nội
- Danh sách bao gồm 10 địa điểm trong Hà Nội

### 2. **Tìm Địa Điểm Gần Đây**
- Nhập bán kính tìm kiếm (km)
- Hiển thị tất cả POI trong bán kính đó
- Sắp xếp theo khoảng cách
- Hiển thị số lượng kết quả tìm được

### 3. **Danh Sách POI**
- Sidebar hiển thị danh sách địa điểm
- Thông tin: Tên, Danh mục, Địa chỉ
- Click để chọn địa điểm

### 4. **Xem Chi Tiết**
- Hiển thị thông tin đầy đủ:
  - Tên địa điểm
  - Danh mục
  - Địa chỉ
  - Số điện thoại
  - Website
  - Mô tả chi tiết

### 5. **Hỗ Trợ Đa Ngôn Ngữ**
- Hỗ trợ 10 ngôn ngữ:
  - Vietnamese (Tiếng Việt)
  - English
  - Spanish (Español)
  - French (Français)
  - German (Deutsch)
  - Japanese (日本語)
  - Chinese (中文)
  - Korean (한국어)
  - Thai (ไทย)
  - Indonesian (Bahasa Indonesia)

### 6. **Text-to-Speech**
- Phát âm thanh mô tả địa điểm
- Hỗ trợ multiple languages
- Nút "Nghe Mô Tả" trong giao diện

## 🏗️ Kiến Trúc Ứng Dụng

```
Tourist/
├── Models/
│   └── PointOfInterest.cs      # Entity models (POI, Translation, Language)
├── Data/
│   └── ThuyetMinhDbContext.cs  # Entity Framework DbContext
├── Services/
│   ├── IServices.cs             # Service interfaces
│   ├── PoiService.cs            # POI data access
│   ├── LocationService.cs       # Location selection
│   ├── TranslationService.cs    # Multi-language support
│   └── TtsService.cs            # Text-to-speech
├── MainPage.xaml                # UI layout
├── MainPage.xaml.cs             # UI logic & view model
├── MauiProgram.cs               # DI configuration
└── appsettings.json             # Configuration
```

## 🔧 Cấu Hình

### Connection String

Edit `MauiProgram.cs` hàm `GetConnectionString()`:

```csharp
private static string GetConnectionString()
{
    return "Server=YOUR_SERVER;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;";
}
```

**Hoặc** sử dụng `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

### Các Vị Trí Được Định Nghĩa

File: `MainPage.xaml.cs` - `GetPredefinedLocations()`

```csharp
("Tháp Rùa - Hà Nội", 21.0285m, 105.8542m),
("Phố Cổ Hà Nội", 21.0290m, 105.8540m),
("Hồ Gươm", 21.0285m, 105.8542m),
// ... + 7 more locations
```

## 📊 Các Thành Phần Chính

### 1. **Models** (`Models/PointOfInterest.cs`)
```csharp
public class PointOfInterest
{
    public int POIId { get; set; }
    public string POIName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public ICollection<POITranslation> Translations { get; set; }
}

public class POITranslation
{
    public int TranslationId { get; set; }
    public string LanguageCode { get; set; }
    public string TranslatedName { get; set; }
    public string TranslatedDescription { get; set; }
}
```

### 2. **Services**

#### **IPoiService**
```csharp
Task<List<PointOfInterest>> GetNearbyPOIsAsync(decimal lat, decimal lon, float radiusKm);
Task<PointOfInterest> GetPOIDetailsAsync(int poiId);
Task<POITranslation> GetPOITranslationAsync(int poiId, string languageCode);
```

#### **ILocationService**
```csharp
Task<(decimal latitude, decimal longitude)> GetCurrentLocationAsync();
void SetSimulatedLocation(decimal latitude, decimal longitude);
Task<List<(string name, decimal latitude, decimal longitude)>> GetPredefinedLocationsAsync();
```

#### **ITranslationService**
```csharp
Task<string> TranslateTextAsync(string text, string targetLanguageCode);
Task<List<Language>> GetAvailableLanguagesAsync();
```

#### **ITtsService**
```csharp
Task SpeakAsync(string text, string languageCode);
Task<bool> IsSupportedAsync(string languageCode);
```

### 3. **UI Components** (`MainPage.xaml`)

- **Grid Layout**: 4 rows
  - Row 0: Location & Language selection
  - Row 1: Search button
  - Row 2: Map + POI List sidebar
  - Row 3: Action buttons

- **Controls**:
  - `Picker`: Location selection
  - `Picker`: Language selection
  - `Entry`: Radius input
  - `CollectionView`: POI list
  - `Button`: Search, Details, Speak, Refresh

## 🔄 Dependency Injection

File: `MauiProgram.cs`

```csharp
builder.Services.AddScoped(_ => new ThuyetMinhDbContext(connectionString));
builder.Services.AddScoped<IPoiService, PoiService>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<ITtsService, TtsService>();
builder.Services.AddScoped<ILocationService, LocationService>();
```

Injection vào `MainPage.xaml.cs`:

```csharp
private readonly IPoiService _poiService;
private readonly ILocationService _locationService;
// ... (services được lấy từ DI container)
```

## 🎯 Luồng Công Việc

```
1. Khởi động App
   ↓
2. Load vị trí được định nghĩa sẵn
   ↓
3. Load danh sách ngôn ngữ
   ↓
4. Tìm POI gần vị trí mặc định
   ↓
5. Hiển thị trong danh sách & bản đồ
   ↓
6. Người dùng: Chọn vị trí → Nhập bán kính → Click "Tìm Địa Điểm"
   ↓
7. Tìm kiếm → Hiển thị kết quả
   ↓
8. Chọn POI → Xem chi tiết / Nghe mô tả
```

## 📱 Nền Tảng Hỗ Trợ

- **Android** (API 21+)
- **iOS** (15.0+)
- **macOS** (Catalyst 15.0+)
- **Windows** (10.0.17763+)

## 🚀 Chạy Ứng Dụng

### Yêu Cầu
- .NET 10 SDK
- Visual Studio 2022 với MAUI workload
- SQL Server database (ThuyetMinhDaNgonNgu) đã được tạo

### Build & Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run on Windows
dotnet run -f net10.0-windows10.0.19041.0

# Run on Android (nếu có emulator)
dotnet run -f net10.0-android
```

## 📦 Dependencies

```xml
<PackageReference Include="Microsoft.Maui.Controls" Version="10.0.20" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="10.0.0" />
```

## 🐛 Troubleshooting

### Lỗi Connection String
```
❌ Unable to connect to database
✅ Kiểm tra connection string trong MauiProgram.cs
✅ Verify SQL Server is running
✅ Verify database exists: ThuyetMinhDaNgonNgu
```

### Lỗi Text-to-Speech
```
❌ TextToSpeech not working
✅ Kiểm tra platform-specific permissions
✅ Verify language is supported
✅ Check device settings
```

### Lỗi Collection View
```
❌ Items not showing in list
✅ Verify POI data is returned
✅ Check ItemsSource binding
✅ Verify POI count > 0
```

## 🔮 Future Enhancements

- [ ] Thực tế Google Maps integration
- [ ] Real-time GPS location
- [ ] POI favorites/bookmarks
- [ ] Search by category
- [ ] Offline maps
- [ ] Review/rating system
- [ ] Route planning

## 📄 License

MIT License

## 👨‍💻 Developer

Developed with ❤️ using .NET MAUI

---

**Last Updated**: January 2025
**Version**: 1.0.0
