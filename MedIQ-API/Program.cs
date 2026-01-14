using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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

app.Run();