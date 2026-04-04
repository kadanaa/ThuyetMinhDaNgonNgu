using Tourist.Models;

namespace Tourist.Services
{
    public interface IPoiService
    {
        Task<List<PointOfInterest>> GetNearbyPOIsAsync(decimal latitude, decimal longitude, float radiusKm);
        Task<List<PointOfInterest>> GetAllApprovedPOIsAsync();
        Task<PointOfInterest> GetPOIDetailsAsync(int poiId);
        Task<POITranslation> GetPOITranslationAsync(int poiId, string languageCode);
    }

    public interface ITranslationService
    {
        Task<string> TranslateTextAsync(string text, string targetLanguageCode);
        Task<List<Language>> GetAvailableLanguagesAsync();
        Task<Language> GetLanguageByCodeAsync(string languageCode);
    }

    public interface ITtsService
    {
        Task SpeakAsync(string text, string languageCode);
        Task<bool> IsSupportedAsync(string languageCode);
    }

    public interface ILocationService
    {
        Task<(decimal latitude, decimal longitude)> GetCurrentLocationAsync();
        void SetSimulatedLocation(decimal latitude, decimal longitude);
        Task<List<(string name, decimal latitude, decimal longitude)>> GetPredefinedLocationsAsync();
    }
}
