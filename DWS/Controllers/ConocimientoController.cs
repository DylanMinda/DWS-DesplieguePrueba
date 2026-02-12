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

        // --- ACCIÓN PARA RESTAURAR DATOS ORIGINALES (48 PREGUNTAS) ---
        [HttpGet]
        public async Task<IActionResult> Seed()
        {
            // Limpiar datos previos para evitar duplicados si el usuario desea restaurar
            var existingQAs = await _context.PreguntasConocimiento.ToListAsync();
            var existingCats = await _context.CategoriasConocimiento.ToListAsync();
            _context.PreguntasConocimiento.RemoveRange(existingQAs);
            _context.CategoriasConocimiento.RemoveRange(existingCats);
            await _context.SaveChangesAsync();

            // 1. CREAR CATEGORÍAS
            var catMed = new ConocimientoCategoria { Nombre = "Uso de Medicamentos", Icono = "fas fa-pills", Descripcion = "Guía de Medicación y Uso Responsable." };
            var catRes = new ConocimientoCategoria { Nombre = "Antibióticos y Bacterias", Icono = "fas fa-shield-virus", Descripcion = "Peligros de la Resistencia a los Antibióticos." };
            var catMit = new ConocimientoCategoria { Nombre = "Mitos sobre la Salud", Icono = "fas fa-balance-scale", Descripcion = "Mitos, Realidades y Precauciones." };

            _context.CategoriasConocimiento.AddRange(catMed, catRes, catMit);
            await _context.SaveChangesAsync();

            // --- FUNCIÓN HELPER PARA EVITAR REPETICIÓN ---
            async Task AddQA(int catId, string q, string a, string keywords, List<(string q, string a)> subs)
            {
                var main = new ConocimientoQA { CategoriaId = catId, Pregunta = q, Respuesta = a, Keywords = keywords, FechaCreacion = DateTime.UtcNow };
                _context.PreguntasConocimiento.Add(main);
                await _context.SaveChangesAsync();

                foreach (var s in subs)
                {
                    _context.PreguntasConocimiento.Add(new ConocimientoQA 
                    { 
                        CategoriaId = catId, 
                        ParentId = main.Id, 
                        Pregunta = s.q, 
                        Respuesta = s.a, 
                        Keywords = keywords, 
                        FechaCreacion = DateTime.UtcNow 
                    });
                }
                await _context.SaveChangesAsync();
            }

            // --- CATEGORÍA: MEDICACIÓN ---
            await AddQA(catMed.Id, "¿En qué consiste exactamente la automedicación?", "La automedicación es el uso de medicamentos por iniciativa propia sin receta médica. Puede ser peligroso si no se hace bajo supervisión profesional, ya que puede enmascarar enfermedades reales.", "medicacion, automedicacion", new List<(string, string)> {
                ("¿Qué diferencia hay entre automedicación y autocuidado?", "El autocuidado es elegir hábitos saludables (dieta, ejercicio), mientras que la automedicación es usar fármacos sin receta para tratar síntomas. El autocuidado previene, la automedicación mal hecha pone en riesgo."),
                ("¿Cuáles son los riesgos de ocultar síntomas graves?", "Automedicarse para un dolor de estómago fuerte podría 'tapar' una apendicitis. Al no sentir el dolor, no buscas ayuda profesional y una condición tratable puede volverse mortal."),
                ("¿Cómo afecta la automedicación a la seguridad del paciente?", "Aumenta la probabilidad de interacciones peligrosas, errores en la dosis y desarrollo de alergias no detectadas, según la OMS es una de las mayores amenazas para la salud pública.")
            });

            await AddQA(catMed.Id, "¿Qué elementos debo revisar al leer una receta o etiqueta médica?", "Leer la receta es clave para entender la dosis exacta, la frecuencia y la duración total del tratamiento, evitando errores que comprometan tu recuperación.", "receta, etiqueta", new List<(string, string)> {
                ("¿Dónde encuentro la fecha de vencimiento y por qué importa?", "Suele estar en el borde del blister o la caja. Tomar medicina vencida es peligroso porque los componentes químicos se degradan y pueden volverse tóxicos o perder su efecto."),
                ("¿Qué significa 'Vía de Administración' (Oral, Tópica, etc.)?", "Indica cómo debe entrar el fármaco al cuerpo. Si pones gotas para el oído en el ojo, o tragas una pastilla que era sublingual, el medicamento no funcionará o causará daño."),
                ("¿Cómo identifico excipientes que podrían darme alergia?", "En el prospecto (papel interno), busca la lista de excipientes. Sustancias como lactosa o gluten pueden causar reacciones graves en personas sensibles.")
            });

            await AddQA(catMed.Id, "¿Por qué es fundamental respetar los horarios indicados?", "Respetar los horarios garantiza que el medicamento mantenga niveles estables en tu sangre durante todo el día, asegurando que el tratamiento realmente funcione.", "horarios, docis", new List<(string, string)> {
                ("¿Es lo mismo '3 veces al día' que 'cada 8 horas'?", "No. '3 veces' puede ser aleatorio (desayuno, almuerzo, cena). 'Cada 8 horas' es estricto para mantener el nivel de fármaco estable en sangre durante las 24 horas del día."),
                ("¿Qué es la 'Ventana Terapéutica' de un medicamento?", "Es el rango exacto de dosis donde el fármaco cura. Si bajas de ahí no sirve; si subes de ahí se vuelve veneno para tus órganos (riñón o hígado)."),
                ("¿Cómo influyen los alimentos en la absorción del fármaco?", "Algunos fármacos necesitan grasa para absorberse, otros se bloquean con el calcio de la leche. Seguir la instrucción 'con alimentos' o 'en ayunas' determina si la medicina entra a tu sangre.")
            });

            await AddQA(catMed.Id, "¿Qué debo hacer ante el olvido de una dosis?", "Ante un olvido, lo más importante es no entrar en pánico. Debes evaluar cuánto tiempo ha pasado, pero recuerda: **nunca tomes doble dosis**.", "olvido, dosis", new List<(string, string)> {
                ("¿Existe alguna 'regla de tiempo' para tomarla tarde?", "Generalmente, si te acuerdas antes de la mitad del tiempo para la siguiente dosis, tómala. Si falta poco para la siguiente, es mejor esperar y seguir con el horario normal."),
                ("¿Por qué NUNCA debo duplicar la dosis para compensar?", "Duplicar la dosis NO arregla el olvido, solo sobrecarga tus riñones e hígado con una cantidad tóxica que tu cuerpo no puede procesar de golpe."),
                ("¿Qué riesgos hay en tratamientos críticos como anticonceptivos?", "En tratamientos donde la hormona es constante, un olvido de más de 12 horas puede anular la eficacia totalmente. En estos casos, se debe usar un método de barrera (preservativo) adicional.")
            });

            // --- CATEGORÍA: RESISTENCIA ---
            await AddQA(catRes.Id, "¿Qué es la resistencia bacteriana a los antibióticos?", "La resistencia bacteriana ocurre cuando las bacterias aprenden a sobrevivir a los antibióticos. Esto hace que infecciones comunes vuelvan a ser peligrosas y difíciles de tratar.", "resistencia, bacteria", new List<(string, string)> {
                ("¿Cómo hacen las bacterias para volverse 'superbacterias'?", "Las bacterias mutan y desarrollan 'escudos' o bombas para expulsar el antibiótico. Al reproducirse, pasan este 'superpoder' a otras bacterias, creando una familia resistente."),
                ("¿Cuál es la diferencia entre resistencia natural y adquirida?", "La natural es propia de la bacteria. La adquirida ocurre por culpa nuestra: al usar mal los antibióticos obligamos a la bacteria a aprender cómo sobrevivir."),
                ("¿Por qué la OMS considera esto una amenaza para la humanidad?", "Si los antibióticos dejan de funcionar, cirugías simples o partos volverán a ser mortales por infecciones que hoy consideramos fáciles de curar.")
            });

            await AddQA(catRes.Id, "¿Los antibióticos sirven para tratar la gripe o el resfriado común?", "Los antibióticos NO sirven para combatir virus como la gripe. Usarlos sin necesidad solo daña tu flora intestinal y ayuda a crear bacterias más resistentes.", "gripe, resfriado", new List<(string, string)> {
                ("¿Por qué un antibiótico no mata a un virus?", "Los antibióticos atacan la estructura física de la bacteria (su pared). Los virus no tienen esa estructura, por lo que el antibiótico simplemente no tiene nada a qué atacar."),
                ("¿Qué pasa con mi flora intestinal si tomo antibióticos sin necesidad?", "El antibiótico mata a las bacterias 'buenas' de tu vientre. Esto causa diarreas, debilita tus defensas y deja el camino libre a hongos y bacterias malas."),
                ("¿Qué medicamentos sí son efectivos para síntomas virales?", "Para virus se usan analgésicos, hidratación y reposo. Los antibióticos NO bajan la fiebre ni quitan el moco si la causa es un virus.")
            });

            await AddQA(catRes.Id, "¿Es seguro interrumpir el tratamiento de antibióticos antes de tiempo?", "Nunca dejes un tratamiento de antibióticos a la mitad. Aunque te sientas mejor, debes terminar la caja para asegurar que no sobresurva ninguna bacteria fuerte.", "interrumpir, tratamiento", new List<(string, string)> {
                ("¿Por qué me siento bien antes de terminar la caja?", "Porque el antibiótico mató a las bacterias más débiles primero. Las que quedan vivas son las más fuertes y peligrosas; si dejas de tomarlo, esas sobrevivientes te volverán a enfermar peor."),
                ("¿Qué sucede con las bacterias que 'sobreviven' al corte?", "Se vuelven líderes de una nueva infección que ya sabe cómo resistir a ese antibiótico. La próxima vez que lo tomes, ya no te servirá de nada."),
                ("¿Cómo se crea una infección recurrente por falta de adherencia?", "Al no terminar el ciclo, dejas focos de infección dormidos que despertarán en semanas o meses con mucha más agresividad.")
            });

            await AddQA(catRes.Id, "¿Cómo afecta el mal uso de antibióticos a la salud global (One Health)?", "El mal uso de fármacos afecta a humanos, animales y al medio ambiente por igual. Es un problema global que genera un entorno lleno de bacterias resistentes.", "one health, global", new List<(string, string)> {
                ("¿Qué tiene que ver la salud de los animales con la mía?", "Si se usan antibióticos para engordar pollos o vacas, las bacterias de esos animales se vuelven resistentes y saltan a los humanos a través de la comida o el contacto."),
                ("¿Cómo llegan los antibióticos de la granja a nuestras mesas?", "A través del agua contaminada con desechos animales y el consumo de carne mal cocida que contiene bacterias que ya aprendieron a ser súper resistentes."),
                ("¿Cómo afecta el desecho de medicinas al medio ambiente?", "Tirar medicinas al baño contamina ríos. Las bacterias del agua aprenden a resistir a esos fármacos, creando un ambiente donde hasta el agua puede ser foco de superbacterias.")
            });

            // --- CATEGORÍA: MITOS ---
            await AddQA(catMit.Id, "¿Puedo usar medicamentos recomendados por otras personas?", "Lo que le sirvió a un conocido podría ser tóxico para ti. Cada cuerpo es único y un fármaco 'seguro' para otro puede causarte una reacción grave.", "mitos, recomendacion", new List<(string, string)> {
                ("¿Por qué lo que le sirve a un vecino me puede hacer daño a mí?", "Tu genética, historial de alergias y el estado de tus riñones son un mundo aparte. Un fármaco 'seguro' para tu vecino puede darte un ataque al corazón o insuficiencia renal a ti."),
                ("¿Cómo influye el peso y la edad en la dosis de cada persona?", "Un niño o un anciano procesan los fármacos mucho más lento. Darle una dosis de adulto a un niño puede causar daños cerebrales o la muerte por sobredosis."),
                ("¿Qué son las interacciones medicamentosas cruzadas?", "Es cuando un fármaco choca con otro que ya tomas. El recomendado por tu amigo podría anular tu medicina para la presión o causar una hemorragia interna.")
            });

            await AddQA(catMit.Id, "¿Son siempre inofensivos los productos naturales?", "Es un mito que 'Natural' significa inofensivo. Muchas plantas medicinales tienen químicos potentes que pueden dañar tu hígado si se usan mal.", "natural, botánico", new List<(string, string)> {
                ("¿Significa 'Natural' que no tiene efectos secundarios?", "¡No! El veneno de serpiente es natural. Muchas plantas medicinales causan toxicidad hepática grave si se consumen en dosis incorrectas."),
                ("¿Pueden las hierbas anular el efecto de mis medicinas?", "Sí. Por ejemplo, la hierba de San Juan anula el efecto de muchos anticonceptivos y antidepresivos. Lo natural también es químico."),
                ("¿Por qué falta regulación en la dosis de productos botánicos?", "A diferencia de las pastillas, una planta puede tener más o menos veneno dependiendo de donde creció. No hay control exacto de cuánto químico 'natural' estás tragando.")
            });

            await AddQA(catMit.Id, "¿Cuáles son las señales de una reacción adversa a un farmaco?", "Conocer las señales de una reacción adversa (como ronchas, picazón o falta de aire) te permite actuar rápido y evitar complicaciones vitales.", "alergia, reaccion", new List<(string, string)> {
                ("¿Cómo distingo un efecto secundario de una alergia?", "Un efecto secundario es 'esperado' (ej. sueño). Una alergia es una defensa extrema del cuerpo (ronchas, picazón, ojos hinchados) y es mucho más peligrosa."),
                ("¿Qué es un choque anafiláctico y cómo detectarlo a tiempo?", "Es la reacción más grave: se cierra la garganta y baja la presión. Si te cuesta respirar tras una pastilla, es una emergencia vital de vida o muerte."),
                ("¿A qué entidad debo reportar una reacción médica extraña?", "Debes avisar a tu médico y, si es posible, al sistema de Farmacovigilancia de tu país para que alerten a otros sobre ese lote de medicina.")
            });

            await AddQA(catMit.Id, "¿Cuándo es indispensable acudir a un médico profesional?", "La consulta médica es la única forma de obtener un diagnóstico real. Este chat es educativo y nunca debe retrasar la atención profesional ante síntomas graves.", "medico, urgencia", new List<(string, string)> {
                ("¿Qué síntomas de alerta requieren ir a urgencias ya mismo?", "Dolor de pecho, pérdida de visión, desmayos, fiebre que no baja o sangrados inusuales. No preguntes a un chat, ¡ve al hospital!"),
                ("¿Por qué la receta médica es un documento de seguridad?", "La receta confirma que un experto analizó tu cuerpo y decidió que el beneficio de la medicina es mayor que el riesgo. Es tu escudo legal y de salud."),
                ("¿Cuál es el peligro de postergar un diagnóstico real por usar IA?", "La IA analiza datos, no a la persona. Confiar ciegamente en un chat para una enfermedad real puede hacer que pierdas meses valiosos de tratamiento para algo grave.")
            });

            return Content("✅ ¡RESTAURACIÓN COMPLETA! Las 48 preguntas originales han sido cargadas en la base de datos.");
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
