using System;
using System.IO;
using System.Text.Json;

namespace atc
{
    public class TelegramSettings
    {
        public string ChatId { get; set; } = "";
        public bool IsEnabled { get; set; } = true;

        private static string DirectoryPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "atc");

        private static string FilePath => Path.Combine(DirectoryPath, "telegram_settings.json");

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public bool IsConfigured => !string.IsNullOrWhiteSpace(ChatId);
        private static TelegramSettings? _cachedSettings;


        public static TelegramSettings Load()
        {
            if (_cachedSettings != null) return _cachedSettings;

            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    _cachedSettings = JsonSerializer.Deserialize<TelegramSettings>(json, _jsonOptions)
                                     ?? new TelegramSettings();
                    return _cachedSettings;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TelegramSettings] Error loading: {ex.Message}");
                }
            }
            return new TelegramSettings();
        }

        public static async System.Threading.Tasks.Task<TelegramSettings> LoadAsync()
        {
            if (_cachedSettings != null) return _cachedSettings;

            if (File.Exists(FilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(FilePath);
                    _cachedSettings = JsonSerializer.Deserialize<TelegramSettings>(json, _jsonOptions)
                                     ?? new TelegramSettings();
                    return _cachedSettings;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TelegramSettings] Error loading async: {ex.Message}");
                }
            }
            return new TelegramSettings();
        }

        public static void ClearCache() => _cachedSettings = null;

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath); // No-op if exists
                string json = JsonSerializer.Serialize(this, _jsonOptions);
                File.WriteAllText(FilePath, json);
                _cachedSettings = this; // Update cache
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramSettings] Error saving: {ex.Message}");
            }
        }
    }
}
