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
    public class Tickets2Controller : Controller
    {
        private readonly TicketDbContext _context;

        public Tickets2Controller(TicketDbContext context)
        {
            _context = context;
        }

        // GET: Tickets2
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
                .Where(t => t.SupporterId == loggedInUserId)
                .ToListAsync();

            return View(userTickets);
        }

        // GET: Tickets2/Details/5
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

        // GET: Tickets2/Create
        public IActionResult Create()
        {
            // Obtener el ID del usuario de soporte actualmente autenticado
            var supporterId = ObtenerIdUsuarioLogueado();// Lógica para obtener el ID del usuario de soporte actualmente autenticado;

            // Crear un nuevo ticket y establecer el SupporterId
            var ticket = new Ticket
            {
                SupporterId = supporterId, // Establecer el SupporterId como el ID del usuario de soporte
                CreateDate = DateTime.Now, // Establecer la fecha actual
                UserId = ObtenerIdUsuarioLogueado() // Establecer el ID del usuario logueado

            };



            return View(ticket);
        }

        // POST: Tickets2/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,Title,Description,CreateDate,CloseDate,StatusId,UserId,SupporterId,AdminId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                // Obtener el ID del usuario de soporte actualmente autenticado
                var supporterId = ObtenerIdUsuarioLogueado(); // Lógica para obtener el ID del usuario de soporte actualmente autenticado;

                // Establecer el SupporterId en el objeto Ticket
                ticket.SupporterId = supporterId;

                // Agregar el ticket al contexto y guardarlo
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets2/Edit/5
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

        // POST: Tickets2/Edit/5
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

        // GET: Tickets2/Delete/5
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

        // POST: Tickets2/Delete/5
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
