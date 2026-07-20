using System.Text.Json;
using TicketService.DTOs;

namespace TicketService.Services;

public class GlpiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GlpiService> _logger;

    public GlpiService(HttpClient httpClient, IConfiguration config, ILogger<GlpiService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    // Открыть сессию с GLPI — получить session_token
    private async Task<string> InitSessionAsync()
    {
        var appToken = _config["Glpi:AppToken"];
        var userToken = _config["Glpi:UserToken"];

        _logger.LogInformation($"Используем AppToken: '{appToken}', UserToken: '{userToken}'");

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_config["Glpi:BaseUrl"]}/initSession");

        request.Headers.Add("App-Token", appToken);
        request.Headers.Add("Authorization", $"user_token {userToken}");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogInformation($"GLPI ответ: {response.StatusCode} — {content}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"GLPI API ошибка {response.StatusCode}: {content}");

        var json = JsonSerializer.Deserialize<JsonElement>(content);
        return json.GetProperty("session_token").GetString()!;
    }

    // Закрыть сессию
    private async Task KillSessionAsync(string sessionToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"{_config["Glpi:BaseUrl"]}/killSession");

        request.Headers.Add("App-Token", _config["Glpi:AppToken"]);
        request.Headers.Add("Session-Token", sessionToken);

        await _httpClient.SendAsync(request);
    }

    // Получить все заявки из GLPI
    public async Task<List<GlpiTicketDto>> GetTicketsAsync()
    {
        var sessionToken = await InitSessionAsync();
        var allTickets = new List<GlpiTicketDto>();
        int rangeStart = 0;
        int rangeSize = 500;

        try
        {
            while (true)
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_config["Glpi:BaseUrl"]}/Ticket?range={rangeStart}-{rangeStart + rangeSize - 1}");

                request.Headers.Add("App-Token", _config["Glpi:AppToken"]);
                request.Headers.Add("Session-Token", sessionToken);

                var response = await _httpClient.SendAsync(request);

            // 206 = частичный контент (есть ещё страницы)
            // 200 = последняя страница
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    break; // больше нет заявок

                var content = await response.Content.ReadAsStringAsync();

            // GLPI возвращает ошибку в виде массива если нет данных
                if (content.StartsWith("[\"ERROR"))
                    break;

                var page = JsonSerializer.Deserialize<List<GlpiTicketDto>>(content) ?? new List<GlpiTicketDto>();

                if (page.Count == 0)
                    break;

                allTickets.AddRange(page);
                _logger.LogInformation($"Загружено {allTickets.Count} заявок...");

            // Если вернулось меньше чем запрашивали — это последняя страница
                if (page.Count < rangeSize)
                    break;

                rangeStart += rangeSize;
            }
        }
        finally
        {
            await KillSessionAsync(sessionToken);
        }

        return allTickets;
    }
}