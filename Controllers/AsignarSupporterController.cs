using Microsoft.AspNetCore.Mvc;
using HelpDesk.Models;
using MimeKit;
using System.Configuration;
using MailKit.Net.Smtp;

namespace HelpDesk.Controllers
{
    public class AsignarSupporterController : Controller
    {
        private readonly TicketDbContext _context;
        private readonly IConfiguration _configuration;

        public AsignarSupporterController(TicketDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {

            ViewBag.Tickets = _context.Ticket.Where(t => t.StatusId == 1).ToList();
            ViewBag.Admins = _context.Account.Where(a => a.RoleId == 1).ToList();
            ViewBag.Soportes = _context.Account.Where(a => a.RoleId == 2).ToList();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int ticketId, int? adminId, int? soporteId)
        {
            var ticket = await _context.Ticket.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (adminId != null)
            {
                ticket.AdminId = adminId;
            }
            else if (soporteId != null)
            {
                ticket.SupporterId = soporteId;
            }

            await _context.SaveChangesAsync();

            await EnviarCorreoAsignacion(ticket);

            TempData["SuccessMessage"] = "El ticket ha sido asignado con éxito.";

            // Cargar nuevamente los datos necesarios para la vista
            ViewBag.Tickets = _context.Ticket.Where(t => t.StatusId == 1).ToList();
            ViewBag.Admins = _context.Account.Where(a => a.RoleId == 1).ToList();
            ViewBag.Soportes = _context.Account.Where(a => a.RoleId == 2).ToList();

            // Retorna la vista actualizada
            return View("Index");
        }


        private async Task EnviarCorreoAsignacion(Ticket ticket)
        {
            var user = await _context.Account.FindAsync(ticket.UserId);
            string userName = user?.FullName ?? "Usuario";
            string userEmail = user?.Email ?? "Correo no disponible";
            string ticketTitle = ticket.Title ?? "Sin título";
            string ticketDescription = ticket.Description ?? "Sin descripción";

            string asunto = "Ticket Asignado: " + ticketTitle;
            string mensaje = $"Se le ha asignado el ticket '{ticketTitle}' con ID {ticket.id}.\n\n" +
                             $"Descripción: {ticketDescription}\n\n" +
                             $"Correo del usuario: {userEmail}\n\n" +
                             "Por favor, revise su panel de control para más detalles.";

            if (ticket.AdminId != null)
            {
                var admin = await _context.Account.FindAsync(ticket.AdminId);
                if (admin != null && !string.IsNullOrEmpty(admin.Email))
                {
                    await EnviarCorreoAsync(admin.Email, asunto, mensaje);
                }
            }
            else if (ticket.SupporterId != null)
            {
                var soporte = await _context.Account.FindAsync(ticket.SupporterId);
                if (soporte != null && !string.IsNullOrEmpty(soporte.Email))
                {
                    await EnviarCorreoAsync(soporte.Email, asunto, mensaje);
                }
            }

            if (user != null && !string.IsNullOrEmpty(user.Email))
            {
                await EnviarCorreoAsync(user.Email, asunto, mensaje);
            }
        }

        private async Task EnviarCorreoAsync(string destinatario, string asunto, string mensaje)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("HelpDesk", _configuration["Smtp:Username"]));
            emailMessage.To.Add(new MailboxAddress("", destinatario));
            emailMessage.Subject = asunto;
            emailMessage.Body = new TextPart("plain") { Text = mensaje };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }




    }
}
