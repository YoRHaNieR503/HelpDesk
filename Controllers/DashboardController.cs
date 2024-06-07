using HelpDesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Controllers
{
    public class DashboardController : Controller
    {
        private readonly TicketDbContext _context;

        public DashboardController(TicketDbContext context)
        {
            _context = context;
        }


        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var accountsCount = await _context.Account.CountAsync();
            var ticketsCount = await _context.Ticket.CountAsync();

            // Otros datos que puedas necesitar para tus gráficas

            ViewBag.AccountsCount = accountsCount;
            ViewBag.TicketsCount = ticketsCount;

            // Puedes pasar más datos a la vista según tus necesidades

            return View();
        }
    }
}
