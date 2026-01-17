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
        [Route("api/[controller]")]
        public class ChatController : ControllerBase
        {
            private readonly IConfiguration _configuration;

            // Constructor para inyectar la configuración y leer N8N_CHAT_URL
            public ChatController(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            [HttpPost("analizar-salud")]
            public async Task<IActionResult> PostToN8n(IFormFile imagen, string pregunta)
            {
                // Usamos la variable que ya configuraste en Render
                string n8nWebhookUrl = _configuration["N8N_CHAT_URL"];

                if (string.IsNullOrEmpty(n8nWebhookUrl))
                {
                    return BadRequest("La URL de n8n no está configurada.");
                }

                using var client = new HttpClient();
                using var content = new MultipartFormDataContent();

                // Enviamos la imagen para el futuro procesamiento de OCR
                if (imagen != null)
                {
                    var fileStream = new StreamContent(imagen.OpenReadStream());
                    content.Add(fileStream, "archivo_medico", imagen.FileName);
                }

                // Importante: Cambia "pregunta_usuario" por "chatInput" 
                // para que coincida con tu flujo de n8n actual
                content.Add(new StringContent(pregunta ?? ""), "chatInput");

                try
                {
                    var response = await client.PostAsync(n8nWebhookUrl, content);
                    var respuestaDelAgente = await response.Content.ReadAsStringAsync();
                    return Ok(new { texto = respuestaDelAgente });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error de conexión: {ex.Message}");
                }
            }
        }
    }
}
