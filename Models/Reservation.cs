namespace TicketManagementSystem.Models
{
    public class Reservation
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public Guid UserId { get; set; }
        public List<Guid> TicketIds { get; set; }
        public DateTime ConfirmedAt { get; set; }
        public DateTime ReservedUntil { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        public Reservation(Guid eventId, Guid userId, List<Guid> ticketIds)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            UserId = userId;
            TicketIds = ticketIds;
            ReservedUntil = DateTime.UtcNow.AddMinutes(15);
        }

        public bool Expired()
        {
            return this.ReservedUntil.ToUniversalTime() < DateTime.UtcNow;
        }
    }
    public enum ReservationStatus
    {
        Active,
        Expired,
        Confirmed,
        Cancelled
    }

    public class ReservationResponse
    {
        public Guid ReservationId { get; set; }
        public Reservation Reservation { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        public ReservationResponse(Reservation reservation)
        {
           
        }
    }
}