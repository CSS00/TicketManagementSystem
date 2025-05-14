namespace TicketManagementSystem.Models
{
    public class Event
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }

        public Event(string name, string venue, string description, DateTime date)
        {
            Id = Guid.NewGuid();
            Name = name;
            Venue = venue;
            Description = description;
            Date = date;
        }
    }

    public class EventDetails
    {
        public Guid? EventId { get; set; }
        public string Name { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public int Capacity { get; set; }

        public EventDetails(Event evt, int totalTickets)
        {
            EventId = evt.Id;
            Name = evt.Name;
            Venue = evt.Venue;
            Description = evt.Description;
            Date = evt.Date;
            Capacity = totalTickets;
        }
    }
}