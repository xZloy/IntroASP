using BeerCsWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
// Registro de servicios
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

// Aquí agregas soporte a Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Beer CSV Exporter";
});
var host = builder.Build();
host.Run();
