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

    private double startY;

    public MainPage()
    {
        InitializeComponent();

        var app = MauiProgram.CreateMauiApp();
        _poiService = app.Services.GetService<IPoiService>();
        _translationService = app.Services.GetService<ITranslationService>();
        _ttsService = app.Services.GetService<ITtsService>();
        _locationService = app.Services.GetService<ILocationService>();

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var locations = await _locationService.GetPredefinedLocationsAsync();
            LocationPicker.ItemsSource = locations.Select(l => l.name).ToList();
            if (locations.Count > 0)
                LocationPicker.SelectedIndex = 0;

            _availableLanguages = await _translationService.GetAvailableLanguagesAsync();
            if (_availableLanguages.Count == 0)
                _availableLanguages = GetDefaultLanguages();

            LanguagePicker.ItemsSource = _availableLanguages.Select(l => $"{l.LanguageName} ({l.LanguageCode})").ToList();
            if (_availableLanguages.Count > 0)
            {
                LanguagePicker.SelectedIndex = 0;
                _selectedLanguage = _availableLanguages[0];
            }

            await OnSearchClickedAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in InitializeAsync: {ex.Message}");
            await DisplayAlert("Lỗi", $"Không thể khởi tạo ứng dụng: {ex.Message}", "OK");
        }
    }

    private async void OnLocationChanged(object sender, EventArgs e)
    {
        if (LocationPicker.SelectedIndex < 0) return;

        var locations = await _locationService.GetPredefinedLocationsAsync();
        if (LocationPicker.SelectedIndex < locations.Count)
        {
            var location = locations[LocationPicker.SelectedIndex];
            _locationService.SetSimulatedLocation(location.latitude, location.longitude);
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        if (LanguagePicker.SelectedIndex >= 0 && LanguagePicker.SelectedIndex < _availableLanguages.Count)
        {
            _selectedLanguage = _availableLanguages[LanguagePicker.SelectedIndex];
        }
    }

    private async Task OnSearchClickedAsync()
    {
        try
        {
            if (!float.TryParse(RadiusEntry.Text, out var radius) || radius <= 0)
            {
                await DisplayAlert("Lỗi", "Vui lòng nhập bán kính hợp lệ (> 0)", "OK");
                return;
            }

            SearchButton.IsEnabled = false;
            SearchButton.Text = "Đang tìm kiếm...";
            PoiCollectionView.ItemsSource = null;

            var (lat, lon) = await _locationService.GetCurrentLocationAsync();

            touristMap.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Location((double)lat, (double)lon),
                    Distance.FromMiles(0.5)
                )
            );

            touristMap.Pins.Clear();

            _currentPois = await _poiService.GetNearbyPOIsAsync(lat, lon, radius);

            foreach (var poi in _currentPois)
            {
                var pin = new Pin
                {
                    Location = new Location((double)poi.Latitude, (double)poi.Longitude),
                    Address = poi.Category
                };
                touristMap.Pins.Add(pin);
            }

            PoiCollectionView.ItemsSource = _currentPois;

            SearchButton.IsEnabled = true;
            SearchButton.Text = "Tìm Địa Điểm Gần Đây";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSearchClicked: {ex.Message}");
            await DisplayAlert("Lỗi", $"Lỗi tìm kiếm: {ex.Message}", "OK");
            SearchButton.IsEnabled = true;
            SearchButton.Text = "Tìm Địa Điểm Gần Đây";
        }
    }

    private async void OnSearchClicked(object sender, EventArgs e) => await OnSearchClickedAsync();

    private async void OnShowDetails(object sender, EventArgs e)
    {
        if (PoiCollectionView.SelectedItem is not PointOfInterest poi)
        {
            await DisplayAlert("Thông báo", "Vui lòng chọn một địa điểm", "OK");
            return;
        }

        _selectedPoi = poi;
        var translation = await _poiService.GetPOITranslationAsync(poi.POIId, _selectedLanguage?.LanguageCode ?? "vi");

        var message = $"📍 {poi.POIName}\n\n" +
                     $"📍 Danh Mục: {poi.Category}\n" +
                     $"📍 Địa Chỉ: {poi.Address}\n" +
                     $"📍 Điện Thoại: {poi.PhoneNumber}\n" +
                     $"📍 Website: {poi.Website}\n\n" +
                     $"📋 Mô Tả:\n{translation?.TranslatedDescription ?? poi.Description}";

        await DisplayAlert("Chi Tiết Địa Điểm", message, "OK");
    }

    private async void OnSpeak(object sender, EventArgs e)
    {
        if (PoiCollectionView.SelectedItem is not PointOfInterest poi)
        {
            await DisplayAlert("Thông báo", "Vui lòng chọn một địa điểm", "OK");
            return;
        }

        try
        {
            SpeakButton.IsEnabled = false;
            SpeakButton.Text = "🔊 Đang phát...";

            var translation = await _poiService.GetPOITranslationAsync(poi.POIId, _selectedLanguage?.LanguageCode ?? "vi");
            var textToSpeak = translation?.TranslatedDescription ?? poi.Description;

            await _ttsService.SpeakAsync(textToSpeak, _selectedLanguage?.LanguageCode ?? "vi");

            SpeakButton.IsEnabled = true;
            SpeakButton.Text = "🔊 Nghe Mô Tả";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSpeak: {ex.Message}");
            await DisplayAlert("Lỗi", $"Lỗi phát âm thanh: {ex.Message}", "OK");
            SpeakButton.IsEnabled = true;
            SpeakButton.Text = "🔊 Nghe Mô Tả";
        }
    }

    private async void OnRefresh(object sender, EventArgs e) => await OnSearchClickedAsync();

    private List<Language> GetDefaultLanguages() => new()
    {
        new() { LanguageId = 1, LanguageCode = "vi", LanguageName = "Tiếng Việt", NativeName = "Việt Nam", IsActive = true },
        new() { LanguageId = 2, LanguageCode = "en", LanguageName = "English", NativeName = "United States", IsActive = true },
        new() { LanguageId = 3, LanguageCode = "es", LanguageName = "Español", NativeName = "España", IsActive = true },
        new() { LanguageId = 4, LanguageCode = "fr", LanguageName = "Français", NativeName = "France", IsActive = true },
        new() { LanguageId = 5, LanguageCode = "de", LanguageName = "Deutsch", NativeName = "Deutschland", IsActive = true },
        new() { LanguageId = 6, LanguageCode = "ja", LanguageName = "日本語", NativeName = "日本", IsActive = true },
        new() { LanguageId = 7, LanguageCode = "zh", LanguageName = "中文", NativeName = "中国", IsActive = true },
        new() { LanguageId = 8, LanguageCode = "ko", LanguageName = "한국어", NativeName = "한국", IsActive = true },
        new() { LanguageId = 9, LanguageCode = "th", LanguageName = "ไทย", NativeName = "ไทย", IsActive = true },
        new() { LanguageId = 10, LanguageCode = "id", LanguageName = "Bahasa Indonesia", NativeName = "Indonesia", IsActive = true }
    };

    // Pan gesture handler cho BottomSheet
    void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                startY = BottomSheet.TranslationY;
                break;

            case GestureStatus.Running:
                double newY = startY + e.TotalY;
                if (newY < 0) newY = 0;
                if (newY > 250) newY = 250;
                BottomSheet.TranslationY = newY;
                break;

            case GestureStatus.Completed:
                if (BottomSheet.TranslationY > 150)
                    BottomSheet.TranslateTo(0, 250, 200);
                else
                    BottomSheet.TranslateTo(0, 0, 200);
                break;
        }
    }
}