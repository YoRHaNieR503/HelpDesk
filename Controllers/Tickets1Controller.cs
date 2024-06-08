using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models;

namespace HelpDesk.Controllers
{
    public class Tickets1Controller : Controller
    {
        private readonly TicketDbContext _context;

        public Tickets1Controller(TicketDbContext context)
        {
            _context = context;
        }

        // GET: Tickets1
        public async Task<IActionResult> Index()
        {
            // Obtener el nombre de usuario o correo electrónico del usuario autenticado
            var loggedInUserName = User.Identity.Name;

            // Buscar el ID del usuario autenticado
            var loggedInUserId = _context.Account
                .Where(a => a.Username == loggedInUserName || a.Email == loggedInUserName)
                .Select(a => a.Id)
                .FirstOrDefault();

            // Obtener solo los tickets relacionados con el usuario autenticado
            var userTickets = await _context.Ticket
                .Where(t => t.UserId == loggedInUserId)
                .ToListAsync();

            return View(userTickets);
        }

        // GET: Tickets1/Details/5
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

            return View(ticket);
        }


        // Método para obtener el ID del usuario logueado
        private int ObtenerIdUsuarioLogueado()
        {
            var loggedInUserName = User.Identity.Name;
            var loggedInUserId = _context.Account
                .Where(a => a.Username == loggedInUserName || a.Email == loggedInUserName)
                .Select(a => a.Id)
                .FirstOrDefault();
            return loggedInUserId;
        }


        // GET: Tickets1/Create
        public IActionResult Create()
        {
            try
            {
                var listaStatus = (from s in _context.Status
                                   where s.StatusText == "User"
                                   select s).ToList();
                ViewData["listStatus"] = new SelectList(listaStatus, "Id", "name");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al obtener los datos: " + ex.Message;
            }

            var ticket = new Ticket
            {
                CreateDate = DateTime.Now, // Establecer la fecha actual
                UserId = ObtenerIdUsuarioLogueado(), // Establecer el ID del usuario logueado
                StatusId = 1 // Establecer automáticamente el StatusId a 1
            };

            return View(ticket);
        }




        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Title,Description,CreateDate,CloseDate,StatusId,UserId,SupporterId,AdminId")] Ticket ticket, string Comentario)
        {
            if (ModelState.IsValid)
            {
                // Asegurar que los campos que no se deben modificar estén protegidos
                ticket.AdminId = null;
                ticket.SupporterId = null;
                ticket.CloseDate = null;

                // Establecer el StatusId a 1 (o cualquier valor que desees) antes de guardar el ticket
                ticket.StatusId = 1;

                // Obtener el ID del usuario logueado y asignarlo al ticket
                ticket.UserId = ObtenerIdUsuarioLogueado();

                _context.Add(ticket);
                await _context.SaveChangesAsync();

                // Verificar si se proporcionó un comentario y si el TicketId es válido
                if (!string.IsNullOrEmpty(Comentario) && ticket.id > 0)
                {
                    // Crear un nuevo comentario y guardarlo en la base de datos
                    var comentarioObj = new Comentarios
                    {
                        Comentario = Comentario,
                        TicketId = ticket.id
                    };
                    _context.Add(comentarioObj);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }


        // GET: Tickets1/Edit/5
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
            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
