using System.Text;
using System.Text.Json;
using BeerCsvTask.Dto;
using Microsoft.Extensions.Configuration;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        var start = DateTime.Now;
        Console.WriteLine($"[{start:HH:mm:ss}] Inicio del proceso diario...");

        try
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Iniciando tarea...");
            await LogAsync("C:\\Data\\exports2\\beer_task_log.txt", "Inicio de ejecución manual");

            var cfg = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var apiBase = cfg["ApiBaseUrl"] ?? throw new Exception("ApiBaseUrl no configurado");
            var csvPath = cfg["Csv:OutputPath"] ?? "C:\\Data\\exports2\\beers.csv";
            var withHeader = bool.TryParse(cfg["Csv:WithHeader"], out var wh) ? wh : true;
            var logFile = cfg["Logging:LogFile"] ?? "C:\\Data\\exports2\\beer_task_log.txt";

            await LogAsync(logFile, $"[{start}] Iniciando exportación CSV");

            using var http = new HttpClient();
            var res = await http.GetAsync($"{apiBase.TrimEnd('/')}/api/beers");

            if (!res.IsSuccessStatusCode)
            {
                var msg = $"Falla al consumir API ({(int)res.StatusCode}) {res.ReasonPhrase}";
                Console.WriteLine(msg);
                await LogAsync(logFile, msg);
                return 1;
            }

            var bytes = await res.Content.ReadAsByteArrayAsync();
            var beers = JsonSerializer.Deserialize<List<BeerDto>>(bytes,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var csv = BuildCsv(beers, withHeader);
            var dir = Path.GetDirectoryName(csvPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllTextAsync(csvPath, csv, new UTF8Encoding(false));

            Console.WriteLine($"Archivo generado en: {csvPath}");
            await LogAsync(logFile, $"CSV generado correctamente ({beers.Count} registros)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            await LogAsync("C:\\Data\\exports2\\beer_task_log.txt", $"ERROR: {ex}");
            return 1;
        }
    }

    static string BuildCsv(IEnumerable<BeerDto> list, bool withHeader)
    {
        var sb = new StringBuilder();
        if (withHeader)
            sb.AppendLine("BeerId,Name,BrandId,CountryCode,FlagUrl");
        static string E(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";
        foreach (var b in list)
            sb.Append(b.BeerId).Append(',')
              .Append(E(b.Name)).Append(',')
              .Append(b.BrandId).Append(',')
              .Append(E(b.CountryCode)).Append(',')
              .Append(E(b.FlagUrl)).AppendLine();
        return sb.ToString();
    }

    static async Task LogAsync(string path, string msg)
    {
        var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\n";
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        await File.AppendAllTextAsync(path, logLine);
    }
}
