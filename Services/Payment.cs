using System;

namespace TicketManagementSystem.Services
{
    public class Payment
    {
        public string CardNumber { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        public Payment(string cardNumber, string name, string address)
        {
            CardNumber = cardNumber;
            Name = name;
            Address = address;
        }
    }
}