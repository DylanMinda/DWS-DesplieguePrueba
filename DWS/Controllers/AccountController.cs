using DWS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DWS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // Buscar usuario en la base de datos de la nube
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == Email && u.Contraseña == Password);

            if (usuario != null)
            {
                var claims = new List<Claim> {
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim("UsuarioId", usuario.Id.ToString())
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Welcome", "Chat");
            }

            ModelState.AddModelError("", "Credenciales inválidas. Inténtalo de nuevo.");
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el correo ya existe
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
                if (existe)
                {
                    ModelState.AddModelError("Email", "Este correo ya está registrado.");
                    return View(usuario);
                }

                // Guardar en la nube
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Welcome", "Chat");
        }
    }
}
