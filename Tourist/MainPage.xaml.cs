using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Tourist.Data;
using Tourist.Models;
using Tourist.Services;

namespace Tourist;

public partial class MainPage : ContentPage
{
    private readonly IPoiService _poiService;
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILocationService _locationService;
    private readonly ThuyetMinhDbContext _context;

    private List<PointOfInterest> _currentPois = new();
    private List<Language> _availableLanguages = new();
    private Language _selectedLanguage;
    private PointOfInterest _selectedPoi;
    private readonly Dictionary<Pin, PointOfInterest> _pinToPoiMap = new();
    private decimal _currentLat;
    private decimal _currentLon;
    private double _startY;
    private Circle _selectedLocationCircle;
    private const double BottomSheetExpandedY = 0;
    private const double BottomSheetCollapsedY = 280;
    private const double BottomSheetSnapThresholdY = 140;

    public MainPage(
        IPoiService poiService,
        ITranslationService translationService,
        ITtsService ttsService,
        ILocationService locationService,
        ThuyetMinhDbContext context)
    {
        InitializeComponent();
        _poiService = poiService;
        _translationService = translationService;
        _ttsService = ttsService;
        _locationService = locationService;
        _context = context;

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // Kiem tra ket noi DB truoc tien
        await CheckDbConnectionAsync();

        try
        {
            var locations = await _locationService.GetPredefinedLocationsAsync();
            LocationPicker.ItemsSource = locations.Select(l => l.name).ToList();
            if (locations.Count > 0)
            {
                LocationPicker.SelectedIndex = 0;
                _locationService.SetSimulatedLocation(locations[0].latitude, locations[0].longitude);
            }

            _availableLanguages = await _translationService.GetAvailableLanguagesAsync();
            if (_availableLanguages.Count == 0)
                _availableLanguages = GetDefaultLanguages();

            LanguagePicker.ItemsSource = _availableLanguages
                .Select(l => $"{l.LanguageName} ({l.LanguageCode})")
                .ToList();

            var defaultIndex = _availableLanguages.FindIndex(l => l.LanguageCode == "en");
            LanguagePicker.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            _selectedLanguage = _availableLanguages[LanguagePicker.SelectedIndex];

            await SearchPoisAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Init] Error: {ex.Message}");
            await DisplayAlert("Loi", $"Khong the khoi tao ung dung: {ex.Message}", "OK");
        }
    }

    // ----------------------------------------------------------------
    // KIEM TRA KET NOI DATABASE
    // Hien badge xanh/do goc tren man hinh
    // ----------------------------------------------------------------
    //private async Task CheckDbConnectionAsync()
    //{
    //    try
    //    {
    //        // SetStatus: dang kiem tra
    //        SetDbStatus(status: "checking");

    //        // Lenh don gian nhat de test connection: dem so ban ghi
    //        var count = await _context.Database.ExecuteSqlRawAsync("SELECT 1");

    //        // Neu khong throw exception = ket noi thanh cong
    //        // Lay them thong tin so luong POI de hien thi
    //        var poiCount = await _context.PointsOfInterest.CountAsync();
    //        var langCount = await _context.Languages.CountAsync();

    //        SetDbStatus("ok", $"DB OK  {poiCount} POI  {langCount} ngon ngu");
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"[DB Check] Failed: {ex.Message}");

    //        // Lay thong bao loi ngan gon de hien len badge
    //        var shortMsg = ex.Message.Length > 40
    //            ? ex.Message[..40] + "..."
    //            : ex.Message;

    //        SetDbStatus("error", $"Loi DB: {shortMsg}");
    //    }
    //}

    private async Task CheckDbConnectionAsync()
    {
        try
        {
            SetDbStatus(status: "checking");

            // ✅ SỬA Ở ĐÂY
            var canConnect = await _context.Database.CanConnectAsync();

            if (!canConnect)
                throw new Exception("Khong the ket noi database");

            var poiCount = await _context.PointsOfInterest.CountAsync();
            var langCount = await _context.Languages.CountAsync();

            //// THÊM: dùng raw SQL để lấy list
            //var rawList = await _context.PointsOfInterest
            //    .FromSqlRaw("SELECT * FROM PointsOfInterest")
            //    .ToListAsync();

            //// THÊM: dùng EF bình thường
            //var efList = await _context.PointsOfInterest.ToListAsync();

            //await MainThread.InvokeOnMainThreadAsync(() =>
            //    DisplayAlert("Debug",
            //        $"CountAsync = {poiCount}\n" +
            //        $"FromSqlRaw count = {rawList.Count}\n" +
            //        $"ToListAsync count = {efList.Count}", "OK"));

            SetDbStatus("ok", $"ACTIVE");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DB] EXCEPTION: {ex}");
            SetDbStatus("error", ex.Message);
        }
    }

    // ----------------------------------------------------------------
    // Cap nhat mau dot va text cua badge
    // status: "checking" | "ok" | "error"
    // ----------------------------------------------------------------
    private void SetDbStatus(string status, string message = null)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (status)
            {
                case "checking":
                    DbStatusDot.Color = Color.FromArgb("#AAAAAA");
                    DbStatusLabel.Text = "Dang ket noi...";
                    DbStatusLabel.TextColor = Color.FromArgb("#555555");
                    break;

                case "ok":
                    DbStatusDot.Color = Color.FromArgb("#43A047");   // xanh la
                    DbStatusLabel.Text = message ?? "Ket noi thanh cong";
                    DbStatusLabel.TextColor = Color.FromArgb("#2E7D32");
                    // Tu dong an badge sau 5 giay
                    Task.Delay(10000).ContinueWith(_ =>
                        MainThread.BeginInvokeOnMainThread(() =>
                            DbStatusBadge.IsVisible = false));
                    break;

                case "error":
                    DbStatusDot.Color = Color.FromArgb("#E53935");   // do
                    DbStatusLabel.Text = message ?? "Loi ket noi DB";
                    DbStatusLabel.TextColor = Color.FromArgb("#C62828");
                    break;
            }
        });
    }

    // ----------------------------------------------------------------
    // Doi vi tri gia lap
    // ----------------------------------------------------------------
    private async void OnLocationChanged(object sender, EventArgs e)
    {
        try
        {
            if (LocationPicker.SelectedIndex < 0) return;
            var locations = await _locationService.GetPredefinedLocationsAsync();
            if (LocationPicker.SelectedIndex < locations.Count)
            {
                var loc = locations[LocationPicker.SelectedIndex];
                _locationService.SetSimulatedLocation(loc.latitude, loc.longitude);
                await SearchPoisAsync();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[LocationChanged] {ex.Message}");
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex >= 0 &&
            LanguagePicker.SelectedIndex < _availableLanguages.Count)
            _selectedLanguage = _availableLanguages[LanguagePicker.SelectedIndex];
    }

    private async void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        try
        {
            var latitude = (decimal)e.Location.Latitude;
            var longitude = (decimal)e.Location.Longitude;

            _locationService.SetSimulatedLocation(latitude, longitude);
            _currentLat = latitude;
            _currentLon = longitude;

            UpdateSelectedLocationDot(latitude, longitude);

            await SearchPoisAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[MapClicked] {ex.Message}");
        }
    }

    private void UpdateSelectedLocationDot(decimal latitude, decimal longitude)
    {
        if (_selectedLocationCircle != null)
            touristMap.MapElements.Remove(_selectedLocationCircle);

        _selectedLocationCircle = new Circle
        {
            Center = new Location((double)latitude, (double)longitude),
            Radius = new Distance(12),
            StrokeColor = Color.FromArgb("#2962FF"),
            StrokeWidth = 2,
            FillColor = Color.FromArgb("#802962FF")
        };

        touristMap.MapElements.Add(_selectedLocationCircle);
    }

    // ----------------------------------------------------------------
    // Tim POI: distance(tourist, POI) <= POI.Radius
    // ----------------------------------------------------------------
    private async Task SearchPoisAsync()
    {
        try
        {
            LoadingOverlay.IsVisible = true;
            SearchButton.IsEnabled = false;
            PoiCollectionView.ItemsSource = null;

            (_currentLat, _currentLon) = await _locationService.GetCurrentLocationAsync();

            touristMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location((double)_currentLat, (double)_currentLon),
                    Distance.FromKilometers(1)
                )
            );

            touristMap.Pins.Clear();
            _pinToPoiMap.Clear();

            _currentPois = await _poiService.GetNearbyPOIsAsync(_currentLat, _currentLon);

            int poicount = await _poiService.CountPOIsAsync();
            //var count = await _context.PointsOfInterest.CountAsync();
            //var list = await _context.PointsOfInterest.ToListAsync();

            //// Thêm dòng này tạm thời để debug:
            //System.Diagnostics.Debug.WriteLine(
            //    $"[DEBUG] Tìm tại ({_currentLat:F6}, {_currentLon:F6}) → {_currentPois.Count} POI");
            //if (_currentPois.Count == 0)
            //    await DisplayAlert("Debug",
            //        $"Query tại ({_currentLat:F6}, {_currentLon:F6})\nKết quả: 0 POI\nKiểm tra Output window", "OK");

            //System.Diagnostics.Debug.WriteLine(
            //    $"[DEBUG] Tìm tại ({_currentLat:F6}, {_currentLon:F6}) → {_currentPois.Count} POI");
            //if (_currentPois.Count == 0)
            //    await DisplayAlert("Debug",
            //        $"CountAsync = {count}, List.Count = {list.Count}\nKết quả: 0 POI\nKiểm tra Output window", "OK");

            foreach (var poi in _currentPois)
            {
                var pin = new Pin
                {
                    Label = poi.POIName,
                    Address = poi.Category ?? string.Empty,
                    Location = new Location((double)poi.Latitude, (double)poi.Longitude),
                    Type = PinType.Place
                };

                pin.MarkerClicked += OnMapPinClicked;
                _pinToPoiMap[pin] = poi;
                touristMap.Pins.Add(pin);
            }

            PoiCollectionView.ItemsSource = _currentPois;

            if (_currentPois.Count > 0)
                await BottomSheet.TranslateTo(0, BottomSheetExpandedY, 220, Easing.CubicOut);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Search] {ex}");
            await DisplayAlert("Loi", $"Loi tim kiem: {ex}", "OK");
        }
        finally
        {
            LoadingOverlay.IsVisible = false;
            SearchButton.IsEnabled = true;
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e) => await SearchPoisAsync();
    private async void OnRefresh(object sender, EventArgs e) => await SearchPoisAsync();

    // ----------------------------------------------------------------
    // Click POI trong sidebar
    // ----------------------------------------------------------------
    private async void OnPoiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PointOfInterest poi) return;
        await FocusPoiAsync(poi);
    }

    private async void OnMapPinClicked(object sender, PinClickedEventArgs e)
    {
        if (sender is not Pin pin)
            return;

        if (_pinToPoiMap.TryGetValue(pin, out var poi))
            await FocusPoiAsync(poi);

        e.HideInfoWindow = true;
    }

    private async Task FocusPoiAsync(PointOfInterest poi)
    {
        _selectedPoi = poi;

        touristMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                new Location((double)poi.Latitude, (double)poi.Longitude),
                Distance.FromKilometers(0.2)
            )
        );

        await BottomSheet.TranslateTo(0, BottomSheetCollapsedY, 220, Easing.CubicOut);
        await ShowPoiInfoSheetAsync(poi);
    }

    private void OnZoomInClicked(object sender, EventArgs e)
    {
        ZoomMap(true);
    }

    private void OnZoomOutClicked(object sender, EventArgs e)
    {
        ZoomMap(false);
    }

    private void ZoomMap(bool zoomIn)
    {
        if (touristMap.VisibleRegion == null)
            return;

        var center = touristMap.VisibleRegion.Center;
        var currentRadiusKm = Math.Max((touristMap.VisibleRegion.LatitudeDegrees * 111d) / 2d, 0.03d);
        var targetRadiusKm = zoomIn
            ? currentRadiusKm / 1.6d
            : currentRadiusKm * 1.6d;

        targetRadiusKm = Math.Clamp(targetRadiusKm, 0.03d, 200d);

        touristMap.MoveToRegion(MapSpan.FromCenterAndRadius(
            center,
            Distance.FromKilometers(targetRadiusKm)
        ));
    }

    private async Task ShowPoiInfoSheetAsync(PointOfInterest poi)
    {
        try
        {
            var langCode = _selectedLanguage?.LanguageCode ?? "en";
            var translation = await _poiService.GetPOITranslationAsync(poi.POIId, langCode);
            var name = translation?.TranslatedName ?? poi.POIName;
            var description = translation?.TranslatedDescription ?? poi.Description ?? "Chua co mo ta.";
            var langName = _selectedLanguage?.LanguageName ?? "English";

            var message = $"Danh muc: {poi.Category}\n" +
                          $"Dia chi: {poi.Address}\n" +
                          (string.IsNullOrEmpty(poi.PhoneNumber) ? "" : $"Dien thoai: {poi.PhoneNumber}\n") +
                          $"\nMo ta ({langName}):\n{description}";

            var action = await DisplayActionSheet(
                title: $"[POI] {name}",
                cancel: "Dong",
                destruction: null,
                "Thuyet Minh ngay",
                "Xem them thong tin"
            );

            if (action == "Thuyet Minh ngay")
                await SpeakPoiAsync(poi);
            else if (action == "Xem them thong tin")
                await DisplayAlert($"[POI] {name}", message, "Dong");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShowInfo] {ex.Message}");
        }
    }

    // ----------------------------------------------------------------
    // Thuyet minh TTS
    // ----------------------------------------------------------------
    private async void OnSpeak(object sender, EventArgs e)
    {
        if (_selectedPoi == null)
        {
            await DisplayAlert("Thong bao", "Vui long chon mot dia diem truoc.", "OK");
            return;
        }
        await SpeakPoiAsync(_selectedPoi);
    }

    private async Task SpeakPoiAsync(PointOfInterest poi)
    {
        try
        {
            SpeakButton.IsEnabled = false;
            SpeakButton.Text = "Dang phat...";

            var langCode = _selectedLanguage?.LanguageCode ?? "en";
            var existing = await _poiService.GetPOITranslationAsync(poi.POIId, langCode);

            string textToSpeak;
            if (existing != null && !string.IsNullOrEmpty(existing.TranslatedDescription))
                textToSpeak = existing.TranslatedDescription;
            else
                textToSpeak = await _translationService.TranslateTextAsync(poi.Description ?? "", langCode);

            await _ttsService.SpeakAsync(textToSpeak, langCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Speak] {ex.Message}");
            await DisplayAlert("Loi", $"Khong the phat am thanh: {ex.Message}", "OK");
        }
        finally
        {
            SpeakButton.IsEnabled = true;
            SpeakButton.Text = "Thuyet Minh";
        }
    }

    // ----------------------------------------------------------------
    // Fallback languages
    // ----------------------------------------------------------------
    private List<Language> GetDefaultLanguages() => new()
    {
        new() { LanguageId = 1,  LanguageCode = "vi", LanguageName = "Tieng Viet",       IsActive = true },
        new() { LanguageId = 2,  LanguageCode = "en", LanguageName = "English",           IsActive = true },
        new() { LanguageId = 3,  LanguageCode = "es", LanguageName = "Espanol",           IsActive = true },
        new() { LanguageId = 4,  LanguageCode = "fr", LanguageName = "Francais",          IsActive = true },
        new() { LanguageId = 5,  LanguageCode = "de", LanguageName = "Deutsch",           IsActive = true },
        new() { LanguageId = 6,  LanguageCode = "ja", LanguageName = "Nhat",              IsActive = true },
        new() { LanguageId = 7,  LanguageCode = "zh", LanguageName = "Trung",             IsActive = true },
        new() { LanguageId = 8,  LanguageCode = "ko", LanguageName = "Han Quoc",          IsActive = true },
        new() { LanguageId = 9,  LanguageCode = "th", LanguageName = "Thai",              IsActive = true },
        new() { LanguageId = 10, LanguageCode = "id", LanguageName = "Indonesia",         IsActive = true },
    };

    // ----------------------------------------------------------------
    // Pan gesture Bottom Sheet
    // ----------------------------------------------------------------
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = BottomSheet.TranslationY;
                break;
            case GestureStatus.Running:
                BottomSheet.TranslationY = Math.Clamp(_startY + e.TotalY, BottomSheetExpandedY, BottomSheetCollapsedY);
                break;
            case GestureStatus.Completed:
                _ = BottomSheet.TranslationY > BottomSheetSnapThresholdY
                    ? BottomSheet.TranslateTo(0, BottomSheetCollapsedY, 180, Easing.CubicOut)
                    : BottomSheet.TranslateTo(0, BottomSheetExpandedY, 180, Easing.CubicOut);
                break;
        }
    }

    private async void OnBottomSheetHandleTapped(object sender, TappedEventArgs e)
    {
        var target = BottomSheet.TranslationY > BottomSheetSnapThresholdY
            ? BottomSheetExpandedY
            : BottomSheetCollapsedY;

        await BottomSheet.TranslateTo(0, target, 180, Easing.CubicOut);
    }
}