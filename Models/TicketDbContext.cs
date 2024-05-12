using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;

namespace HelpDesk.Models
{
    public class TicketDbContext:DbContext
    {

        public TicketDbContext(DbContextOptions options) : base(options) 
        {
            
        }

        public DbSet<Account> Account { get; set; }
        public DbSet<Comentarios> Comentarios { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<Status> Status { get; set; }
        public DbSet<Ticket> Ticket { get; set; }

    }
}
