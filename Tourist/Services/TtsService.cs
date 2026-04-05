using System.Diagnostics;

namespace Tourist.Services
{
    public class TtsService : ITtsService
    {
        /// <summary>
        /// Đọc text bằng TTS với đúng locale của ngôn ngữ đã chọn.
        /// MAUI TextToSpeech hỗ trợ truyền Locale để chọn giọng đọc.
        /// </summary>
        public async Task SpeakAsync(string text, string languageCode)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                // Lấy toàn bộ locale mà thiết bị hỗ trợ
                var locales = await TextToSpeech.GetLocalesAsync();

                // Tìm locale khớp với languageCode (ví dụ "ja", "en", "vi")
                // So sánh phần Language của locale (bỏ qua country suffix như "en-US", "ja-JP")
                var matchedLocale = locales.FirstOrDefault(l =>
                    l.Language.Equals(languageCode, StringComparison.OrdinalIgnoreCase));

                // Fallback: nếu không tìm thấy đúng locale thì dùng mặc định thiết bị
                var options = new SpeechOptions
                {
                    Locale = matchedLocale, // null = dùng mặc định, không sao
                    Volume = 1.0f,
                    Pitch = 1.0f
                };

                Debug.WriteLine($"[TTS] Speaking in '{languageCode}', " +
                                $"locale found: {matchedLocale?.Language ?? "default"}");

                await TextToSpeech.SpeakAsync(text, options);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[TTS] Error: {ex.Message}");

                // Fallback cuối: đọc không có locale nếu mọi thứ đều lỗi
                try { await TextToSpeech.SpeakAsync(text); }
                catch { /* bỏ qua */ }
            }
        }

        /// <summary>
        /// Kiểm tra thiết bị có hỗ trợ ngôn ngữ này không.
        /// </summary>
        public async Task<bool> IsSupportedAsync(string languageCode)
        {
            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                return locales.Any(l =>
                    l.Language.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }
    }
}