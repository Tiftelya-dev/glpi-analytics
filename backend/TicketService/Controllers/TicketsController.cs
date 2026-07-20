using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketService.Data;
using TicketService.Services;

namespace TicketService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TicketSyncService _syncService;

    public TicketsController(AppDbContext db, TicketSyncService syncService)
    {
        _db = db;
        _syncService = syncService;
    }

    // GET /api/tickets — все заявки
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? type,
        [FromQuery] string? priority)
    {
        var query = _db.Tickets.AsQueryable();

        if (!string.IsNullOrEmpty(type))
            query = query.Where(t => t.Type == type);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority);

        return Ok(await query.OrderByDescending(t => t.CreatedAt).ToListAsync());
    }

    // GET /api/tickets/stats/period?start=2026-01-01&end=2026-07-01
    // Для линейного графика — количество заявок по дням
    [HttpGet("stats/period")]
    public async Task<IActionResult> GetByPeriod(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end)
    {
        var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        var endUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);

        var data = await _db.Tickets
            .Where(t => t.CreatedAt >= startUtc && t.CreatedAt <= endUtc)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(data);
    }

    // GET /api/tickets/stats/categories
    // Для гистограммы — количество по типу и приоритету
    [HttpGet("stats/categories")]
    public async Task<IActionResult> GetByCategories(
        [FromQuery] DateTime? start,
        [FromQuery] DateTime? end)
    {
        var startUtc = start.HasValue
            ? DateTime.SpecifyKind(start.Value, DateTimeKind.Utc)
            : DateTime.UtcNow.AddDays(-30);

        var endUtc = end.HasValue
            ? DateTime.SpecifyKind(end.Value, DateTimeKind.Utc)
            : DateTime.UtcNow;

        var filtered = _db.Tickets
            .Where(t => t.CreatedAt >= startUtc && t.CreatedAt <= endUtc);

        var byType = await filtered
            .GroupBy(t => t.Type)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        var byPriority = await filtered
            .GroupBy(t => t.Priority)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(new { byType, byPriority });
    }

    // POST /api/tickets/sync — ручной запуск синхронизации
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        await _syncService.SyncAsync();
        return Ok(new { message = "Синхронизация выполнена" });
    }
}