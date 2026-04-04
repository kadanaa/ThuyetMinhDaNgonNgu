using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Tourist.Data;
using Tourist.Models;

namespace Tourist.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly ThuyetMinhDbContext _context;
        private readonly Dictionary<string, string> _cachedTranslations = new();

        public TranslationService(ThuyetMinhDbContext context)
        {
            _context = context;
        }

        public async Task<string> TranslateTextAsync(string text, string targetLanguageCode)
        {
            if (string.IsNullOrEmpty(text) || targetLanguageCode == "vi")
                return text;

            try
            {
                // For now, return the original text
                // In production, integrate with Azure Translator, Google Translate, or other API
                Debug.WriteLine($"Translating: {text} to {targetLanguageCode}");

                // Check cache first
                string cacheKey = $"{text}_{targetLanguageCode}";
                if (_cachedTranslations.TryGetValue(cacheKey, out var cached))
                    return cached;

                // TODO: Call actual translation API here
                // For now, return original text as placeholder
                _cachedTranslations[cacheKey] = text;
                return text;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in TranslateTextAsync: {ex.Message}");
                return text;
            }
        }

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
                Debug.WriteLine($"Error in GetAvailableLanguagesAsync: {ex.Message}");
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
                Debug.WriteLine($"Error in GetLanguageByCodeAsync: {ex.Message}");
                return null;
            }
        }
    }
}
