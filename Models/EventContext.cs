using Microsoft.EntityFrameworkCore;

namespace TicketManagementSystem.Models;

public class EventContext : DbContext
{
    public EventContext(DbContextOptions<EventContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;

    public bool EventExists(Guid? id)
    {
        return this.Events.Any(e => e.Id == id);
    }
}