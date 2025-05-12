using System;
using System.Collections.Generic;

namespace TicketManagementSystem.Models
{
    public class User
    {
        public Guid Id { get; set; } // Unique identifier for the user
        public string FirstName { get; set; } = string.Empty; // User's first name
        public string LastName { get; set; } = string.Empty; // User's last name
        public string Email { get; set; } = string.Empty; // User's email address
        public string Password { get; set; } = string.Empty; // User's password (hashed in production)
        public DateTime DateOfBirth { get; set; } // User's date of birth
        public List<Ticket> BookedTickets { get; set; } // List of tickets booked by the user

        public User()
        {
            Id = Guid.NewGuid();
            BookedTickets = new List<Ticket>();
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
