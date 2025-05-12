namespace TicketManagementSystem.Models
{
    public class Seat
    {
        public string Section { get; set; }
        public string Row { get; set; }
        public string SeatNumber { get; set; }

        public Seat(string section, string row, string seatNumber)
        {
            Section = section;
            Row = row;
            SeatNumber = seatNumber;
        }

        public override string ToString()
        {
            return $"Section: {Section}, Row: {Row}, Seat: {SeatNumber}";
        }
    }
}