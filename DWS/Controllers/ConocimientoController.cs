using MedIQ_Modelos;
using Microsoft.AspNetCore.Authorization;
using MedIQ_API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace DWS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ConocimientoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ConocimientoController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
            
            // Asegurar que las tablas existan (útil si las migraciones fallan en el entorno local)
            _context.Database.EnsureCreated();
        }

        // --- ACCIÓN PARA MIGRAR DATOS DE CHAT.JS ---
        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            if (await _context.CategoriasConocimiento.AnyAsync())
            {
                return Content("La base de datos ya tiene información. No se realizó el seed.");
            }

            var cat1 = new ConocimientoCategoria { Nombre = "Uso de Medicamentos", Icono = "fas fa-pills", Descripcion = "Educación sobre el uso responsable y riesgos de la automedicación." };
            var cat2 = new ConocimientoCategoria { Nombre = "Antibióticos y Bacterias", Icono = "fas fa-shield-virus", Descripcion = "Información educativa sobre el peligro de la resistencia bacteriana." };
            var cat3 = new ConocimientoCategoria { Nombre = "Mitos sobre la Salud", Icono = "fas fa-stethoscope", Descripcion = "Desmintiendo creencias comunes para fomentar el cuidado profesional." };

            _context.CategoriasConocimiento.AddRange(cat1, cat2, cat3);
            await _context.SaveChangesAsync();

            // --- CATEGORÍA 1: USO DE MEDICAMENTOS ---
            var q1_1 = new ConocimientoQA { CategoriaId = cat1.Id, Pregunta = "¿En qué consiste la automedicación?", Respuesta = "Es el uso de fármacos sin supervisión médica. Puede ocultar enfermedades graves o causar intoxicaciones. Siempre consulta a un profesional.", Keywords = "automedicacion, riesgo, salud" };
            _context.PreguntasConocimiento.Add(q1_1);
            await _context.SaveChangesAsync();

            _context.PreguntasConocimiento.AddRange(
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_1.Id, Pregunta = "¿Cuáles son los riesgos de automedicarse?", Respuesta = "Entre los riesgos están las reacciones alérgicas no detectadas, la toxicidad renal o hepática, y el retraso en un diagnóstico correcto.", Keywords = "riesgo, peligro" },
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_1.Id, Pregunta = "¿Cómo evitar la automedicación?", Respuesta = "La mejor forma es no aceptar recomendaciones de personas no profesionales y acudir al médico ante cualquier síntoma persistente.", Keywords = "prevencion, consulta" },
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_1.Id, Pregunta = "¿Qué son los medicamentos de venta libre?", Respuesta = "Son fármacos que no requieren receta (OTC), pero 'libre' no significa 'sin riesgo'. Deben usarse solo para síntomas leves y breves bajo orientación farmacéutica.", Keywords = "otc, venta libre" }
            );

            var q1_2 = new ConocimientoQA { CategoriaId = cat1.Id, Pregunta = "Importancia de la Adherencia", Respuesta = "Seguir exactamente las indicaciones de tiempo y forma del médico es vital para que el tratamiento sea efectivo y seguro.", Keywords = "adherencia, tratamiento, horario" };
            _context.PreguntasConocimiento.Add(q1_2);
            await _context.SaveChangesAsync();

            _context.PreguntasConocimiento.AddRange(
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_2.Id, Pregunta = "¿Qué hacer si se olvida una dosis?", Respuesta = "La regla general es tomarla en cuanto se recuerde, pero si falta poco para la siguiente, es mejor esperar para evitar una sobredosis. Consulta el prospecto.", Keywords = "olvido, dosis" },
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_2.Id, Pregunta = "¿Cómo se calculan las dosis?", Respuesta = "Las dosis las determina estrictamente el médico basándose en peso, edad, función renal y la condición del paciente. Nunca calcules dosis por tu cuenta.", Keywords = "calculo, peso" },
                new ConocimientoQA { CategoriaId = cat1.Id, ParentId = q1_2.Id, Pregunta = "¿Por qué terminar el tratamiento?", Respuesta = "Terminar el ciclo asegura que la enfermedad se elimine por completo y no regrese con más fuerza o resistencia.", Keywords = "terminar, ciclo" }
            );

            // --- CATEGORÍA 2: ANTIBIÓTICOS ---
            var q2_1 = new ConocimientoQA { CategoriaId = cat2.Id, Pregunta = "¿Qué es la resistencia a antibióticos?", Respuesta = "Es un problema de salud global donde las bacterias se vuelven 'súper bacterias' que ya no mueren con los medicamentos tradicionales.", Keywords = "resistencia, bacteria" };
            _context.PreguntasConocimiento.Add(q2_1);
            await _context.SaveChangesAsync();

            _context.PreguntasConocimiento.AddRange(
                new ConocimientoQA { CategoriaId = cat2.Id, ParentId = q2_1.Id, Pregunta = "¿Por qué el mal uso crea resistencia?", Respuesta = "Al tomar dosis incompletas o antibióticos para virus (como gripe), las bacterias más fuertes sobreviven y se multiplican.", Keywords = "causa, mal uso" },
                new ConocimientoQA { CategoriaId = cat2.Id, ParentId = q2_1.Id, Pregunta = "¿Por qué la resistencia es peligrosa?", Respuesta = "Porque cirugías sencillas o infecciones comunes podrían volverse mortales si no hay antibióticos que funcionen.", Keywords = "peligro, mortalidad" },
                new ConocimientoQA { CategoriaId = cat2.Id, ParentId = q2_1.Id, Pregunta = "¿Los antibióticos curan la gripe?", Respuesta = "No. La gripe es causada por virus, y los antibióticos solo atacan bacterias. Usarlos para la gripe solo daña tu flora intestinal y crea resistencia.", Keywords = "gripe, virus" }
            );

            // --- CATEGORÍA 3: MITOS ---
            var q3_1 = new ConocimientoQA { CategoriaId = cat3.Id, Pregunta = "Mitos comunes sobre curación", Respuesta = "Muchas creencias populares pueden ser peligrosas. Aquí desmitificamos algunas para proteger tu salud.", Keywords = "mito, creencia" };
            _context.PreguntasConocimiento.Add(q3_1);
            await _context.SaveChangesAsync();

            _context.PreguntasConocimiento.AddRange(
                new ConocimientoQA { CategoriaId = cat3.Id, ParentId = q3_1.Id, Pregunta = "¿Lo natural es siempre seguro?", Respuesta = "Es un mito. Muchas plantas medicinales interfieren peligrosamente con medicamentos recetados o pueden ser tóxicas en dosis altas para el hígado.", Keywords = "natural, hierbas" },
                new ConocimientoQA { CategoriaId = cat3.Id, ParentId = q3_1.Id, Pregunta = "¿Inyectable cura más rápido?", Respuesta = "No siempre. El médico elige la vía (oral o inyectable) según la urgencia y el tipo de fármaco, no necesariamente para 'acelerar' la cura.", Keywords = "inyeccion, rapidez" },
                new ConocimientoQA { CategoriaId = cat3.Id, ParentId = q3_1.Id, Pregunta = "¿Se puede dejar el remedio si me siento bien?", Respuesta = "Es un error común. Sentirse bien no significa que la infección terminó; dejarlo antes de tiempo es la causa principal de las recaídas.", Keywords = "abandono, mejora" }
            );

            await _context.SaveChangesAsync();

            return Content("¡Base de datos con propósitos educativos inicializada con éxito!");
        }

        // --- ACCIÓN PARA RESET TOTAL (USAR CON PRECAUCIÓN) ---
        [HttpGet]
        public async Task<IActionResult> Reset()
        {
            try
            {
                // ELIMINAR Y RECREAR TODO (El enfoque "Nuclear" que pidió el usuario)
                await _context.Database.EnsureDeletedAsync();
                await _context.Database.EnsureCreatedAsync();

                // Poblar con datos iniciales
                await Seed();

                return Content("✅ ¡RESETEO EXITOSO! La base de datos ha sido borrada y recreada con el nuevo esquema. Ve al panel de administración.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error durante el reseteo: {ex.Message} \n\nDetalle: {ex.InnerException?.Message}");
            }
        }

        // --- GESTIÓN DE CATEGORÍAS ---

        public async Task<IActionResult> Categorias()
        {
            var categorias = await _context.CategoriasConocimiento
                .Include(c => c.Preguntas)
                .ToListAsync();
            return View(categorias);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoria(string Nombre, string Icono, string Descripcion)
        {
            if (string.IsNullOrEmpty(Nombre))
            {
                TempData["Error"] = "El nombre de la categoría es obligatorio.";
                return RedirectToAction(nameof(Categorias));
            }

            var categoria = new ConocimientoCategoria
            {
                Nombre = Nombre,
                Icono = Icono ?? "",
                Descripcion = Descripcion ?? ""
            };

            _context.CategoriasConocimiento.Add(categoria);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categorias));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.CategoriasConocimiento.FindAsync(id);
            if (categoria != null)
            {
                // Eliminar preguntas asociadas primero
                var preguntas = _context.PreguntasConocimiento.Where(p => p.CategoriaId == id);
                _context.PreguntasConocimiento.RemoveRange(preguntas);
                
                _context.CategoriasConocimiento.Remove(categoria);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categorias));
        }

        // --- GESTIÓN DE PREGUNTAS (QA) ---

        public async Task<IActionResult> Preguntas(int? categoriaId)
        {
            var query = _context.PreguntasConocimiento.Include(p => p.Categoria).AsQueryable();
            
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId);
                ViewBag.SelectedCategoriaId = categoriaId;
            }

            var preguntas = await query.OrderByDescending(p => p.FechaCreacion).ToListAsync();
            ViewBag.Categorias = await _context.CategoriasConocimiento.ToListAsync();
            
            return View(preguntas);
        }

        [HttpPost]
        public async Task<IActionResult> UpsertPregunta(int Id, string Pregunta, string Respuesta, string Keywords, int CategoriaId, int? ParentId)
        {
            if (string.IsNullOrEmpty(Pregunta) || string.IsNullOrEmpty(Respuesta))
            {
                TempData["Error"] = "La pregunta y la respuesta son obligatorias.";
                return RedirectToAction(nameof(Preguntas), new { categoriaId = CategoriaId });
            }

            if (Id == 0)
            {
                var nueva = new ConocimientoQA
                {
                    Pregunta = Pregunta,
                    Respuesta = Respuesta,
                    Keywords = Keywords ?? "",
                    CategoriaId = CategoriaId,
                    ParentId = ParentId,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.PreguntasConocimiento.Add(nueva);
            }
            else
            {
                var original = await _context.PreguntasConocimiento.FindAsync(Id);
                if (original != null)
                {
                    original.Pregunta = Pregunta;
                    original.Respuesta = Respuesta;
                    original.Keywords = Keywords ?? "";
                    original.CategoriaId = CategoriaId;
                    original.ParentId = ParentId;
                    original.FechaActualizacion = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Preguntas), new { categoriaId = CategoriaId });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePregunta(int id)
        {
            var pregunta = await _context.PreguntasConocimiento.FindAsync(id);
            if (pregunta != null)
            {
                int catId = pregunta.CategoriaId;
                _context.PreguntasConocimiento.Remove(pregunta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Preguntas), new { categoriaId = catId });
            }
            return RedirectToAction(nameof(Preguntas));
        }

        // --- SINCRONIZACIÓN CON PINECONE (via n8n) ---

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            try
            {
                // Obtener todo el conocimiento para sincronizar
                var conocimiento = await _context.PreguntasConocimiento
                    .Include(p => p.Categoria)
                    .ToListAsync();

                var dataToSync = conocimiento.Select(p => new
                {
                    // Creamos un string único para que Pinecone lo indexe mejor
                    Text = $"Pregunta: {p.Pregunta}\nRespuesta: {p.Respuesta}\nCategoría: {(p.Categoria?.Nombre ?? "General")}\nKeywords: {p.Keywords}",
                    Metadata = new
                    {
                        p.Pregunta,
                        p.Respuesta,
                        Categoria = p.Categoria?.Nombre ?? "General",
                        p.Keywords,
                        p.Id
                    }
                }).ToList();

                var n8nSyncUrl = _configuration["N8N_KB_SYNC_URL"];
                
                if (string.IsNullOrEmpty(n8nSyncUrl))
                {
                    return BadRequest("URL de sincronización n8n no configurada.");
                }

                var content = new StringContent(
                    JsonSerializer.Serialize(new { action = "reindex", data = dataToSync }),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(n8nSyncUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Sincronización iniciada: {responseContent}";
                }
                else
                {
                    TempData["Error"] = $"Error n8n ({response.StatusCode}): {responseContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error de conexión: {ex.Message}";
            }

            return RedirectToAction(nameof(Categorias));
        }
    }
}
