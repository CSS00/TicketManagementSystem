using Microsoft.EntityFrameworkCore;

namespace TicketManagementSystem.Models;

public class TicketContext : DbContext
{
    public TicketContext(DbContextOptions<TicketContext> options)
        : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; } = null!;

    public bool TicketExists(Guid? id)
    {
        return this.Tickets.Any(e => e.Id == id);
    }
}