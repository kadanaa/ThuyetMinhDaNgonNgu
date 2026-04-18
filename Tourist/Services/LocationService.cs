using System.Diagnostics;

namespace Tourist.Services
{
    public class LocationService : ILocationService
    {
        private decimal _simulatedLatitude = 21.02851100m; // Hà Nội default
        private decimal _simulatedLongitude = 105.85420000m;

        private readonly List<(string name, decimal latitude, decimal longitude)> _predefinedLocations = new()
        {
            ("Tháp Rùa - Hà Nội", 21.02851100m, 105.85420000m),
            ("Phố Cổ Hà Nội", 21.02900000m, 105.85400000m),
            ("Hồ Gươm", 21.02851100m, 105.85420000m),
            ("Hoàn Kiếm Lake", 21.02800000m, 105.85450000m),
            ("Chợ Đồm - Hà Nội", 21.02750000m, 105.85350000m),
            ("Bến Thuyền Ngô Môn", 21.02820000m, 105.85480000m),
            ("Nhà Thờ Lớn Hà Nội", 21.02940000m, 105.85500000m),
            ("Bảo Tàng Hà Nội", 21.03050000m, 105.86200000m),
            ("Văn Miếu - Quốc Tử Giám", 21.02680000m, 105.83520000m),
            ("Đền Quán Thánh", 21.02500000m, 105.83200000m),
        };

        public Task<(decimal latitude, decimal longitude)> GetCurrentLocationAsync()
        {
            return Task.FromResult((_simulatedLatitude, _simulatedLongitude));
        }

        public void SetSimulatedLocation(decimal latitude, decimal longitude)
        {
            _simulatedLatitude = Math.Round(latitude, 8, MidpointRounding.AwayFromZero);
            _simulatedLongitude = Math.Round(longitude, 8, MidpointRounding.AwayFromZero);
            Debug.WriteLine($"Location set to: {latitude}, {longitude}");
        }

        public Task<List<(string name, decimal latitude, decimal longitude)>> GetPredefinedLocationsAsync()
        {
            return Task.FromResult(_predefinedLocations);
        }
    }
}
