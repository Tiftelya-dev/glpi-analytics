using System.Text.Json.Serialization;

namespace TicketService.DTOs;

public class GlpiTicketDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("date_creation")]
    public string DateCreation { get; set; }

    [JsonPropertyName("date_mod")]
    public string DateMod { get; set; }
}