# 💻 Tourist App - Code Examples & API Guide

## 📚 Hướng Dẫn Sử Dụng Services

## 1. POI Service (IPoiService)

### Tìm POI Gần Vị Trí

```csharp
// Inject service
private readonly IPoiService _poiService;

// Tìm POI trong bán kính 2km từ tọa độ
var latitude = 21.0285m;
var longitude = 105.8542m;
var radiusKm = 2f;

var nearbyPois = await _poiService.GetNearbyPOIsAsync(latitude, longitude, radiusKm);

foreach (var poi in nearbyPois)
{
    Console.WriteLine($"📍 {poi.POIName} - {poi.Category}");
    Console.WriteLine($"   Địa chỉ: {poi.Address}");
    Console.WriteLine($"   Tọa độ: ({poi.Latitude}, {poi.Longitude})");
}
```

### Lấy Tất Cả POI

```csharp
var allPois = await _poiService.GetAllApprovedPOIsAsync();
Console.WriteLine($"Total POIs: {allPois.Count}");
```

### Chi Tiết POI

```csharp
var poiId = 1;
var poi = await _poiService.GetPOIDetailsAsync(poiId);

if (poi != null)
{
    Console.WriteLine($"Tên: {poi.POIName}");
    Console.WriteLine($"Danh mục: {poi.Category}");
    Console.WriteLine($"Địa chỉ: {poi.Address}");
    Console.WriteLine($"Điện thoại: {poi.PhoneNumber}");
    Console.WriteLine($"Website: {poi.Website}");
    Console.WriteLine($"Translations: {poi.Translations.Count}");
}
```

### Lấy Translation

```csharp
var poiId = 1;
var languageCode = "en"; // English

var translation = await _poiService.GetPOITranslationAsync(poiId, languageCode);

if (translation != null)
{
    Console.WriteLine($"Tên (Tiếng Anh): {translation.TranslatedName}");
    Console.WriteLine($"Mô tả (Tiếng Anh): {translation.TranslatedDescription}");
}
else
{
    Console.WriteLine("Không tìm thấy bản dịch");
}
```

## 2. Location Service (ILocationService)

### Lấy Vị Trí Hiện Tại

```csharp
private readonly ILocationService _locationService;

var (latitude, longitude) = await _locationService.GetCurrentLocationAsync();
Console.WriteLine($"Vị trí hiện tại: {latitude}, {longitude}");
```

### Đặt Vị Trí Dự Phòng

```csharp
// Đặt vị trí Hồ Gươm
_locationService.SetSimulatedLocation(21.0285m, 105.8542m);

// Sau đó lấy vị trí
var (lat, lon) = await _locationService.GetCurrentLocationAsync();
// Result: (21.0285, 105.8542)
```

### Lấy Danh Sách Vị Trí Được Định Nghĩa

```csharp
var locations = await _locationService.GetPredefinedLocationsAsync();

foreach (var (name, lat, lon) in locations)
{
    Console.WriteLine($"{name}: {lat}, {lon}");
}

// Output:
// Tháp Rùa - Hà Nội: 21.0285, 105.8542
// Phố Cổ Hà Nội: 21.0290, 105.8540
// Hồ Gươm: 21.0285, 105.8542
// ...
```

## 3. Translation Service (ITranslationService)

### Dịch Văn Bản

```csharp
private readonly ITranslationService _translationService;

var text = "Đây là một điểm tham quan tuyệt vời";
var targetLanguage = "en"; // English

var translatedText = await _translationService.TranslateTextAsync(text, targetLanguage);
Console.WriteLine($"Gốc: {text}");
Console.WriteLine($"Dịch: {translatedText}");
```

### Lấy Danh Sách Ngôn Ngữ

```csharp
var languages = await _translationService.GetAvailableLanguagesAsync();

Console.WriteLine("Ngôn ngữ khả dụng:");
foreach (var lang in languages)
{
    Console.WriteLine($"  {lang.LanguageCode} - {lang.LanguageName} ({lang.NativeName})");
}

// Output:
// vi - Vietnamese (Việt Nam)
// en - English (United States)
// es - Spanish (España)
// fr - French (France)
// ...
```

### Lấy Thông Tin Ngôn Ngữ

```csharp
var language = await _translationService.GetLanguageByCodeAsync("en");

if (language != null)
{
    Console.WriteLine($"Code: {language.LanguageCode}");
    Console.WriteLine($"Name: {language.LanguageName}");
    Console.WriteLine($"Native: {language.NativeName}");
    Console.WriteLine($"Active: {language.IsActive}");
}
```

## 4. TTS Service (ITtsService)

### Phát Âm Thanh

```csharp
private readonly ITtsService _ttsService;

var text = "Xin chào, đây là Tháp Rùa";
var languageCode = "vi"; // Vietnamese

await _ttsService.SpeakAsync(text, languageCode);
```

### Kiểm Tra Hỗ Trợ Ngôn Ngữ

```csharp
var isSupported = await _ttsService.IsSupportedAsync("en");
Console.WriteLine($"English supported: {isSupported}");

var isSupported2 = await _ttsService.IsSupportedAsync("unknown");
Console.WriteLine($"Unknown supported: {isSupported2}");
```

## 5. Kết Hợp Services

### Ví Dụ 1: Tìm POI Và Phát Âm Thanh

```csharp
// 1. Tìm POI gần vị trí
var location = await _locationService.GetCurrentLocationAsync();
var pois = await _poiService.GetNearbyPOIsAsync(location.latitude, location.longitude, 2f);

// 2. Chọn POI đầu tiên
if (pois.Any())
{
    var firstPoi = pois[0];

    // 3. Lấy bản dịch
    var translation = await _poiService.GetPOITranslationAsync(firstPoi.POIId, "en");
    var description = translation?.TranslatedDescription ?? firstPoi.Description;

    // 4. Phát âm thanh
    await _ttsService.SpeakAsync(description, "en");
}
```

### Ví Dụ 2: Hiển Thị Danh Sách POI Đa Ngôn Ngữ

```csharp
// Lấy danh sách POI
var pois = await _poiService.GetAllApprovedPOIsAsync();

// Lấy ngôn ngữ
var language = await _translationService.GetLanguageByCodeAsync("es");

foreach (var poi in pois)
{
    var translation = await _poiService.GetPOITranslationAsync(poi.POIId, language.LanguageCode);

    var name = translation?.TranslatedName ?? poi.POIName;
    var desc = translation?.TranslatedDescription ?? poi.Description;

    Console.WriteLine($"📍 {name}");
    Console.WriteLine($"   {desc.Substring(0, Math.Min(50, desc.Length))}...");
}
```

### Ví Dụ 3: Workflow Hoàn Chỉnh

```csharp
public class TouristWorkflow
{
    private readonly IPoiService _poiService;
    private readonly ILocationService _locationService;
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;

    public TouristWorkflow(
        IPoiService poiService,
        ILocationService locationService,
        ITranslationService translationService,
        ITtsService ttsService)
    {
        _poiService = poiService;
        _locationService = locationService;
        _translationService = translationService;
        _ttsService = ttsService;
    }

    public async Task ExecuteAsync()
    {
        // 1. Chọn vị trí
        Console.WriteLine("Chọn vị trí...");
        var locations = await _locationService.GetPredefinedLocationsAsync();
        var selectedLocation = locations[0]; // Tháp Rùa
        _locationService.SetSimulatedLocation(selectedLocation.latitude, selectedLocation.longitude);
        Console.WriteLine($"✓ Vị trí: {selectedLocation.name}");

        // 2. Lấy danh sách ngôn ngữ
        Console.WriteLine("\nLấy danh sách ngôn ngữ...");
        var languages = await _translationService.GetAvailableLanguagesAsync();
        Console.WriteLine($"✓ Tìm thấy {languages.Count} ngôn ngữ");

        // 3. Tìm POI gần vị trí
        Console.WriteLine("\nTìm POI gần vị trí...");
        var pois = await _poiService.GetNearbyPOIsAsync(
            selectedLocation.latitude, 
            selectedLocation.longitude, 
            2f);
        Console.WriteLine($"✓ Tìm thấy {pois.Count} POI");

        // 4. Hiển thị POI
        if (pois.Any())
        {
            var poi = pois[0];
            Console.WriteLine($"\n📍 {poi.POIName}");
            Console.WriteLine($"   Danh mục: {poi.Category}");
            Console.WriteLine($"   Địa chỉ: {poi.Address}");

            // 5. Lấy bản dịch
            if (languages.Any())
            {
                var selectedLanguage = languages.FirstOrDefault(l => l.LanguageCode == "en");
                if (selectedLanguage != null)
                {
                    var translation = await _poiService.GetPOITranslationAsync(
                        poi.POIId, 
                        selectedLanguage.LanguageCode);

                    if (translation != null)
                    {
                        Console.WriteLine($"\n📝 Tên (English): {translation.TranslatedName}");
                        Console.WriteLine($"📝 Mô tả: {translation.TranslatedDescription.Substring(0, 50)}...");

                        // 6. Phát âm thanh
                        Console.WriteLine("\n🔊 Phát âm thanh...");
                        await _ttsService.SpeakAsync(translation.TranslatedDescription, selectedLanguage.LanguageCode);
                        Console.WriteLine("✓ Phát xong");
                    }
                }
            }
        }

        Console.WriteLine("\n✅ Workflow hoàn tất");
    }
}
```

## 6. Haversine Distance Calculation

Công thức tính khoảng cách được sử dụng trong `PoiService`:

```csharp
private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
{
    const double R = 6371; // Earth's radius in km
    var dLat = ToRad(lat2 - lat1);
    var dLon = ToRad(lon2 - lon1);
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    return R * c;
}

private double ToRad(double degrees) => degrees * Math.PI / 180;
```

**Ví dụ:**
```csharp
// Khoảng cách từ Tháp Rùa đến Hồ Gươm
var distance = CalculateDistance(21.0285, 105.8542, 21.0285, 105.8542);
Console.WriteLine($"Khoảng cách: {distance:F2} km");
```

## 7. Unit Testing

### Test PoiService

```csharp
[TestClass]
public class PoiServiceTests
{
    private IPoiService _poiService;
    private ThuyetMinhDbContext _context;

    [TestInitialize]
    public void Setup()
    {
        var connectionString = "..."; // Test connection
        _context = new ThuyetMinhDbContext(connectionString);
        _poiService = new PoiService(_context);
    }

    [TestMethod]
    public async Task GetNearbyPOIs_WithValidLocation_ReturnsResults()
    {
        // Arrange
        var latitude = 21.0285m;
        var longitude = 105.8542m;
        var radius = 5f;

        // Act
        var result = await _poiService.GetNearbyPOIsAsync(latitude, longitude, radius);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task GetPOITranslation_WithValidLanguage_ReturnsTranslation()
    {
        // Arrange
        var poiId = 1;
        var languageCode = "en";

        // Act
        var result = await _poiService.GetPOITranslationAsync(poiId, languageCode);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("en", result.LanguageCode);
    }
}
```

## 8. Error Handling

### Pattern Recommended

```csharp
try
{
    var pois = await _poiService.GetNearbyPOIsAsync(latitude, longitude, radiusKm);

    if (!pois.Any())
    {
        // Log warning
        Debug.WriteLine("No POIs found");
        await DisplayAlert("Thông báo", "Không tìm thấy địa điểm", "OK");
        return;
    }

    // Process results
}
catch (ArgumentException ex)
{
    // Invalid parameters
    Debug.WriteLine($"Invalid parameters: {ex.Message}");
    await DisplayAlert("Lỗi", "Tham số không hợp lệ", "OK");
}
catch (InvalidOperationException ex)
{
    // Database connection error
    Debug.WriteLine($"Database error: {ex.Message}");
    await DisplayAlert("Lỗi", "Lỗi kết nối cơ sở dữ liệu", "OK");
}
catch (Exception ex)
{
    // Unexpected error
    Debug.WriteLine($"Unexpected error: {ex.Message}");
    await DisplayAlert("Lỗi", $"Lỗi không mong muốn: {ex.Message}", "OK");
}
```

## 📖 Tài Liệu Tham Khảo

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [TextToSpeech API](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/text-to-speech)

---

**Last Updated**: January 2025
