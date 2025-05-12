namespace TicketManagementSystem.Models
{
    public enum Status {
        Available,
        Reserved,
        Booked
    }

    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public decimal Price { get; set; }
        public Seat Seat { get; set; }
        public Status Status { get; set; } = Status.Available;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReservedUntil { get; set; }
        public Guid? ReservedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Ticket(Guid eventId, decimal price, Seat seat)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Price = price;
            Seat = seat;
            Status = Status.Available;
            CreatedAt = DateTime.UtcNow;
        }
    }
}