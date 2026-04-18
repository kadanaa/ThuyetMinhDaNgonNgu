using Tourist.Models;

namespace Tourist.Services
{
    public interface IPoiService
    {
        /// <summary>
        /// Trả về các POI mà tourist đang đứng trong vùng phủ sóng (distance <= POI.Radius).
        /// Không cần truyền radius — mỗi POI tự có Radius riêng.
        /// </summary>
        Task<List<PointOfInterest>> GetNearbyPOIsAsync(decimal latitude, decimal longitude);

        Task<List<PointOfInterest>> GetAllApprovedPOIsAsync();
        Task<PointOfInterest> GetPOIDetailsAsync(int poiId);
        Task<POITranslation> GetPOITranslationAsync(int poiId, string languageCode);
        Task<int> CountPOIsAsync();
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

    public interface ITouristIdentityService
    {
        Task RegisterCurrentDeviceSessionAsync();
        Task UpdateCurrentLocationAsync(decimal latitude, decimal longitude);
    }
}