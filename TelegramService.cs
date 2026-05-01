using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace atc
{
    public static class TelegramService
    {
        private static readonly HttpClient client = new HttpClient();
        
        // Hardcoded bot credentials for @missDiligenceBot
        private const string BOT_ID = "5472804874";
        private const string API_KEY = "AAEGGYjTenN7Tbkrr1rroPO_bibtdIY5YKk";
        private static readonly string BotToken = $"{BOT_ID}:{API_KEY}";

        /// <summary>
        /// Sends a notification when the timer/alarm reaches zero.
        /// Reads settings to check if Telegram is enabled and configured.
        /// </summary>
        public static async Task SendZeroNotificationAsync(string message)
        {
            var settings = await TelegramSettings.LoadAsync();
            if (!settings.IsEnabled || string.IsNullOrWhiteSpace(settings.ChatId))
            {
                Console.WriteLine("[TelegramService] Skipped — disabled or not configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "⏰ Time is up!";
            }

            await SendMessageAsync(settings.ChatId, message);
        }

        /// <summary>
        /// Sends a test notification to verify the connection.
        /// </summary>
        public static async Task<bool> SendTestMessageAsync(string chatId)
        {
            return await SendMessageAsync(chatId, "✅ Test notification from Timer App!");
        }

        /// <summary>
        /// Polls getUpdates for the most recent message matching the given PIN.
        /// Searches from newest to oldest to avoid matching stale messages.
        /// </summary>
        public static async Task<string?> GetChatIdFromPinAsync(string pin)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{BotToken}/getUpdates";
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                return await Task.Run(() => {
                    using JsonDocument doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("result", out JsonElement resultList) || resultList.ValueKind != JsonValueKind.Array) return null;
                    foreach (JsonElement update in resultList.EnumerateArray().Reverse()) {
                        if (!update.TryGetProperty("message", out JsonElement message)) continue;
                        if (!message.TryGetProperty("text", out JsonElement textProp)) continue;
                        if (textProp.GetString()?.Trim() != pin) continue;
                        if (message.TryGetProperty("chat", out JsonElement chat) && chat.TryGetProperty("id", out JsonElement idProp)) return idProp.GetRawText();
                    }
                    return null;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramService] Error detecting chat ID: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Core method that sends a message via the Telegram Bot API.
        /// Returns true on success, false on failure.
        /// </summary>
        private static async Task<bool> SendMessageAsync(string chatId, string text)
        {
            try
            {
                string url = $"https://api.telegram.org/bot{BotToken}/sendMessage";

                var payload = new
                {
                    chat_id = chatId,
                    text = text
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[TelegramService] Sent to {chatId}: {text}");
                    return true;
                }

                string body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[TelegramService] API error {response.StatusCode}: {body}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TelegramService] Error sending message: {ex.Message}");
                return false;
            }
        }
    }
}
