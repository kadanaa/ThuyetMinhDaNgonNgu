using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tourist.Data;
using Tourist.Models;

namespace Tourist.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ThuyetMinhDbContext _context;

        // Cache: tránh gọi API nhiều lần cho cùng một đoạn text + ngôn ngữ
        private readonly Dictionary<string, string> _cache = new();

        // HttpClient dùng chung — không nên new() mỗi lần gọi
        private static readonly HttpClient _http = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // ----------------------------------------------------------------
        // CẤU HÌNH API DỊCH THUẬT
        // Mặc định dùng MyMemory (miễn phí, không cần key).
        // Nếu muốn dùng Google Translate: đặt _useGoogle = true và điền API key.
        // ----------------------------------------------------------------
        private const bool _useGoogle = false;
        private const string _googleApiKey = "YOUR_GOOGLE_API_KEY_HERE";

        public TranslationService(ThuyetMinhDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Dịch text từ tiếng Việt sang ngôn ngữ đích.
        /// Thứ tự ưu tiên: Cache → API dịch thuật.
        /// Nếu nguồn là tiếng Việt và đích cũng là tiếng Việt thì trả về ngay.
        /// </summary>
        public async Task<string> TranslateTextAsync(string text, string targetLanguageCode)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // Không cần dịch nếu đích là tiếng Việt (ngôn ngữ gốc của dữ liệu)
            if (targetLanguageCode == "vi") return text;

            // Kiểm tra cache trước
            var cacheKey = $"{targetLanguageCode}|{text}";
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                Debug.WriteLine($"[Translation] Cache hit for '{targetLanguageCode}'");
                return cached;
            }

            try
            {
                string translated = _useGoogle
                    ? await TranslateWithGoogleAsync(text, targetLanguageCode)
                    : await TranslateWithMyMemoryAsync(text, targetLanguageCode);

                // Lưu vào cache
                _cache[cacheKey] = translated;
                return translated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Translation] Error: {ex.Message}");
                // Fallback: trả về text gốc tiếng Việt nếu dịch thất bại
                return text;
            }
        }

        // ----------------------------------------------------------------
        // MyMemory API — hoàn toàn miễn phí, không cần đăng ký
        // Giới hạn: 5000 ký tự/ngày với IP thường
        // Docs: https://mymemory.translated.net/doc/spec.php
        // ----------------------------------------------------------------
        private async Task<string> TranslateWithMyMemoryAsync(string text, string targetLang)
        {
            // MyMemory dùng format "vi|en", "vi|ja", ...
            var sourceLang = "vi";
            var langPair = $"{sourceLang}|{targetLang}";
            var encodedText = Uri.EscapeDataString(text);

            var url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair={langPair}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            // Cấu trúc response: { "responseData": { "translatedText": "..." }, "responseStatus": 200 }
            var status = doc.RootElement.GetProperty("responseStatus").GetInt32();
            if (status != 200)
            {
                Debug.WriteLine($"[MyMemory] Non-200 status: {status}");
                return text;
            }

            var translated = doc.RootElement
                .GetProperty("responseData")
                .GetProperty("translatedText")
                .GetString();

            Debug.WriteLine($"[MyMemory] vi→{targetLang}: '{text[..Math.Min(30, text.Length)]}...' → '{translated?[..Math.Min(30, translated?.Length ?? 0)]}...'");

            return translated ?? text;
        }

        // ----------------------------------------------------------------
        // Google Translate API — cần API key từ Google Cloud Console
        // Free tier: 500,000 ký tự/tháng đầu
        // Docs: https://cloud.google.com/translate/docs/reference/rest
        // ----------------------------------------------------------------
        private async Task<string> TranslateWithGoogleAsync(string text, string targetLang)
        {
            if (_googleApiKey == "YOUR_GOOGLE_API_KEY_HERE")
                throw new InvalidOperationException("Chưa cấu hình Google API key trong TranslationService.cs");

            var encodedText = Uri.EscapeDataString(text);
            var url = $"https://translation.googleapis.com/language/translate/v2" +
                      $"?q={encodedText}&source=vi&target={targetLang}&key={_googleApiKey}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            // Cấu trúc response Google:
            // { "data": { "translations": [ { "translatedText": "..." } ] } }
            var translated = doc.RootElement
                .GetProperty("data")
                .GetProperty("translations")[0]
                .GetProperty("translatedText")
                .GetString();

            return translated ?? text;
        }

        // ----------------------------------------------------------------
        // Lấy danh sách ngôn ngữ từ DB
        // ----------------------------------------------------------------
        public async Task<List<Language>> GetAvailableLanguagesAsync()
        {
            try
            {
                return await _context.Languages
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.LanguageName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Translation] GetAvailableLanguagesAsync error: {ex.Message}");
                return new List<Language>();
            }
        }

        public async Task<Language> GetLanguageByCodeAsync(string languageCode)
        {
            try
            {
                return await _context.Languages
                    .FirstOrDefaultAsync(l => l.LanguageCode == languageCode && l.IsActive);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Translation] GetLanguageByCodeAsync error: {ex.Message}");
                return null;
            }
        }
    }
}