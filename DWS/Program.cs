using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using DWS.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DEL SERVIDOR (RENDER) ---
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://*:{port}");

// --- 2. REGISTRO DE SERVICIOS (TODO ANTES DE builder.Build()) ---
builder.Services.AddControllersWithViews();

// CONFIGURACIÓN DE LA BASE DE DATOS EN LA NUBE
var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// CONFIGURACIÓN DE AUTENTICACIÓN
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.Cookie.Name = "MedIQ_Auth";
    });

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
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();