using MedIQ_Modelos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        [ValidateAntiForgeryToken] // Seguridad básica
        public async Task<IActionResult> Login(string Email, string Password)
        {
            // Verificación de nulos para evitar el error azul (Exception)
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "Por favor ingrese correo y contraseña.");
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
                    // Verificar con SHA256 como fallback
                    string passwordHashSHA256 = HashPassword(Password);
                    passwordValida = usuario.Contraseña == passwordHashSHA256;

                    // Si la contraseña es correcta, actualizarla a BCrypt
                    if (passwordValida)
                    {
                        usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(Password);
                        await _context.SaveChangesAsync();
                    }
                }

                if (passwordValida)
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Name, usuario.Email),  // ← Email como Name para User.Identity.Name
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim("NombreCompleto", usuario.Nombre),  // ← Nombre en claim separado
                        new Claim("UsuarioId", usuario.Id.ToString()),
                        new Claim(ClaimTypes.Role, usuario.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));

                    return RedirectToAction("Welcome", "Chat");
                }
            }

            ModelState.AddModelError("", "Credenciales inválidas. Inténtalo de nuevo.");
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        // CAMBIO IMPORTANTE: El nombre del método debe coincidir con la acción de la vista o usar [ActionName]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Validar que el correo no exista ya
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                    {
                        ModelState.AddModelError("Email", "Este correo ya está registrado.");
                        return View(usuario);
                    }

                    // 2. Validar complejidad de contraseña (Simple)
                    if (!EsContraseñaSegura(usuario.Contraseña))
                    {
                        ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 8 caracteres, una mayúscula y un número.");
                        return View(usuario);
                    }

                    // Encriptamos la contraseña antes de guardarla usando BCrypt
                    usuario.Contraseña = BCrypt.Net.BCrypt.HashPassword(usuario.Contraseña);

                    _context.Usuarios.Add(usuario); // Especificamos la tabla Usuarios
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    // Esto capturará errores de conexión con Render o de la base de datos
                    ModelState.AddModelError("", "Error al conectar con la base de datos: " + ex.Message);
                }
            }

            return View(usuario);
        }

        // Método simple para validar seguridad
        private bool EsContraseñaSegura(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;
            if (password.Length < 8) return false; // Mínimo 8 caracteres
            if (!password.Any(char.IsUpper)) return false; // Al menos una mayúscula
            if (!password.Any(char.IsDigit)) return false; // Al menos un número
            return true;
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

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}