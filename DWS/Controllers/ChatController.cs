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
            // 1. Detección de riesgo (Alertas Preventivas RF.APL.04)
            string alerta = DetectarRiesgo(mensaje.Texto);

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

                // Devolvemos tanto la respuesta de la IA como la posible alerta
                return Json(new { texto = respuestaDelAgente, esIA = true, alerta = alerta });
            }
            catch (Exception ex)
            {
                // FALLBACK DE SEGURIDAD:
                // Incluso si la IA falla, debemos mostrar la alerta si existe.
                return Json(new { texto = "Error de comunicación con el Agente: " + ex.Message, esIA = true, alerta = alerta });
            }
        }

        private string DetectarRiesgo(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje)) return null;

            mensaje = mensaje.ToLower();
            var palabrasClave = new[] { 
                "tomé demasiado", "tome demasiado", "sobredosis", 
                "mezclar", "sin receta", "duele mucho", "suicidio", "morir" 
            };

            foreach (var palabra in palabrasClave)
            {
                if (mensaje.Contains(palabra))
                {
                    return "⚠️ PRECAUCIÓN: Detectamos que tu consulta implica riesgos de salud graves o interacciones peligrosas. " +
                           "Recuerda que este sistema NO reemplaza a un médico. Si tienes síntomas graves, acude a urgencias inmediatamente.";
                }
            }

            return null;
        }

        public IActionResult ProcesarImagen(string archivo)
        {
            ViewBag.NombreArchivo = archivo;
            return View(); // Debes crear la vista ProcesarImagen.cshtml
        }
    }
}
