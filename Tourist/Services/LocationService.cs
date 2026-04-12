using System.Diagnostics;

namespace Tourist.Services
{
    public class LocationService : ILocationService
    {
        private decimal _simulatedLatitude = 21.0285m; // Hà Nội default
        private decimal _simulatedLongitude = 105.8542m;

        private readonly List<(string name, decimal latitude, decimal longitude)> _predefinedLocations = new()
        {
            ("Tháp Rùa - Hà Nội", 21.0285m, 105.8542m),
            ("Phố Cổ Hà Nội", 21.0290m, 105.8540m),
            ("Hồ Gươm", 21.0285m, 105.8542m),
            ("Hoàn Kiếm Lake", 21.0280m, 105.8545m),
            ("Chợ Đồm - Hà Nội", 21.0275m, 105.8535m),
            ("Bến Thuyền Ngô Môn", 21.0282m, 105.8548m),
            ("Nhà Thờ Lớn Hà Nội", 21.0294m, 105.8550m),
            ("Bảo Tàng Hà Nội", 21.0305m, 105.8620m),
            ("Văn Miếu - Quốc Tử Giám", 21.0268m, 105.8352m),
            ("Đền Quán Thánh", 21.0250m, 105.8320m),
        };

        public Task<(decimal latitude, decimal longitude)> GetCurrentLocationAsync()
        {
            return Task.FromResult((_simulatedLatitude, _simulatedLongitude));
        }

        public void SetSimulatedLocation(decimal latitude, decimal longitude)
        {
            _simulatedLatitude = latitude;
            _simulatedLongitude = longitude;
            Debug.WriteLine($"Location set to: {latitude}, {longitude}");
        }

        public Task<List<(string name, decimal latitude, decimal longitude)>> GetPredefinedLocationsAsync()
        {
            return Task.FromResult(_predefinedLocations);
        }
    }
}
