using System.Diagnostics;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Tourist.Models;
using Tourist.Services;

namespace Tourist;

public partial class MainPage : ContentPage
{
    private readonly IPoiService _poiService;
    private readonly ITranslationService _translationService;
    private readonly ITtsService _ttsService;
    private readonly ILocationService _locationService;

    private List<PointOfInterest> _currentPois = new();
    private List<Language> _availableLanguages = new();
    private Language _selectedLanguage;
    private PointOfInterest _selectedPoi;

    // Giữ tọa độ hiện tại để dùng lại khi cần
    private decimal _currentLat;
    private decimal _currentLon;

    private double _startY;

    // ----------------------------------------------------------------
    // Constructor nhận service qua DI
    // ----------------------------------------------------------------
    public MainPage(
        IPoiService poiService,
        ITranslationService translationService,
        ITtsService ttsService,
        ILocationService locationService)
    {
        InitializeComponent();
        _poiService = poiService;
        _translationService = translationService;
        _ttsService = ttsService;
        _locationService = locationService;

        _ = InitializeAsync();
    }

    // ----------------------------------------------------------------
    // Khởi tạo ban đầu
    // ----------------------------------------------------------------
    private async Task InitializeAsync()
    {
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

            // Mặc định chọn tiếng Anh
            var defaultIndex = _availableLanguages.FindIndex(l => l.LanguageCode == "en");
            LanguagePicker.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            _selectedLanguage = _availableLanguages[LanguagePicker.SelectedIndex];

            await SearchPoisAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Init] Error: {ex.Message}");
            await DisplayAlert("Lỗi", $"Không thể khởi tạo ứng dụng: {ex.Message}", "OK");
        }
    }

    // ----------------------------------------------------------------
    // Đổi vị trí giả lập → tìm lại POI
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
            Debug.WriteLine($"[LocationChanged] Error: {ex.Message}");
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex >= 0 &&
            LanguagePicker.SelectedIndex < _availableLanguages.Count)
            _selectedLanguage = _availableLanguages[LanguagePicker.SelectedIndex];
    }

    // ----------------------------------------------------------------
    // Tìm POI theo logic: distance(tourist, POI) <= POI.Radius
    // ----------------------------------------------------------------
    private async Task SearchPoisAsync()
    {
        try
        {
            // Hiện loading overlay
            LoadingOverlay.IsVisible = true;
            SearchButton.IsEnabled = false;
            PoiCollectionView.ItemsSource = null;

            (_currentLat, _currentLon) = await _locationService.GetCurrentLocationAsync();

            // Di chuyển map đến vị trí tourist
            touristMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location((double)_currentLat, (double)_currentLon),
                    Distance.FromKilometers(1)
                )
            );

            touristMap.Pins.Clear();

            _currentPois = await _poiService.GetNearbyPOIsAsync(_currentLat, _currentLon);

            foreach (var poi in _currentPois)
            {
                touristMap.Pins.Add(new Pin
                {
                    Label = poi.POIName,
                    Address = poi.Category ?? string.Empty,
                    Location = new Location((double)poi.Latitude, (double)poi.Longitude),
                    Type = PinType.Place
                });
            }

            PoiCollectionView.ItemsSource = _currentPois;

            // Kéo bottom sheet lên để thấy danh sách nếu có POI
            if (_currentPois.Count > 0)
                await BottomSheet.TranslateTo(0, 0, 300, Easing.CubicOut);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Search] Error: {ex.Message}");
            await DisplayAlert("Lỗi", $"Lỗi tìm kiếm: {ex.Message}", "OK");
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
    // Click vào POI trong sidebar:
    //   1. Map di chuyển đến POI
    //   2. Bottom sheet thu lại một nửa để thấy map
    //   3. Hiện bảng thông tin chi tiết
    // ----------------------------------------------------------------
    private async void OnPoiSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not PointOfInterest poi) return;
        _selectedPoi = poi;

        // Map di chuyển đến POI (zoom gần hơn)
        touristMap.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                new Location((double)poi.Latitude, (double)poi.Longitude),
                Distance.FromKilometers(0.2)
            )
        );

        // Thu bottom sheet lại một nửa để thấy bản đồ
        await BottomSheet.TranslateTo(0, 180, 250, Easing.CubicOut);

        // Hiện thông tin POI dạng popup
        await ShowPoiInfoSheetAsync(poi);
    }

    // ----------------------------------------------------------------
    // Hiện bảng thông tin POI — dùng ActionSheet thay DisplayAlert
    // để có nút "Thuyết Minh" ngay bên trong
    // ----------------------------------------------------------------
    private async Task ShowPoiInfoSheetAsync(PointOfInterest poi)
    {
        try
        {
            var langCode = _selectedLanguage?.LanguageCode ?? "en";
            var translation = await _poiService.GetPOITranslationAsync(poi.POIId, langCode);

            var name = translation?.TranslatedName ?? poi.POIName;
            var description = translation?.TranslatedDescription ?? poi.Description
                              ?? "Chưa có mô tả.";

            var langName = _selectedLanguage?.LanguageName ?? "English";

            var message = $"🏷️ {poi.Category}  |  📍 r = {poi.Radius:F1} km\n" +
                          $"🏠 {poi.Address}\n" +
                          (string.IsNullOrEmpty(poi.PhoneNumber) ? "" : $"📞 {poi.PhoneNumber}\n") +
                          $"\n── Mô tả ({langName}) ──\n{description}";

            // ActionSheet cho phép chọn "Thuyết Minh" hoặc "Đóng"
            var action = await DisplayActionSheet(
                title: $"📍 {name}",
                cancel: "Đóng",
                destruction: null,
                "🔊 Thuyết Minh ngay",
                "📋 Xem thêm thông tin"
            );

            if (action == "🔊 Thuyết Minh ngay")
                await SpeakPoiAsync(poi);
            else if (action == "📋 Xem thêm thông tin")
                await DisplayAlert($"📍 {name}", message, "Đóng");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ShowInfo] Error: {ex.Message}");
        }
    }

    // ----------------------------------------------------------------
    // Thuyết minh: dịch → đọc TTS
    // ----------------------------------------------------------------
    private async void OnSpeak(object sender, EventArgs e)
    {
        if (_selectedPoi == null)
        {
            await DisplayAlert("Thông báo", "Vui lòng chọn một địa điểm từ danh sách trước.", "OK");
            return;
        }
        await SpeakPoiAsync(_selectedPoi);
    }

    private async Task SpeakPoiAsync(PointOfInterest poi)
    {
        try
        {
            SpeakButton.IsEnabled = false;
            SpeakButton.Text = "🔊 Đang phát...";

            var langCode = _selectedLanguage?.LanguageCode ?? "en";
            var rawDescription = poi.Description ?? string.Empty;

            // Ưu tiên bản dịch sẵn trong DB
            var existing = await _poiService.GetPOITranslationAsync(poi.POIId, langCode);
            string textToSpeak;

            if (existing != null && !string.IsNullOrEmpty(existing.TranslatedDescription))
                textToSpeak = existing.TranslatedDescription;
            else
                textToSpeak = await _translationService.TranslateTextAsync(rawDescription, langCode);

            await _ttsService.SpeakAsync(textToSpeak, langCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Speak] Error: {ex.Message}");
            await DisplayAlert("Lỗi", $"Không thể phát âm thanh: {ex.Message}", "OK");
        }
        finally
        {
            SpeakButton.IsEnabled = true;
            SpeakButton.Text = "🔊 Thuyết Minh";
        }
    }

    // ----------------------------------------------------------------
    // Fallback languages
    // ----------------------------------------------------------------
    private List<Language> GetDefaultLanguages() => new()
    {
        new() { LanguageId = 1,  LanguageCode = "vi", LanguageName = "Tiếng Việt",      NativeName = "Việt Nam",    IsActive = true },
        new() { LanguageId = 2,  LanguageCode = "en", LanguageName = "English",          NativeName = "English",     IsActive = true },
        new() { LanguageId = 3,  LanguageCode = "es", LanguageName = "Español",          NativeName = "España",      IsActive = true },
        new() { LanguageId = 4,  LanguageCode = "fr", LanguageName = "Français",         NativeName = "France",      IsActive = true },
        new() { LanguageId = 5,  LanguageCode = "de", LanguageName = "Deutsch",          NativeName = "Deutschland", IsActive = true },
        new() { LanguageId = 6,  LanguageCode = "ja", LanguageName = "日本語",            NativeName = "日本",        IsActive = true },
        new() { LanguageId = 7,  LanguageCode = "zh", LanguageName = "中文",              NativeName = "中国",        IsActive = true },
        new() { LanguageId = 8,  LanguageCode = "ko", LanguageName = "한국어",            NativeName = "한국",        IsActive = true },
        new() { LanguageId = 9,  LanguageCode = "th", LanguageName = "ไทย",              NativeName = "ไทย",         IsActive = true },
        new() { LanguageId = 10, LanguageCode = "id", LanguageName = "Bahasa Indonesia", NativeName = "Indonesia",   IsActive = true },
    };

    // ----------------------------------------------------------------
    // Pan gesture cho Bottom Sheet
    // ----------------------------------------------------------------
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _startY = BottomSheet.TranslationY;
                break;

            case GestureStatus.Running:
                var newY = Math.Clamp(_startY + e.TotalY, 0, 280);
                BottomSheet.TranslationY = newY;
                break;

            case GestureStatus.Completed:
                // Snap: nếu kéo quá nửa thì thu lại, không thì mở ra
                _ = BottomSheet.TranslationY > 140
                    ? BottomSheet.TranslateTo(0, 280, 200, Easing.CubicOut)
                    : BottomSheet.TranslateTo(0, 0, 200, Easing.CubicOut);
                break;
        }
    }
}