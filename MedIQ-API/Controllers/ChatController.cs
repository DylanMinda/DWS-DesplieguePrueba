using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MedIQ_API.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Threading.Tasks;

    namespace MedIQ_API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")] // Esto hará que la URL sea: api/Chat
        public class ChatController : ControllerBase
        {
            // El código que pusiste va aquí adentro:

            [HttpPost("analizar-salud")]
            public async Task<IActionResult> PostToN8n(IFormFile imagen, string pregunta)
            {
                // 1. La URL del Webhook de n8n (reemplaza con tu URL real de n8n)
                var n8nWebhookUrl = "https://tu-n8n-en-la-nube.com/webhook/identificador";

                using var client = new HttpClient();
                using var content = new MultipartFormDataContent();

                // Enviamos la imagen si existe
                if (imagen != null)
                {
                    var fileStream = new StreamContent(imagen.OpenReadStream());
                    content.Add(fileStream, "archivo_medico", imagen.FileName);
                }

                // Enviamos la pregunta
                content.Add(new StringContent(pregunta ?? ""), "pregunta_usuario");

                // 2. n8n recibe, procesa con IA y nos devuelve la respuesta
                var response = await client.PostAsync(n8nWebhookUrl, content);
                var jsonRespuesta = await response.Content.ReadAsStringAsync();

                return Ok(jsonRespuesta);
            }
        }
    }
}
