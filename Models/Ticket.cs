namespace TicketManagementSystem.Models
{
    // public enum Status {
    //     Available,
    //     Reserved
    // }

    public class Ticket
    {
        public Guid? Id { get; set; }
        public Guid EventId { get; init; }
        public decimal Price { get; set; }
        public string Section { get; set; }
        public string Row { get; set; }
        public string SeatNumber { get; set; }
        // public Status Status { get; set; } = Status.Available;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? ReservationId { get; set; }
        public Ticket(Guid eventId, decimal price, string section, string row, string seatNumber)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Price = price;
            Section = section;
            Row = row;
            SeatNumber = seatNumber;
            // Status = Status.Available;
            CreatedAt = DateTime.UtcNow;
        }
    }
}