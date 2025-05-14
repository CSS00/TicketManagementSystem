using System;
using System.Collections.Generic;

namespace TicketManagementSystem.Models
{
    public enum Role {
        RegularUser,
        Admin
    }

    public class User
    {
        public Guid Id { get; set; } // Unique identifier for the user
        public Role Role { get; set; } = Role.RegularUser; // User's role
        public string FirstName { get; set; } = string.Empty; // User's first name
        public string LastName { get; set; } = string.Empty; // User's last name
        public string Email { get; set; } = string.Empty; // User's email address
        public string Password { get; set; } = string.Empty; // User's password (hashed in production)
        public List<Ticket> BookedTickets { get; set; } // List of tickets booked by the user

        public User(string firstName, string lastName, string email, string password, Role? role)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            Role = role.HasValue ? role.Value : Role.RegularUser;
            BookedTickets = new List<Ticket>();
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
