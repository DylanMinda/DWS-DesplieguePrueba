using MedIQ_Modelos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace DWS.Controllers
{
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;

        public ChatController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Página de inicio (Bienvenida)
        public IActionResult Welcome()
        {
            return View();
        }

        // Vista del Chat - Solo accesible si está logueado
        [Authorize]
        public IActionResult Chat(string category)
        {
            // Pasamos la categoría a la vista para que el asistente sepa el contexto
            ViewBag.Category = category;

            // Aquí el de backend luego cargará el historial real
            ViewBag.Conversations = new List<ChatSession>();

            return View();
        }

        // Acción para procesar el mensaje (aquí se conectará con n8n)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage([FromBody] Mensaje mensaje)
        {
            // Lee la URL desde las variables de entorno de Render
            string n8nUrl = _configuration["N8N_CHAT_URL"];

            using var client = new HttpClient();
            var payload = new
            {
                chatInput = mensaje.Texto,
                usuario = User.Identity.Name
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(n8nUrl, content);
                var respuestaDelAgente = await response.Content.ReadAsStringAsync();

                return Json(new { texto = respuestaDelAgente, esIA = true });
            }
            catch (Exception ex)
            {
                return Json(new { texto = "Error de comunicación: " + ex.Message, esIA = true });
            }
        }

        public IActionResult ProcesarImagen(string archivo)
        {
            ViewBag.NombreArchivo = archivo;
            return View(); // Debes crear la vista ProcesarImagen.cshtml
        }
    }
}
