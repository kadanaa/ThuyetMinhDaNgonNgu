using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Tourist.Data;

namespace Tourist.Services
{
    public class TouristIdentityService : ITouristIdentityService
    {
        private const string DeviceIdKey = "tourist_device_id";
        private const int OfflineAfterMinutes = 1;
        private static readonly HttpClient _httpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(4)
        };

        private readonly ThuyetMinhDbContext _context;

        public TouristIdentityService(ThuyetMinhDbContext context)
        {
            _context = context;
        }

        public async Task RegisterCurrentDeviceSessionAsync()
        {
            try
            {
                await UpsertSessionAsync(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TouristIdentity] Cannot register session: {ex}");
            }
        }

        public async Task HeartbeatAsync()
        {
            try
            {
                await UpsertSessionAsync(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TouristIdentity] Cannot heartbeat: {ex}");
            }
        }

        public async Task UpdateCurrentLocationAsync(decimal latitude, decimal longitude)
        {
            try
            {
                await UpsertSessionAsync(latitude, longitude);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TouristIdentity] Cannot update location: {ex}");
            }
        }

        private async Task UpsertSessionAsync(decimal? latitude, decimal? longitude)
        {
            var deviceId = await GetOrCreateDeviceIdAsync();
            var ipAddress = await ResolveIpAddressAsync();
            var platform = DeviceInfo.Current.Platform.ToString();

            var pDeviceId = new SqlParameter("@DeviceId", SqlDbType.NVarChar, 100) { Value = deviceId };
            var pIpAddress = new SqlParameter("@IPAddress", SqlDbType.NVarChar, 50) { Value = (object)ipAddress ?? DBNull.Value };
            var pPlatform = new SqlParameter("@Platform", SqlDbType.NVarChar, 30) { Value = (object)platform ?? DBNull.Value };
            var pCurrentLatitude = new SqlParameter("@CurrentLatitude", SqlDbType.Decimal)
            {
                Precision = 10,
                Scale = 8,
                Value = latitude.HasValue ? latitude.Value : DBNull.Value
            };
            var pCurrentLongitude = new SqlParameter("@CurrentLongitude", SqlDbType.Decimal)
            {
                Precision = 11,
                Scale = 8,
                Value = longitude.HasValue ? longitude.Value : DBNull.Value
            };
            var pIsActive = new SqlParameter("@IsActive", SqlDbType.Bit) { Value = true };
            var pOfflineAfterMinutes = new SqlParameter("@OfflineAfterMinutes", SqlDbType.Int) { Value = OfflineAfterMinutes };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC [dbo].[sp_UpsertTouristSession] @DeviceId, @IPAddress, @Platform, @CurrentLatitude, @CurrentLongitude, @IsActive, @OfflineAfterMinutes",
                pDeviceId,
                pIpAddress,
                pPlatform,
                pCurrentLatitude,
                pCurrentLongitude,
                pIsActive,
                pOfflineAfterMinutes);

            Debug.WriteLine($"[TouristIdentity] Heartbeat device={deviceId}, ip={ipAddress}, lat={latitude}, lon={longitude}");
        }

        private static async Task<string> GetOrCreateDeviceIdAsync()
        {
            string? deviceId = null;

            try
            {
                deviceId = await SecureStorage.Default.GetAsync(DeviceIdKey);
            }
            catch
            {
                // ignore and fallback to Preferences
            }

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = Preferences.Default.Get(DeviceIdKey, string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                return deviceId;
            }

            deviceId = $"tourist-{Guid.NewGuid():N}";

            try
            {
                await SecureStorage.Default.SetAsync(DeviceIdKey, deviceId);
            }
            catch
            {
                Preferences.Default.Set(DeviceIdKey, deviceId);
            }

            return deviceId;
        }

        private static async Task<string> ResolveIpAddressAsync()
        {
            var localIp = GetLocalIpv4Address();

            try
            {
                var publicIp = (await _httpClient.GetStringAsync("https://api.ipify.org")).Trim();
                if (!string.IsNullOrWhiteSpace(publicIp))
                {
                    return publicIp;
                }
            }
            catch
            {
                // ignore and fallback to local IP
            }

            return localIp;
        }

        private static string GetLocalIpv4Address()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var address in host.AddressList)
                {
                    if (address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(address))
                    {
                        return address.ToString();
                    }
                }
            }
            catch
            {
                // ignored
            }

            return "unknown";
        }
    }
}
