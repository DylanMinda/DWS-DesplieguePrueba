using MedIQ_Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MedIQ_API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace DWS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly DWS.Services.IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AccountController(AppDbContext context, DWS.Services.IEmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _emailService = emailService;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad básica
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // Verificación de nulos para evitar el error azul (Exception)
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Por favor ingrese correo y contraseña.");
                return View();
            }

            // 1. Verificar bloqueo en Memoria por Email
            string cacheKey = "LoginAttempts_" + Email;
            if (_cache.TryGetValue(cacheKey, out int attempts) && attempts >= 3)
            {
                ModelState.AddModelError("", "Has superado el límite de 3 intentos fallidos. Por seguridad, la cuenta está bloqueada por 15 minutos.");
                return View();
            }

            // Buscar usuario por email
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (usuario != null)
            {
                bool passwordValida = false;

                // Intentar verificar con BCrypt primero (nuevo método)
                try
                {
                    passwordValida = BCrypt.Net.BCrypt.Verify(Password, usuario.Contraseña);
                }
                catch (BCrypt.Net.SaltParseException)
                {
                    // Si falla, es porque la contraseña está en SHA256 (método antiguo)
                    string passwordHashSHA256 = HashPassword(Password);
                    passwordValida = usuario.Contraseña == passwordHashSHA256;

                    if (passwordValida)
                    {
                        usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(Password);
                        await _context.SaveChangesAsync();
                    }
                }

                if (passwordValida)
                {
                    // Éxito: Limpiar intentos fallidos
                    _cache.Remove(cacheKey);

                    if (usuario.Rol != "Admin")
                    {
                        ModelState.AddModelError("", "Acceso denegado. Este portal es exclusivo para administradores.");
                        return View();
                    }

                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, usuario.Email),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim("NombreCompleto", usuario.Nombre),
                        new Claim("UsuarioId", usuario.Id.ToString()),
                        new Claim(ClaimTypes.Role, usuario.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    // Incrementar intentos fallidos en Memoria
                    _cache.TryGetValue(cacheKey, out attempts);
                    attempts++;
                    
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
                    
                    _cache.Set(cacheKey, attempts, cacheOptions);

                    if (attempts >= 3)
                    {
                        ModelState.AddModelError("", "Has superado el límite de 3 intentos fallidos. Tu cuenta ha sido bloqueada por 15 minutos.");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"El email o la contraseña son incorrectos. Intentos restantes: {3 - attempts}");
                    }
                    return View();
                }
            }

            ModelState.AddModelError("", "El email o la contraseña son incorrectos.");
            return View();
        }

        // Método helper para encriptar contraseñas (SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == Email);
            if (usuario == null)
            {
                // Por seguridad, no decimos si el email existe o no
                ViewBag.Message = "Si el correo está registrado, recibirás una contraseña temporal.";
                return View();
            }

            // Generar contraseña temporal aleatoria
            string tempPassword = Guid.NewGuid().ToString().Substring(0, 8) + "1A!";
            usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(tempPassword);
            await _context.SaveChangesAsync();

            // Enviar correo
            string mensaje = $@"
                <h3>Recuperación de Contraseña - MedIQ</h3>
                <p>Hola {usuario.Nombre},</p>
                <p>Has solicitado restablecer tu contraseña. Tu nueva contraseña temporal es:</p>
                <h2 style='color:#1976D2;'>{tempPassword}</h2>
                <p>Por favor, inicia sesión con esta contraseña y cámbiala lo antes posible en tu perfil.</p>
                <br>
                <p>Si no solicitaste este cambio, ignora este mensaje.</p>";

            try {
                await _emailService.SendEmailAsync(usuario.Email, "Tu nueva contraseña temporal - MedIQ", mensaje);
                ViewBag.Message = "Se ha enviado una contraseña temporal a tu correo.";
                ViewBag.IsSuccess = true;
            } catch (Exception ex) {
                ModelState.AddModelError("", "Error al enviar el correo: " + ex.Message);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}