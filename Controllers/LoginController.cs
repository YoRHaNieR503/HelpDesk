using HelpDesk.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HelpDesk.Controllers
{
    public class LoginController : Controller
    {
        private readonly TicketDbContext _context;

        public LoginController(TicketDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string emailOrUsername, string password)
        {
            var account = _context.Account.FirstOrDefault(a =>
                (a.Email == emailOrUsername || a.Username == emailOrUsername) && a.Password == password);

            if (account != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account.Username ?? account.Email),
            new Claim(ClaimTypes.Role, account.RoleId == 1 ? "Admin" : account.RoleId == 2 ? "Support" : "User"),
            new Claim("FullName", account.FullName ?? ""),
            new Claim("FotoUrl", account.FotoUrl ?? "")
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                if (account.RoleId == 1)
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (account.RoleId == 2)
                {
                    return RedirectToAction("Index", "Tickets2"); // Redirige a la página de inicio de sesión de soporte
                }
                else
                {
                    // Redirige a una página diferente si no es admin ni soporte
                    return RedirectToAction("Index", "Tickets1"); // Ejemplo de redirección para usuarios no admin ni soporte
                }
            }

            ViewBag.ErrorMessage = "Usuario o contraseña incorrectos.";

            // Si la autenticación falla, se mantiene en la vista actual
            return View("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
