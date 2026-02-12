using MedIQ_Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MedIQ_API.Data;

namespace DWS.Controllers
{
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public ChatController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        // Página de inicio (Bienvenida)
        public IActionResult Welcome()
        {
            return View();
        }

        // Vista del Chat - Ahora pública
        public IActionResult Chat(string category)
        {
            ViewBag.Category = category;
            ViewBag.Conversations = new List<ChatSession>();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetDynamicMenu()
        {
            var menu = await _context.CategoriasConocimiento
                .Include(c => c.Preguntas)
                    .ThenInclude(p => p.SubPreguntas)
                .Select(c => new
                {
                    id = c.Id.ToString(),
                    titulo = c.Nombre,
                    icono = c.Icono,
                    descripcion = c.Descripcion,
                    // Filtramos solo las preguntas principales (sin padre)
                    preguntas = c.Preguntas.Where(p => p.ParentId == null).Select(p => new
                    {
                        q = p.Pregunta,
                        a = p.Respuesta,
                        keywords = p.Keywords,
                        // Añadimos sus sub-preguntas (Nivel 3)
                        sub = p.SubPreguntas.Select(s => new
                        {
                            q = s.Pregunta,
                            a = s.Respuesta
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Json(menu);
        }

        // Acción para procesar el mensaje - Ahora pública
        [HttpPost]
        public async Task<IActionResult> SendMessage(string chatInput, IFormFile? image)
        {
            // 1. Detección de riesgo (Alertas Preventivas RF.APL.04)
            string? alerta = DetectarRiesgo(chatInput);

            // 2. Detección de solicitudes de diagnóstico médico
            if (alerta == null)
            {
                alerta = DetectarDiagnostico(chatInput);
            }

            // Procesa la imagen si existe
            string? imagenPathUrl = null;
            if (image != null && image.Length > 0)
            {
                try
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string fileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    imagenPathUrl = "/uploads/" + fileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error guardando imagen: " + ex.Message);
                }
            }

            // Lee la URL desde las variables de entorno o appsettings.json
            string? n8nUrlBase = _configuration["N8N_CHAT_URL"];

            if (string.IsNullOrEmpty(n8nUrlBase))
            {
                return Json(new { texto = "Error: La URL de n8n no está configurada.", esIA = true, alerta = alerta, imagenUrl = imagenPathUrl });
            }

            // Construir la URL con parámetros de consulta
            var usuarioNombre = User.Identity?.IsAuthenticated == true ? User.Identity.Name : "Usuario_Anonimo";
            var n8nUrl = $"{n8nUrlBase}?chatInput={Uri.EscapeDataString(chatInput ?? "")}&usuario={Uri.EscapeDataString(usuarioNombre ?? "")}";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5); // Aumentar tiempo de espera a 5 minutos
            using var content = new MultipartFormDataContent();

            // Añadir imagen si existe (solo la imagen va en el cuerpo multipart)
            if (image != null)
            {
                var fileStream = image.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
                content.Add(fileContent, "data", image.FileName); 
            }

            try
            {
                // Realizar el POST. Si hay contenido imagen, lo enviamos; si no, enviamos content vacío
                // pero n8n leerá los textos de la URL.
                var response = await client.PostAsync(n8nUrl, content);
                var respuestaDelAgente = await response.Content.ReadAsStringAsync();

                // Devolvemos la respuesta real de la IA y la URL de la imagen para el historial
                return Json(new { texto = respuestaDelAgente, esIA = true, alerta = alerta, imagenUrl = imagenPathUrl });
            }
            catch (Exception ex)
            {
                return Json(new { texto = "Error de comunicación con n8n: " + ex.Message, esIA = true, alerta = alerta, imagenUrl = imagenPathUrl });
            }
        }

        private string DetectarRiesgo(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje)) return null;

            mensaje = mensaje.ToLower();
            var palabrasClave = new[] { 
                "tomé demasiado", "tome demasiado", "sobredosis", 
                "mezclar", "sin receta", "duele mucho", "suicidio", "morir","abortar",
                "hacerme daño", "cortarme", "autolesión", "cutting", "no quiero vivir",
                "no aguanto más", "no puedo más", "quiero morir", "quiero dejar de vivir",
                "aborto casero", "métodos naturales para abortar", "abortar en casa",
                "abortar sin dolor", "abortar sin riesgos", "abortar sin complicaciones",
                "pastillas con alcohol", "combinar drogas", "mezcla letal","convulsiones", 
                "desmayo", "perder el conocimiento", "no respiro", "no puedo respirar",
                "dolor insoportable", "hemorragia", "sangrado abundante"
            };

            foreach (var palabra in palabrasClave)
            {
                if (mensaje.Contains(palabra))
                {
                    return "PRECAUCIÓN: Detectamos que tu consulta implica riesgos de salud graves o interacciones peligrosas. " +
                           "Recuerda que este sistema NO reemplaza a un médico. Si tienes síntomas graves, acude a urgencias inmediatamente.";
                }
            }

            return null;
        }

        private string DetectarDiagnostico(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje)) return null;

            mensaje = mensaje.ToLower();
            var palabrasDiagnostico = new[] {
                "tengo", "me duele", "siento", "padezco", "sufro de",
                "diagnostícame", "qué tengo", "que tengo", "estoy enfermo",
                "me diagnosticas", "crees que tengo", "será que tengo",
                "tengo síntomas de", "creo que tengo", "me salió"
            };

            foreach (var palabra in palabrasDiagnostico)
            {
                if (mensaje.Contains(palabra))
                {
                    return "Lo siento, no puedo realizar diagnósticos médicos. " +
                           "MedIQ es una herramienta educativa sobre medicamentos y conocimiento, " +
                           "pero NO puede diagnosticar enfermedades ni evaluar síntomas. " +
                           "Si tienes molestias o síntomas, por favor consulta con un médico profesional.";
                }
            }

            return null;
        }

        public IActionResult ProcesarImagen(string archivo)
        {
            ViewBag.NombreArchivo = archivo;
            return View(); // Debes crear la vista ProcesarImagen.cshtml
        }

        // ========== CONVERSATION HISTORY ENDPOINTS ==========

        [HttpPost]
        public async Task<IActionResult> CreateSession([FromBody] string titulo)
        {
            if (User.Identity?.IsAuthenticated != true) 
                return Json(new { sessionId = 0, titulo = "Chat Temporal", fecha = DateTime.UtcNow });

            var email = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var session = new ChatSession
            {
                Titulo = string.IsNullOrEmpty(titulo) ? "Nuevo Chat" : titulo,
                FechaCreacion = DateTime.UtcNow,
                UsuarioId = usuario.Id
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return Json(new { sessionId = session.Id, titulo = session.Titulo, fecha = session.FechaCreacion });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserSessions()
        {
            if (User.Identity?.IsAuthenticated != true) return Json(new List<object>());

            var email = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var sessions = await _context.ChatSessions
                .Where(s => s.UsuarioId == usuario.Id)
                .OrderByDescending(s => s.FechaCreacion)
                .Select(s => new
                {
                    id = s.Id,
                    titulo = s.Titulo,
                    fecha = s.FechaCreacion
                })
                .ToListAsync();

            return Json(sessions);
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionMessages(int sessionId)
        {
            if (User.Identity?.IsAuthenticated != true) return Json(new List<object>());

            var email = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == usuario.Id);
            if (session == null) return NotFound();

            var mensajes = await _context.Mensajes
                .Where(m => m.ChatSessionId == sessionId)
                .OrderBy(m => m.Fecha)
                .Select(m => new
                {
                    texto = m.Texto,
                    esIA = m.EsIA,
                    fecha = m.Fecha,
                    imagenUrl = m.ImagenUrl
                })
                .ToListAsync();

            return Json(mensajes);
        }

        [HttpPost]
        public async Task<IActionResult> SaveMessage([FromBody] SaveMessageRequest request)
        {
            if (User.Identity?.IsAuthenticated != true || request.SessionId == 0) return Ok();

            var email = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UsuarioId == usuario.Id);
            if (session == null) return NotFound();

            var mensaje = new Mensaje
            {
                Texto = request.Texto,
                EsIA = request.EsIA,
                Fecha = DateTime.UtcNow,
                ChatSessionId = request.SessionId,
                ImagenUrl = request.ImagenUrl
            };

            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();

            var email = User.Identity.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return Unauthorized();

            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UsuarioId == usuario.Id);
            
            if (session == null) return NotFound();

            // Delete all messages in this session
            var mensajes = await _context.Mensajes.Where(m => m.ChatSessionId == sessionId).ToListAsync();
            _context.Mensajes.RemoveRange(mensajes);
            
            // Delete the session
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class SaveMessageRequest
    {
        public int SessionId { get; set; }
        public string Texto { get; set; }
        public bool EsIA { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
