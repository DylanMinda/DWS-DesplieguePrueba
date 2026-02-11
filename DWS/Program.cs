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
            Console.WriteLine("✅ Usuario administrador creado:");
            Console.WriteLine("   📧 Email: admin@mediq.com");
            Console.WriteLine("   🔑 Contraseña: Admin123!");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error migrando DB desde DWS.");
    }
}

app.Run();