using Microsoft.EntityFrameworkCore;

namespace TicketManagementSystem.Models
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options)
            : base(options)
        {
            this.Users.Add(new User("Jane", "Doe", "abc@gmail.com", "12345"));
            this.Users.Add(new User("Sarah", "L", "sl@gmail.com", "67890"){
                Role = Role.Admin
            });
            this.SaveChanges();
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}