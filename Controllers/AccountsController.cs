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
    public class AccountsController : Controller
    {
        private readonly TicketDbContext _context;

        public AccountsController(TicketDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            var accounts = await _context.Account.ToListAsync();

            // Cargar el nombre de estado para cada cuenta
            foreach (var account in accounts)
            {
                if (account.Status != null)
                {
                    var statusId = int.Parse(account.Status);
                    account.Status = await _context.Status
                        .Where(s => s.Id == statusId)
                        .Select(s => s.name)
                        .FirstOrDefaultAsync();
                }
            }

            return View(accounts);
        }


        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);

            if (account == null)
            {
                return NotFound();
            }

            // Cargar el status
            if (account.Status != null)
            {
                var statusId = int.Parse(account.Status);
                account.Status = await _context.Status
                    .Where(s => s.Id == statusId)
                    .Select(s => s.name)
                    .FirstOrDefaultAsync();
            }

            // Cargar el rol
            if (account.RoleId != null)
            {
                var roleId = account.RoleId.Value;
                var role = await _context.Role
                    .Where(r => r.Id == roleId)
                    .Select(r => r.Name)
                    .FirstOrDefaultAsync();

                ViewBag.RoleName = role; // Pasar el nombre del rol a través de ViewBag
            }

            return View(account);
        }






        // GET: Accounts/Create
        public IActionResult Create()
        {

            try
            {
                var listaRoles = _context.Role.ToList();
                ViewData["listRoles"] = new SelectList(listaRoles, "Id", "Name");

                var listaStatus = (from s in _context.Status
                                   where s.StatusText == "User"
                                   select s).ToList();
                ViewData["liststatus"] = new SelectList(listaStatus, "Id", "name");

            }
            catch (Exception ex)
            {
                
                ViewBag.ErrorMessage = "Error al obtener los datos: " + ex.Message;
            }

            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Password,FullName,Status,Email,RoleId,FotoUrl,AssociateE")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            try
            {
                var listaRoles = _context.Role.ToList();
                ViewData["listRoles"] = new SelectList(listaRoles, "Id", "Name", account.RoleId);

                var listaStatus = (from s in _context.Status
                                   where s.StatusText == "User"
                                   select new SelectListItem { Value = s.Id.ToString(), Text = s.name }).ToList();
                ViewData["listStatus"] = new SelectList(listaStatus, "Value", "Text", account.Status);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Error al obtener los datos: " + ex.Message;
            }

            return View(account);
        }



        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Password,FullName,Status,Email,RoleId,FotoUrl,AssociateE")] Account account)
        {


            if (id != account.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.Id))
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
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Account.Any(e => e.Id == id);
        }
    }
}
