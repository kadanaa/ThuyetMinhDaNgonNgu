using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tourist.Data;
using Tourist.Models;

namespace Tourist.Services
{
    public class PoiService : IPoiService
    {
        private readonly ThuyetMinhDbContext _context;

        public PoiService(ThuyetMinhDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Trả về các POI mà tourist đang đứng BÊN TRONG vùng phủ sóng của POI đó.
        /// Logic: distance(tourist, POI) <= POI.Radius (km)
        /// </summary>
        public async Task<List<PointOfInterest>> GetNearbyPOIsAsync(decimal latitude, decimal longitude)
        {
            try
            {
                // Bước 1: Lấy toàn bộ POI đã được duyệt từ DB
                var allPois = await _context.PointsOfInterest
                    .Where(p => p.IsApproved && p.Status == "Active")
                    .ToListAsync();

                // Bước 2: Lọc trong C# — chỉ giữ POI mà tourist đang đứng trong vùng phủ sóng
                // Điều kiện: khoảng cách từ tourist đến POI <= Radius của POI đó
                var nearbyPois = allPois
                    .Where(p => CalculateDistanceKm(
                        (double)latitude, (double)longitude,
                        (double)p.Latitude, (double)p.Longitude
                    ) <= (double)p.Radius)
                    .OrderBy(p => CalculateDistanceKm(
                        (double)latitude, (double)longitude,
                        (double)p.Latitude, (double)p.Longitude
                    ))
                    .ToList();

                return nearbyPois;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetNearbyPOIsAsync: {ex.Message}");
                return new List<PointOfInterest>();
            }
        }

        public async Task<List<PointOfInterest>> GetAllApprovedPOIsAsync()
        {
            try
            {
                return await _context.PointsOfInterest
                    .Where(p => p.IsApproved && p.Status == "Active")
                    .Include(p => p.Translations)
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAllApprovedPOIsAsync: {ex.Message}");
                return new List<PointOfInterest>();
            }
        }

        public async Task<PointOfInterest> GetPOIDetailsAsync(int poiId)
        {
            try
            {
                return await _context.PointsOfInterest
                    .Include(p => p.Translations)
                    .FirstOrDefaultAsync(p => p.POIId == poiId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPOIDetailsAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<POITranslation> GetPOITranslationAsync(int poiId, string languageCode)
        {
            try
            {
                return await _context.POITranslations
                    .FirstOrDefaultAsync(t => t.POIId == poiId && t.LanguageCode == languageCode);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetPOITranslationAsync: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Công thức Haversine — tính khoảng cách (km) giữa 2 tọa độ trên mặt cầu.
        /// </summary>
        private double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRad(double degrees) => degrees * Math.PI / 180;
    }
}