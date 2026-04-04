using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Tourist.Services
{
    public class TtsService : ITtsService
    {
        public async Task SpeakAsync(string text, string languageCode)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                Debug.WriteLine($"Speaking: {text} in language: {languageCode}");

                // Use MAUI's built-in TextToSpeech
                await TextToSpeech.SpeakAsync(text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SpeakAsync: {ex.Message}");
            }
        }

        public Task<bool> IsSupportedAsync(string languageCode)
        {
            return Task.FromResult(true);
        }
    }
}
