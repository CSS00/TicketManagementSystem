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
        public string FirstName { get; set; } // User's first name
        public string LastName { get; set; } // User's last name
        public string Email { get; set; } // User's email address
        public string Password { get; set; } // User's password (hashed in production)

        public User(string firstName, string lastName, string email, string password)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }
}
