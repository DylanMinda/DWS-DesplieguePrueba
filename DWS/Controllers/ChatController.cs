using DWS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            // Por ahora solo simulamos una respuesta para que pruebes tu frontend
            var respuesta = new
            {
                texto = "Recibido. Como soy el frontend, aún no estoy conectado a n8n, pero tu mensaje fue: " + mensaje.Texto,
                esIA = true
            };

            return Json(respuesta);
        }
        [HttpPost]
        public async Task<IActionResult> EnviarAn8n(string mensaje, IFormFile imagen)
        {
            // URL que te da n8n al crear un "Webhook Node"
            string n8nUrl = "https://tu-instancia-n8n.com/webhook/asistente-medico";

            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();

            // Añadimos el texto
            content.Add(new StringContent(mensaje ?? ""), "chatInput");

            // Si hay imagen para OCR, la enviamos como archivo
            if (imagen != null)
            {
                var fileStream = imagen.OpenReadStream();
                content.Add(new StreamContent(fileStream), "data", imagen.FileName);
            }

            var response = await client.PostAsync(n8nUrl, content);
            var result = await response.Content.ReadAsStringAsync();

            return Json(new { respuesta = result }); // n8n devuelve la transcripción o respuesta
        }
    }
}
