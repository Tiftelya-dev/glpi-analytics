using Microsoft.EntityFrameworkCore;
using TicketService.Models;

namespace TicketService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Ticket> Tickets { get; set; }
}