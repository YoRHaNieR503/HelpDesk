using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Models
{
    public class TicketDbContext:DbContext
    {

        public TicketDbContext(DbContextOptions options) : base(options) 
        {
            
        }

        public DbSet<Account> clientes { get; set; }

    }
}
