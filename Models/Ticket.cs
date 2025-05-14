namespace TicketManagementSystem.Models
{
    public class Ticket
    {
        public Guid? Id { get; set; }
        public Guid EventId { get; init; }
        public decimal Price { get; set; }
        public string Section { get; set; }
        public string Row { get; set; }
        public string SeatNumber { get; set; }
        public DateTime CreatedAt { get; init; }
        public Guid? ReservationId { get; set; }
        public Ticket(Guid eventId, decimal price, string section, string row, string seatNumber)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Price = price;
            Section = section;
            Row = row;
            SeatNumber = seatNumber;
            CreatedAt = DateTime.UtcNow;
        }
    }
}