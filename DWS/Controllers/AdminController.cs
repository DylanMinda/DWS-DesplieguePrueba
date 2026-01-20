using MedIQ_API.Data;
using MedIQ_Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DWS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard con estadísticas
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                TotalConversaciones = await _context.ChatSessions.CountAsync(),
                MensajesHoy = await _context.Mensajes
                    .Where(m => m.Fecha.Date == DateTime.UtcNow.Date)
                    .CountAsync(),
                UsuariosAdmin = await _context.Usuarios
                    .Where(u => u.Rol == "Admin")
                    .CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        // Gestión de usuarios
        public async Task<IActionResult> Users()
        {
            var usuarios = await _context.Usuarios
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            return View(usuarios);
        }

        // Cambiar rol de usuario
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int userId, string newRole)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound();

            // Prevenir que el admin se quite su propio rol
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
            
            if (currentUser?.Id == userId && newRole != "Admin")
            {
                return BadRequest("No puedes quitarte tu propio rol de administrador");
            }

            usuario.Rol = newRole;
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        // Eliminar usuario
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var usuario = await _context.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound();

            // Prevenir que el admin se elimine a sí mismo
            var currentUserEmail = User.Identity.Name;
            var currentUser = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == currentUserEmail);
            
            if (currentUser?.Id == userId)
            {
                return BadRequest("No puedes eliminarte a ti mismo");
            }

            // Eliminar conversaciones y mensajes del usuario
            var sessions = await _context.ChatSessions.Where(s => s.UsuarioId == userId).ToListAsync();
            foreach (var session in sessions)
            {
                var mensajes = await _context.Mensajes.Where(m => m.ChatSessionId == session.Id).ToListAsync();
                _context.Mensajes.RemoveRange(mensajes);
            }
            _context.ChatSessions.RemoveRange(sessions);
            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        // Ver todas las conversaciones
        public async Task<IActionResult> Conversations()
        {
            var conversaciones = await (from session in _context.ChatSessions
                                       join usuario in _context.Usuarios on session.UsuarioId equals usuario.Id
                                       select new
                                       {
                                           session.Id,
                                           session.Titulo,
                                           session.FechaCreacion,
                                           UsuarioNombre = usuario.Nombre,
                                           UsuarioEmail = usuario.Email,
                                           CantidadMensajes = _context.Mensajes.Count(m => m.ChatSessionId == session.Id)
                                       })
                                       .OrderByDescending(s => s.FechaCreacion)
                                       .ToListAsync();

            return View(conversaciones);
        }
    }
}
