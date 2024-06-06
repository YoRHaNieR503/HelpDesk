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
                UserId = ObtenerIdUsuarioLogueado() // Establecer el ID del usuario logueado
            };

            return View(ticket);
        }




        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Title,Description,CreateDate,CloseDate,StatusId,UserId,SupporterId,AdminId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Asegurar que los campos que no se deben modificar estén protegidos
                ticket.AdminId = null;
                ticket.SupporterId = null;
                ticket.CloseDate = null;

                // Obtener el ID del usuario logueado y asignarlo al ticket
                ticket.UserId = ObtenerIdUsuarioLogueado();

                _context.Add(ticket);
                await _context.SaveChangesAsync();
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

        // GET: Tickets1/Delete/5
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

            return View(ticket);
        }

        // POST: Tickets1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Ticket.FindAsync(id);
            if (ticket != null)
            {
                _context.Ticket.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _context.Ticket.Any(e => e.id == id);
        }
    }
}
