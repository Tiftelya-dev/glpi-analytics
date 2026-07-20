using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Models;

namespace TicketService.Services;

// Фоновый сервис — синхронизирует заявки каждые 5 минут
public class TicketSyncService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<TicketSyncService> _logger;

    public TicketSyncService(IServiceProvider services, ILogger<TicketSyncService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Сразу синхронизируем при старте
        await SyncAsync();

        // Затем каждые 5 минут
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            await SyncAsync();
        }
    }

    public async Task SyncAsync()
    {
        using var scope = _services.CreateScope();
        var glpiService = scope.ServiceProvider.GetRequiredService<GlpiService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            _logger.LogInformation("Синхронизация заявок из GLPI...");
            var glpiTickets = await glpiService.GetTicketsAsync();

            foreach (var dto in glpiTickets)
            {
                var existing = await db.Tickets.FirstOrDefaultAsync(t => t.GlpiId == dto.Id);
                var ticket = existing ?? new Ticket();

                ticket.GlpiId      = dto.Id;
                ticket.Title       = dto.Name;
                ticket.Description = dto.Content;
                ticket.Type        = MapType(dto.Type);
                ticket.Priority    = MapPriority(dto.Priority);
                ticket.Status      = MapStatus(dto.Status);
                ticket.CreatedAt   = DateTime.SpecifyKind(
                    DateTime.Parse(dto.DateCreation), DateTimeKind.Utc);
                ticket.UpdatedAt   = DateTime.SpecifyKind(
                    DateTime.Parse(dto.DateMod), DateTimeKind.Utc);

                if (existing == null) db.Tickets.Add(ticket);
            }

            await db.SaveChangesAsync();
            _logger.LogInformation($"Синхронизировано {glpiTickets.Count} заявок.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка синхронизации");
        }
    }

    // 1 = Инцидент, 2 = Запрос
    private static string MapType(int type) => type switch
    {
        1 => "Incident",
        2 => "Request",
        _ => "Unknown"
    };

    // 1-2 = Низкий, 3 = Умеренный, 4-5 = Высокий, 6 = Критичный
    private static string MapPriority(int priority) => priority switch
    {
        1 or 2 => "Low",
        3      => "Moderate",
        4 or 5 => "High",
        6      => "Critical",
        _      => "Unknown"
    };

    private static string MapStatus(int status) => status switch
    {
        1 => "New",
        2 => "Assigned",
        3 => "Planned",
        4 => "Pending",
        5 => "Solved",
        6 => "Closed",
        _ => "Unknown"
    };
}