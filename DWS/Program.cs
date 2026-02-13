using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MedIQ_Modelos;
using MedIQ_API.Data;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });

// Fix for Render/inotify limit: Disable configuration reload on change
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
if (args != null) builder.Configuration.AddCommandLine(args);

// --- 1. CONFIGURACIÓN DEL SERVIDOR (RENDER) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

// --- 2. REGISTRO DE SERVICIOS (TODO ANTES DE builder.Build()) ---
builder.Services.AddControllersWithViews();

// CONFIGURACIÓN DE LA BASE DE DATOS EN LA NUBE
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'AppDbContext' no fue encontrada.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// CONFIGURACIÓN DE AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "MedIQ_Auth";
    });

// CONFIGURACIÓN DE DATA PROTECTION (Evita que las cookies se corrompan al reiniciar)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
    .SetApplicationName("MedIQ");

// SERVICIOS PERSONALIZADOS
builder.Services.AddMemoryCache();
builder.Services.AddScoped<DWS.Services.IEmailService, DWS.Services.EmailService>();

// --- 3. CONSTRUCCIÓN DE LA APLICACIÓN ---
var app = builder.Build(); // <--- Ahora sí, todos los servicios están registrados

// --- 4. CONFIGURACIÓN DEL PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Nota: En Render, el SSL lo maneja su propio Proxy. 
// Si ves errores de redirección infinita, mantén esta línea comentada.
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// RUTA POR DEFECTO: Inicia directamente en el Login del MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}");

// AUTO-MIGRACIÓN (SEGURO PARA PRODUCCIÓN)
// EF Core aplica solo las migraciones pendientes sin borrar datos existentes.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Asegurar que la DB exista y tenga el esquema actual
        context.Database.EnsureCreated(); 
        Console.WriteLine("✅ Migraciones aplicadas correctamente.");

        // SEED DATA: Crear usuario administrador por defecto
        if (!context.Usuarios.Any(u => u.Email == "admin@mediq.com"))
        {
            var admin = new Usuario
            {
                Nombre = "Administrador",
                Email = "admin@mediq.com",
                Contraseña = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Rol = "Admin"
            };
            context.Usuarios.Add(admin);
            context.SaveChanges();
            Console.WriteLine("✅ Usuario administrador creado.");
        }

        // SEED DATA: Contenido (Preguntas y Respuestas)
        SeedContent(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error migrando DB desde DWS.");
    }
}

app.Run();

// --- MÉTODO AUXILIAR PARA SEEDING DE CONTENIDO ---
void SeedContent(AppDbContext context)
{
    if (context.CategoriasConocimiento.Any()) return; // Si ya hay datos, no hacemos nada

    Console.WriteLine("🌱 Sembrando datos de conocimiento...");

    // 1. Categoría: Medicación
    var catMedicacion = new ConocimientoCategoria { Nombre = "Guía de Medicación", Icono = "💊", Descripcion = "Uso Responsable" };
    context.CategoriasConocimiento.Add(catMedicacion);
    context.SaveChanges(); // Guardamos para tener el Id

    AddPregunta(context, catMedicacion.Id, 
        "¿En qué consiste exactamente la automedicación?", 
        "La automedicación es el uso de medicamentos por iniciativa propia sin receta médica. Puede ser peligroso si no se hace bajo supervisión profesional, ya que puede enmascarar enfermedades reales.",
        "medicacion, automedicacion, receta",
        new[] {
            ("¿Qué diferencia hay entre automedicación y autocuidado?", "El autocuidado es elegir hábitos saludables (dieta, ejercicio), mientras que la automedicación es usar fármacos sin receta para tratar síntomas. El autocuidado previene, la automedicación mal hecha pone en riesgo."),
            ("¿Cuáles son los riesgos de ocultar síntomas graves?", "Automedicarse para un dolor de estómago fuerte podría 'tapar' una apendicitis. Al no sentir el dolor, no buscas ayuda profesional y una condición tratable puede volverse mortal."),
            ("¿Cómo afecta la automedicación a la seguridad del paciente?", "Aumenta la probabilidad de interacciones peligrosas, errores en la dosis y desarrollo de alergias no detectadas, según la OMS es una de las mayores amenazas para la salud pública.")
        });

    AddPregunta(context, catMedicacion.Id,
        "¿Qué elementos debo revisar al leer una receta o etiqueta médica?",
        "Leer la receta es clave para entender la dosis exacta, la frecuencia y la duración total del tratamiento, evitando errores que comprometan tu recuperación.",
        "receta, etiqueta, dosis",
        new[] {
            ("¿Dónde encuentro la fecha de vencimiento y por qué importa?", "Suele estar en el borde del blister o la caja. Tomar medicina vencida es peligroso porque los componentes químicos se degradan y pueden volverse tóxicos o perder su efecto."),
            ("¿Qué significa 'Vía de Administración' (Oral, Tópica, etc.)?", "Indica cómo debe entrar el fármaco al cuerpo. Si pones gotas para el oído en el ojo, o tragas una pastilla que era sublingual, el medicamento no funcionará o causará daño."),
            ("¿Cómo identifico excipientes que podrían darme alergia?", "En el prospecto (papel interno), busca la lista de excipientes. Sustancias como lactosa o gluten pueden causar reacciones graves en personas sensibles.")
        });

    AddPregunta(context, catMedicacion.Id,
        "¿Por qué es fundamental respetar los horarios indicados?",
        "Respetar los horarios garantiza que el medicamento mantenga niveles estables en tu sangre durante todo el día, asegurando que el tratamiento realmente funcione.",
        "horario, dosis, frecuencia",
        new[] {
            ("¿Es lo mismo '3 veces al día' que 'cada 8 horas'?", "No. '3 veces' puede ser aleatorio (desayuno, almuerzo, cena). 'Cada 8 horas' es estricto para mantener el nivel de fármaco estable en sangre durante las 24 horas del día."),
            ("¿Qué es la 'Ventana Terapéutica' de un medicamento?", "Es el rango exacto de dosis donde el fármaco cura. Si bajas de ahí no sirve; si subes de ahí se vuelve veneno para tus órganos (riñón o hígado)."),
            ("¿Cómo influyen los alimentos en la absorción del fármaco?", "Algunos fármacos necesitan grasa para absorberse, otros se bloquean con el calcio de la leche. Seguir la instrucción 'con alimentos' o 'en ayunas' determina si la medicina entra a tu sangre.")
        });
    
    AddPregunta(context, catMedicacion.Id,
        "¿Qué debo hacer ante el olvido de una dosis?",
        "Ante un olvido, lo más importante es no entrar en pánico. Debes evaluar cuánto tiempo ha pasado, pero recuerda: **nunca tomes doble dosis**.",
        "olvido, dosis, doble",
        new[] {
            ("¿Existe alguna 'regla de tiempo' para tomarla tarde?", "Generalmente, si te acuerdas antes de la mitad del tiempo para la siguiente dosis, tómala. Si falta poco para la siguiente, es mejor esperar y seguir con el horario normal."),
            ("¿Por qué NUNCA debo duplicar la dosis para compensar?", "Duplicar la dosis NO arregla el olvido, solo sobrecarga tus riñones e hígado con una cantidad tóxica que tu cuerpo no puede procesar de golpe."),
            ("¿Qué riesgos hay en tratamientos críticos como anticonceptivos?", "En tratamientos donde la hormona es constante, un olvido de más de 12 horas puede anular la eficacia totalmente. En estos casos, se debe usar un método de barrera (preservativo) adicional.")
        });


    // 2. Categoría: Resistencia
    var catResistencia = new ConocimientoCategoria { Nombre = "Resistencia Antimicrobiana", Icono = "🛡️", Descripcion = "Peligros y Prevención" };
    context.CategoriasConocimiento.Add(catResistencia);
    context.SaveChanges();

    AddPregunta(context, catResistencia.Id,
        "¿Qué es la resistencia bacteriana a los antibióticos?",
        "La resistencia bacteriana ocurre cuando las bacterias aprenden a sobrevivir a los antibióticos. Esto hace que infecciones comunes vuelvan a ser peligrosas y difíciles de tratar.",
        "resistencia, bacteria, antibiotico",
        new[] {
            ("¿Cómo hacen las bacterias para volverse 'superbacterias'?", "Las bacterias mutan y desarrollan 'escudos' o bombas para expulsar el antibiótico. Al reproducirse, pasan este 'superpoder' a otras bacterias, creando una familia resistente."),
            ("¿Cuál es la diferencia entre resistencia natural y adquirida?", "La natural es propia de la bacteria. La adquirida ocurre por culpa nuestra: al usar mal los antibióticos obligamos a la bacteria a aprender cómo sobrevivir."),
            ("¿Por qué la OMS considera esto una amenaza para la humanidad?", "Si los antibióticos dejan de funcionar, cirugías simples o partos volverán a ser mortales por infecciones que hoy consideramos fáciles de curar.")
        });

    AddPregunta(context, catResistencia.Id,
        "¿Los antibióticos sirven para tratar la gripe o el resfriado común?",
        "Los antibióticos NO sirven para combatir virus como la gripe. Usarlos sin necesidad solo daña tu flora intestinal y ayuda a crear bacterias más resistentes.",
        "gripe, virus, resfriado",
        new[] {
            ("¿Por qué un antibiótico no mata a un virus?", "Los antibióticos atacan la estructura física de la bacteria (su pared). Los virus no tienen esa estructura, por lo que el antibiótico simplemente no tiene nada a qué atacar."),
            ("¿Qué pasa con mi flora intestinal si tomo antibióticos sin necesidad?", "El antibiótico mata a las bacterias 'buenas' de tu vientre. Esto causa diarreas, debilita tus defensas y deja el camino libre a hongos y bacterias malas."),
            ("¿Qué medicamentos sí son efectivos para síntomas virales?", "Para virus se usan analgésicos, hidratación y reposo. Los antibióticos NO bajan la fiebre ni quitan el moco si la causa es un virus.")
        });

     AddPregunta(context, catResistencia.Id,
        "¿Es seguro interrumpir el tratamiento de antibióticos antes de tiempo?",
        "Nunca dejes un tratamiento de antibióticos a la mitad. Aunque te sientas mejor, debes terminar la caja para asegurar que no sobreviva ninguna bacteria fuerte.",
        "interrumpir, tratamiento, antibióticos",
        new[] {
            ("¿Por qué me siento bien antes de terminar la caja?", "Porque el antibiótico mató a las bacterias más débiles primero. Las que quedan vivas son las más fuertes y peligrosas; si dejas de tomarlo, esas sobrevivientes te volverán a enfermar peor."),
            ("¿Qué sucede con las bacterias que 'sobreviven' al corte?", "Se vuelven líderes de una nueva infección que ya sabe cómo resistir a ese antibiótico. La próxima vez que lo tomes, ya no te servirá de nada."),
            ("¿Cómo se crea una infección recurrente por falta de adherencia?", "Al no terminar el ciclo, dejas focos de infección dormidos que despertarán en semanas o meses con mucha más agresividad.")
        });

     AddPregunta(context, catResistencia.Id,
        "¿Cómo afecta el mal uso de antibióticos a la salud global (One Health)?",
        "El mal uso de fármacos afecta a humanos, animales y al medio ambiente por igual. Es un problema global que genera un entorno lleno de bacterias resistentes.",
        "one health, salud global, medio ambiente",
        new[] {
            ("¿Qué tiene que ver la salud de los animales con la mía?", "Si se usan antibióticos para engordar pollos o vacas, las bacterias de esos animales se vuelven resistentes y saltan a los humanos a través de la comida o el contacto."),
            ("¿Cómo llegan los antibióticos de la granja a nuestras mesas?", "A través del agua contaminada con desechos animales y el consumo de carne mal cocida que contiene bacterias que ya aprendieron a ser súper resistentes."),
            ("¿Cómo afecta el desecho de medicinas al medio ambiente?", "Tirar medicinas al baño contamina ríos. Las bacterias del agua aprenden a resistir a esos fármacos, creando un ambiente donde hasta el agua puede ser foco de superbacterias.")
        });


    // 3. Categoría: Mitos
    var catMitos = new ConocimientoCategoria { Nombre = "Mitos y Realidades", Icono = "⚖️", Descripcion = "Precauciones" };
    context.CategoriasConocimiento.Add(catMitos);
    context.SaveChanges();

    AddPregunta(context, catMitos.Id,
        "¿Puedo usar medicamentos recomendados por otras personas?",
        "Lo que le sirvió a un conocido podría ser tóxico para ti. Cada cuerpo es único y un fármaco 'seguro' para otro puede causarte una reacción grave.",
        "recomendacion, vecino, amigo",
        new[] {
            ("¿Por qué lo que le sirve a un vecino me puede hacer daño a mí?", "Tu genética, historial de alergias y el estado de tus riñones son un mundo aparte. Un fármaco 'seguro' para tu vecino puede darte un ataque al corazón o insuficiencia renal a ti."),
            ("¿Cómo influye el peso y la edad en la dosis de cada persona?", "Un niño o un anciano procesan los fármacos mucho más lento. Darle una dosis de adulto a un niño puede causar daños cerebrales o la muerte por sobredosis."),
            ("¿Qué son las interacciones medicamentosas cruzadas?", "Es cuando un fármaco choca con otro que ya tomas. El recomendado por tu amigo podría anular tu medicina para la presión o causar una hemorragia interna.")
        });

    AddPregunta(context, catMitos.Id,
        "¿Son siempre inofensivos los productos naturales?",
        "Es un mito que 'Natural' significa inofensivo. Muchas plantas medicinales tienen químicos potentes que pueden dañar tu hígado si se usan mal.",
        "natural, plantas, hierbas",
        new[] {
            ("¿Significa 'Natural' que no tiene efectos secundarios?", "¡No! El veneno de serpiente es natural. Muchas plantas medicinales causan toxicidad hepática grave si se consumen en dosis incorrectas."),
            ("¿Pueden las hierbas anular el efecto de mis medicinas?", "Sí. Por ejemplo, la hierba de San Juan anula el efecto de muchos anticonceptivos y antidepresivos. Lo natural también es químico."),
            ("¿Por qué falta regulación en la dosis de productos botánicos?", "A diferencia de las pastillas, una planta puede tener más o menos veneno dependiendo de donde creció. No hay control exacto de cuánto químico 'natural' estás tragando.")
        });

    AddPregunta(context, catMitos.Id,
        "¿Cuáles son las señales de una reacción adversa a un farmaco?",
        "Conocer las señales de una reacción adversa (como ronchas, picazón o falta de aire) te permite actuar rápido y evitar complicaciones vitales.",
        "reaccion, alergia, efectos secundarios",
        new[] {
            ("¿Cómo distingo un efecto secundario de una alergia?", "Un efecto secundario es 'esperado' (ej. sueño). Una alergia es una defensa extrema del cuerpo (ronchas, picazón, ojos hinchados) y es mucho más peligrosa."),
            ("¿Qué es un choque anafiláctico y cómo detectarlo a tiempo?", "Es la reacción más grave: se cierra la garganta y baja la presión. Si te cuesta respirar tras una pastilla, es una emergencia vital de vida o muerte."),
            ("¿A qué entidad debo reportar una reacción médica extraña?", "Debes avisar a tu médico y, si es posible, al sistema de Farmacovigilancia de tu país para que alerten a otros sobre ese lote de medicina.")
        });

    AddPregunta(context, catMitos.Id,
        "¿Cuándo es indispensable acudir a un médico profesional?",
        "La consulta médica es la única forma de obtener un diagnóstico real. Este chat es educativo y nunca debe retrasar la atención profesional ante síntomas graves.",
        "medico, urgencias, doctor",
        new[] {
            ("¿Qué síntomas de alerta requieren ir a urgencias ya mismo?", "Dolor de pecho, pérdida de visión, desmayos, fiebre que no baja o sangrados inusuales. No preguntes a un chat, ¡ve al hospital!"),
            ("¿Por qué la receta médica es un documento de seguridad?", "La receta confirma que un experto analizó tu cuerpo y decidió que el beneficio de la medicina es mayor que el riesgo. Es tu escudo legal y de salud."),
            ("¿Cuál es el peligro de postergar un diagnóstico real por usar IA?", "La IA analiza datos, no a la persona. Confiar ciegamente en un chat para una enfermedad real puede hacer que pierdas meses valiosos de tratamiento para algo grave.")
        });

    Console.WriteLine("✅ Datos sembrados correctamente.");
}

void AddPregunta(AppDbContext context, int catId, string p, string r, string k, (string q, string a)[] subs)
{
    var pregunta = new ConocimientoQA { 
        Pregunta = p, 
        Respuesta = r, 
        Keywords = k, 
        CategoriaId = catId, 
        FechaCreacion = DateTime.UtcNow,
        FechaActualizacion = DateTime.UtcNow
    };
    context.PreguntasConocimiento.Add(pregunta);
    context.SaveChanges();

    foreach (var sub in subs)
    {
        context.PreguntasConocimiento.Add(new ConocimientoQA {
            Pregunta = sub.q,
            Respuesta = sub.a,
            Keywords = k,
            CategoriaId = catId,
            ParentId = pregunta.Id, // Enlace a la pregunta padre
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        });
    }
    context.SaveChanges();
}
