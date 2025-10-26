using System.Text;
using System.Text.Json;
using BeerCsWorker.Dtos;
using Microsoft.Extensions.Http;
namespace BeerCsWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public Worker(ILogger<Worker> logger, IHttpClientFactory http, IConfiguration cfg)
    {
        _logger = logger;
        _http = http;
        _cfg = cfg;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var api = _cfg.GetValue<string>("ApiBaseUrl")?.TrimEnd('/')
                    ?? throw new InvalidOperationException("ApiBaseUrl no configurada.");
                var outPath = _cfg.GetValue<string>("Csv:OutputPath")
                    ?? throw new InvalidOperationException("Csv:OutputPath no configurado.");
                var withHeader = _cfg.GetValue("Csv:WithHeader", true);

                _logger.LogInformation("Inicio export CSV {Time}", DateTimeOffset.Now);

                var cli = _http.CreateClient();
                var url = $"{api}/api/beers";

                _logger.LogInformation("GET {Url}", url);
                var res = await cli.GetAsync(url, stoppingToken);

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogError("Falló GET {Status} {Reason}", (int)res.StatusCode, res.ReasonPhrase);
                }
                else
                {
                    var bytes = await res.Content.ReadAsByteArrayAsync(stoppingToken);
                    var beers = JsonSerializer.Deserialize<List<BeerDto>>(bytes, JsonOpts) ?? new();

                    _logger.LogInformation("Registros recibidos: {Count}", beers.Count);

                    var csv = BuildCsv(beers, withHeader);

                    var dir = Path.GetDirectoryName(outPath);
                    if (!string.IsNullOrWhiteSpace(dir))
                        Directory.CreateDirectory(dir);

                    await File.WriteAllTextAsync(outPath, csv, new UTF8Encoding(false), stoppingToken);

                    _logger.LogInformation("CSV escrito en {Path} (len={Len})", outPath, csv.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exportando CSV");
            }
            finally
            {
                _logger.LogInformation("Fin export CSV {Time}", DateTimeOffset.Now);
            }

          
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }


    private static string BuildCsv(IEnumerable<BeerDto> list, bool withHeader)
    {
        var sb = new StringBuilder(16 * 1024);

        if (withHeader)
            sb.AppendLine("BeerId,Name,BrandId,CountryCode,FlagUrl");

        static string E(string? s) => $"\"{(s ?? "").Replace("\"", "\"\"")}\"";

        foreach (var b in list)
        {
            sb.Append(b.BeerId).Append(',')
              .Append(E(b.Name)).Append(',')
              .Append(b.BrandId).Append(',')
              .Append(E(b.CountryCode)).Append(',')
              .Append(E(b.FlagUrl))
              .AppendLine();
        }
        return sb.ToString();
    }
}
