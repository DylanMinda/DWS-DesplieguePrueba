using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MedIQ_API.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. EXTRAER LA CADENA DE CONEXIÓN DE FORMA SEGURA
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");

// Validación para evitar el error "Host can't be null" si el JSON está mal leído
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("No se pudo encontrar la cadena de conexión 'AppDbContext'.");
}

// 2. CONFIGURACIÓN DEL DBCONTEXT
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, // Usamos la variable limpia
    npgsqlOptions => {
        // Estrategia de reintento para la capa gratuita de Render (evita fallos temporales)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    }));

// 3. SERVICIOS ESTÁNDAR
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. CONFIGURACIÓN DEL PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Nota: UseHttpsRedirection a veces causa problemas en Render si no está configurado el certificado en el proxy.
// Si ves errores de "Too many redirects", comenta la siguiente línea:
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// 5. AUTO-MIGRACIÓN PARA RENDER
// Esto asegura que la DB se actualice sola al hacer Deploy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // Crea la base de datos si no existe
        Console.WriteLine("Migraciones aplicadas correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al migrar la base de datos.");
    }
}

app.Run();