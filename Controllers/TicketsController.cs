using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Controllers
{
    public class TicketsController : Controller
    {
        private readonly TicketDbContext _context;
        private readonly IConfiguration _configuration;

        public TicketsController(TicketDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            return View(await _context.Ticket.ToListAsync());
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Obtener los nombres de las entidades relacionadas
            ViewData["StatusName"] = _context.Status.FirstOrDefault(s => s.Id == ticket.StatusId)?.name ?? "No definido";
            ViewData["UserName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.UserId)?.FullName ?? "No definido";
            ViewData["SupporterName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.SupporterId)?.FullName ?? "No definido";
            ViewData["AdminName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.AdminId)?.FullName ?? "No definido";

            return View(ticket);
        }


        // GET: Tickets/Create
        public IActionResult Create()
        {
            try
            {
                var listaUsers = (from a in _context.Account
                                  where a.RoleId == 3
                                  select new SelectListItem { Value = a.Id.ToString(), Text = a.FullName }).ToList();

                ViewData["listUsers"] = listaUsers;

                var listaAdmins = (from a in _context.Account
                                   where a.RoleId == 1
                                   select new SelectListItem { Value = a.Id.ToString(), Text = a.FullName }).ToList();

                ViewData["listAdmins"] = listaAdmins;

                // Poblar el combobox para SupporterId
                var listaSupporters = (from a in _context.Account
                                       where a.RoleId == 2
                                       select new SelectListItem { Value = a.Id.ToString(), Text = a.FullName }).ToList();

                ViewData["listSupporters"] = listaSupporters;

                // Poblar el combobox para StatusId
                var listaStatus = (from s in _context.Status
                                   select new SelectListItem { Value = s.Id.ToString(), Text = s.name }).ToList();

                ViewData["listStatus"] = listaStatus;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al obtener los datos: " + ex.Message;
            }

            return View();
        }




        // POST: Tickets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Title,Description,CreateDate,CloseDate,StatusId,UserId,SupporterId,AdminId")] Ticket ticket, string Comentario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();

                Comentarios comentarioObj = null;
                if (!string.IsNullOrEmpty(Comentario))
                {
                    comentarioObj = new Comentarios
                    {
                        Comentario = Comentario,
                        TicketId = ticket.id
                    };
                    _context.Add(comentarioObj);
                    await _context.SaveChangesAsync();
                }

                // Enviar correo electrónico al usuario que creó el ticket
                var user = await _context.Account.FindAsync(ticket.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    string asunto = ticket.Title;
                    string mensaje = $"Se ha creado un nuevo ticket con los siguientes detalles:\n\n" +
                                     $"Título: {ticket.Title}\n" +
                                     $"Descripción: {ticket.Description}\n\n" +
                                     "Pronto será contactado por un soporte.";
                    await EnviarCorreoAsync(user.Email, asunto, mensaje);
                }

                // Enviar correo electrónico al admin o support
                if (ticket.AdminId.HasValue)
                {
                    var admin = await _context.Account.FindAsync(ticket.AdminId.Value);
                    if (admin != null && !string.IsNullOrEmpty(admin.Email))
                    {
                        string asunto = $"Nuevo ticket asignado: {ticket.Title}";
                        string mensaje = $"Se ha creado un nuevo ticket con los siguientes detalles:\n\n" +
                                         $"Título: {ticket.Title}\n" +
                                         $"Descripción: {ticket.Description}\n" +
                                         $"Comentario: {(comentarioObj != null ? comentarioObj.Comentario : "N/A")}\n" +
                                         $"Asignado a: {admin.Username}\n\n" +
                                         $"El ticket fue creado por: {user?.Username}.";
                        await EnviarCorreoAsync(admin.Email, asunto, mensaje);
                    }
                }
                else if (ticket.SupporterId.HasValue)
                {
                    var supporter = await _context.Account.FindAsync(ticket.SupporterId.Value);
                    if (supporter != null && !string.IsNullOrEmpty(supporter.Email))
                    {
                        string asunto = $"Nuevo ticket asignado: {ticket.Title}";
                        string mensaje = $"Se ha creado un nuevo ticket con los siguientes detalles:\n\n" +
                                         $"Título: {ticket.Title}\n" +
                                         $"Descripción: {ticket.Description}\n" +
                                         $"Comentario: {(comentarioObj != null ? comentarioObj.Comentario : "N/A")}\n" +
                                         $"Asignado a: {supporter.Username}\n\n" +
                                         $"El ticket fue creado por: {user?.Username}.";
                        await EnviarCorreoAsync(supporter.Email, asunto, mensaje);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }



        // Método para enviar correo electrónico
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


        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Obtener las listas necesarias para los dropdowns
            var statusList = _context.Status.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.name
            }).ToList();
            ViewBag.listStatus = statusList;

            var userList = _context.Account.Where(a => a.RoleId == 3).Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.FullName
            }).ToList();
            ViewBag.listUsers = userList;

            var supporterList = _context.Account.Where(a => a.RoleId == 2).Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.FullName
            }).ToList();
            ViewBag.listSupporters = supporterList;

            var adminList = _context.Account.Where(a => a.RoleId == 1).Select(ad => new SelectListItem
            {
                Value = ad.Id.ToString(),
                Text = ad.FullName
            }).ToList();
            ViewBag.listAdmins = adminList;

            return View(ticket);
        }



        // POST: Tickets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,Title,Description,CreateDate,CloseDate,StatusId,UserId,SupporterId,AdminId")] Ticket ticket)
        {
            if (id != ticket.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }


        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Ticket
                .FirstOrDefaultAsync(m => m.id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Obtener los nombres de las entidades relacionadas
            ViewData["StatusName"] = _context.Status.FirstOrDefault(s => s.Id == ticket.StatusId)?.name ?? "No definido";
            ViewData["UserName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.UserId)?.FullName ?? "No definido";
            ViewData["SupporterName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.SupporterId)?.FullName ?? "No definido";
            ViewData["AdminName"] = _context.Account.FirstOrDefault(a => a.Id == ticket.AdminId)?.FullName ?? "No definido";

            return View(ticket);
        }


        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Buscar y eliminar el comentario asociado al ticket
            var comentario = await _context.Comentarios.FirstOrDefaultAsync(c => c.TicketId == id);
            if (comentario != null)
            {
                _context.Comentarios.Remove(comentario);
            }

            _context.Ticket.Remove(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }




        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.id == id);
        }
    }
}
