using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedIQ_Modelos;

namespace MedIQ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        // Aquí inyectarías tu DbContext de Postgres después

        public ChatController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarMensaje([FromForm] string texto, IFormFile imagen)
        {
            // 1. URL de tu Webhook de n8n
            var n8nUrl = "TU_URL_DE_N8N_AQUI";

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(texto ?? ""), "chatInput");

            if (imagen != null)
            {
                var streamContent = new StreamContent(imagen.OpenReadStream());
                content.Add(streamContent, "data", imagen.FileName);
            }

            // 2. Enviar a n8n
            var response = await _httpClient.PostAsync(n8nUrl, content);
            var resultado = await response.Content.ReadAsStringAsync();

            // 3. (Opcional) Guardar en Postgres vía DbContext

            return Ok(new { respuesta = resultado });
        }
    }
}
