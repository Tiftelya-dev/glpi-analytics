namespace TicketService.Models;

public class Ticket
{
    public int Id { get; set; }
    public int GlpiId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;       // Incident / Request
    public string Priority { get; set; } = string.Empty;   // Critical / High / Moderate / Low
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}