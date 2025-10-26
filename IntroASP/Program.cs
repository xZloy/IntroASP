using IntroASP.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("https://localhost:7084")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---------- LOGGING (Serilog: consola + archivo) ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/app-.log",           
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,      
        shared: true)
    .CreateLogger();

builder.Host.UseSerilog(); // usa Serilog como backend de ILogger

// HttpClient para BeerRazorController y demás
builder.Services.AddHttpClient();

// MVC + API (evitar ciclos JSON de EF)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// DbContext con tu cadena "PubContext"
builder.Services.AddDbContext<PubContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("PubContext"));
});

// Swagger para probar la API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI();
//app.UseCors("Frontend"); 



// Rutas MVC por convención
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Rutas API por atributos [ApiController]/[Route]
app.MapControllers();

app.Run();
