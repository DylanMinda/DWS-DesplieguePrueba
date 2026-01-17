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
            // Reemplaza con tu URL de n8n (usar variables de entorno en Render es lo ideal)
            string n8nUrl = "http://localhost:5678/webhook-test/chat-mediq";

            using var client = new HttpClient();

            var payload = new
            {
                chatInput = mensaje.Texto,
                usuario = User.Identity.Name,
                usuarioId = User.FindFirst("UsuarioId")?.Value
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Petición a n8n y espera de la respuesta de Gemini/Pinecone
                var response = await client.PostAsync(n8nUrl, content);
                var respuestaDelAgente = await response.Content.ReadAsStringAsync();

                return Json(new { texto = respuestaDelAgente, esIA = true });
            }
            catch (Exception ex)
            {
                return Json(new { texto = "Error de conexión con el agente: " + ex.Message, esIA = true });
            }
        }

        public IActionResult ProcesarImagen(string archivo)
        {
            ViewBag.NombreArchivo = archivo;
            return View(); // Debes crear la vista ProcesarImagen.cshtml
        }
    }
}
