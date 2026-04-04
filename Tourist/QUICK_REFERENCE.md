# 🚀 Tourist App - Quick Reference Card

## ⚡ 5-Minute Quick Start

### 1️⃣ **Prerequisites Check**
```powershell
# Check .NET version
dotnet --version  # Should be 10.0+

# Check MAUI workload
dotnet workload list | findstr maui
```

### 2️⃣ **Configure Connection**
Edit `Tourist/MauiProgram.cs`:
```csharp
return "Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;";
```

### 3️⃣ **Build & Run**
```powershell
cd Tourist
dotnet build
dotnet run -f net10.0-windows10.0.19041.0
```

### 4️⃣ **Verify**
- ✅ Window opens with title "Du Lịch - Tìm Địa Điểm"
- ✅ Location dropdown shows 10 places
- ✅ Language dropdown shows 10 languages
- ✅ Click "Tìm Địa Điểm Gần Đây" shows POIs

## 📁 Key Files

| File | Purpose | Edit |
|------|---------|------|
| `MauiProgram.cs` | App startup & DI | ✏️ Connection string |
| `MainPage.xaml` | UI Layout | 📐 Add controls |
| `MainPage.xaml.cs` | UI Logic | 🔧 Event handlers |
| `Services/PoiService.cs` | POI data | 💾 Queries |
| `appsettings.json` | Config | ⚙️ Settings |

## 🎯 Common Tasks

### Add a New POI
```sql
-- In SQL Server
INSERT INTO PointsOfInterest 
(POIName, Description, Latitude, Longitude, Category, Address, IsApproved, Status, CreatedDate)
VALUES ('Tên POI', 'Mô tả', 21.0, 105.8, 'Danh mục', 'Địa chỉ', 1, 'Active', GETDATE());

-- Get the new POI ID
SELECT @@IDENTITY AS NewPoiId;

-- Add translations
INSERT INTO POITranslations (POIId, LanguageCode, LanguageName, TranslatedName, TranslatedDescription)
VALUES (NEW_ID, 'en', 'English', 'POI Name', 'Description in English');
```

### Change Default Location
```csharp
// In MainPage.xaml.cs - GetPredefinedLocations()
return new List<(string name, decimal latitude, decimal longitude)>
{
    ("My New Default", 21.1234m, 105.5678m),  // ← Change this line
    // ... rest of locations
};
```

### Add New Language
```csharp
// 1. Database
INSERT INTO Languages (LanguageCode, LanguageName, NativeName, IsActive)
VALUES ('pt', 'Portuguese', 'Brasil', 1);

// 2. Code - Add to GetDefaultLanguages()
new() { LanguageId = 11, LanguageCode = "pt", LanguageName = "Português", NativeName = "Brasil", IsActive = true },
```

### Modify Search Radius
```csharp
// In MainPage.xaml - Change default radius value
<Entry x:Name="RadiusEntry" Placeholder="Bán kính tìm kiếm (km)" Keyboard="Numeric" Text="5" />
//                                                                                       ↑
//                                                                              Change this value
```

## 🔧 Configuration Reference

### Connection String Templates

**Local SQL Server (Windows Auth)**
```
Server=localhost;Database=ThuyetMinhDaNgonNgu;Integrated Security=true;TrustServerCertificate=true;
```

**SQL Server (SQL Auth)**
```
Server=localhost;Database=ThuyetMinhDaNgonNgu;User Id=sa;Password=Password123;TrustServerCertificate=true;
```

**Remote Server**
```
Server=192.168.1.100,1433;Database=ThuyetMinhDaNgonNgu;User Id=user;Password=pass;
```

**Azure SQL Database**
```
Server=yourserver.database.windows.net,1433;Database=ThuyetMinhDaNgonNgu;User Id=user@server;Password=pass;
```

## 🎨 UI Customization Quick Tips

### Change Colors
```xaml
<!-- In MainPage.xaml - Find Button elements -->
<Button BackgroundColor="#2196F3" />   <!-- Blue -->
<Button BackgroundColor="#4CAF50" />   <!-- Green -->
<Button BackgroundColor="#FF9800" />   <!-- Orange -->

<!-- Available colors -->
Red: #F44336
Pink: #E91E63
Purple: #9C27B0
Blue: #2196F3
Cyan: #00BCD4
Green: #4CAF50
Orange: #FF9800
Brown: #795548
Gray: #607D8B
```

### Change Text
```xaml
<!-- In MainPage.xaml - Find Label elements -->
<Label Text="Mặc định Vietnam" />
                    ↑
              Change text here
```

### Change Layout
```xaml
<!-- In MainPage.xaml - Adjust Grid -->
<Grid RowDefinitions="Auto,Auto,*,Auto" ColumnDefinitions="*,250">
                                                        ↑ Change proportions
```

## 📊 Database Quick Commands

### Check Connection
```sql
USE ThuyetMinhDaNgonNgu;
SELECT DB_NAME() AS CurrentDatabase;
```

### View POI Count
```sql
SELECT COUNT(*) AS TotalPOIs FROM PointsOfInterest;
SELECT COUNT(*) AS ApprovedPOIs FROM PointsOfInterest WHERE IsApproved = 1;
```

### View Languages
```sql
SELECT * FROM Languages WHERE IsActive = 1;
```

### View Translations
```sql
SELECT p.POIName, t.LanguageName, t.TranslatedName 
FROM POITranslations t
INNER JOIN PointsOfInterest p ON t.POIId = p.POIId;
```

## 🐛 Debug Tips

### Enable Debug Output
```csharp
// In MauiProgram.cs - Already enabled in #if DEBUG
#if DEBUG
    builder.Logging.AddDebug();  // ← Logs to Output window
#endif
```

### View Debug Messages
Visual Studio → Debug → Windows → Output (Ctrl+Alt+O)

### Common Debug Statements
```csharp
Debug.WriteLine($"POI count: {pois.Count}");
Debug.WriteLine($"Location: {latitude}, {longitude}");
Debug.WriteLine($"Error: {ex.Message}");
```

### Breakpoints
```csharp
// Click left margin in editor to add breakpoint
var pois = await _poiService.GetNearbyPOIsAsync(lat, lon, radius);  // ← Add breakpoint here
```

## 📱 Platform-Specific Notes

### Windows (Primary Development)
- Uses .NET MAUI Windows platform
- Full UI preview in Visual Studio
- No special permissions needed
- Text-to-speech works natively

### Android
- Requires Android 5.0+ (API 21)
- Text-to-speech needs text-to-speech engine installed
- Location permissions in AndroidManifest.xml

### iOS
- Requires iOS 15.0+
- Speech permission in Info.plist
- Microphone access may prompt user

## 🔄 Build & Deploy

### Clean Build
```powershell
dotnet clean
dotnet build
```

### Publish for Windows
```powershell
dotnet publish -f net10.0-windows10.0.19041.0 -c Release
```

### Create Installer
Use Visual Studio Installer Projects or third-party tools like MSIX.

## 📋 Checklist Before Shipping

- [ ] Connection string updated
- [ ] Database backups created
- [ ] All POIs have translations
- [ ] Text-to-speech tested on target language
- [ ] UI tested on different screen sizes
- [ ] Error handling verified
- [ ] Logging enabled for production
- [ ] Security review completed
- [ ] Performance tested (>50 POIs)
- [ ] Unit tests pass

## 🎯 Performance Tips

### Optimize Queries
```csharp
// ✅ Good - Limits results early
.Where(p => p.IsApproved)
.Take(100)
.ToListAsync()

// ❌ Bad - Loads all then filters
.ToListAsync()
.Where(p => p.IsApproved)
.Take(100)
```

### Cache Results
```csharp
// Cache language list (changes rarely)
private static List<Language> _languageCache;

if (_languageCache == null)
{
    _languageCache = await db.Languages.ToListAsync();
}
return _languageCache;
```

### Async All I/O
```csharp
// ✅ Good
await db.PointsOfInterest.ToListAsync();

// ❌ Bad
db.PointsOfInterest.ToList();  // Blocks thread
```

## 📞 Support Resources

| Issue | Resource |
|-------|----------|
| MAUI Questions | https://learn.microsoft.com/dotnet/maui |
| EF Core Docs | https://learn.microsoft.com/ef/core |
| SQL Server | https://learn.microsoft.com/sql/sql-server |
| .NET Docs | https://dotnet.microsoft.com/docs |

## 🎓 Learning Resources

1. **Architecture**: `IMPLEMENTATION_SUMMARY.md`
2. **Setup**: `SETUP_GUIDE.md`
3. **Code Examples**: `CODE_EXAMPLES.md`
4. **Features**: `README.md`

## 💡 Pro Tips

1. **Use XAML Preview**: Real-time UI preview while editing
2. **Async Debugging**: Use Debug → Break All during async operations
3. **Database Profiler**: Profile slow queries in SQL Server
4. **Hot Reload**: Save XAML to see changes without rebuild
5. **Package Cache**: Delete .nuget/packages to force clean restore

## 🚀 Ready to Go!

You now have:
- ✅ Complete Tourist App (.NET MAUI)
- ✅ Full database integration
- ✅ 4 production services
- ✅ Multi-language support (10 languages)
- ✅ Text-to-speech capability
- ✅ Professional error handling
- ✅ Comprehensive documentation
- ✅ Clean, maintainable code

**Happy coding! 🎉**

---

**Last Updated**: January 2025  
**Quick Ref Version**: 1.0  
**Status**: Complete & Ready for Production
