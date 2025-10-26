using IntroASP.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;

public class BeerController : Controller
{
    
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<BeerController> logger;
    private readonly string _apiBase;
    public BeerController(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
        this.logger = logger;
        _apiBase = config["ApiBaseUrl"] ?? "https://localhost:7084";
        
    }

    public IActionResult Index()
    {
        //_logger.LogInformation("Se solicito el listado de x {Time}", DateTime.Now);
        return View(); // tu vista con fetch
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync($"{_apiBase}/api/beers");
        if (!response.IsSuccessStatusCode) return View("Error");

        var json = await response.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<Beer>>(json) ?? new();

        var sb = new StringBuilder();
        sb.AppendLine("BeerId,Name,BrandId");
        foreach (var b in list)
        {
            var name = (b.Name ?? "").Replace("\"", "\"\"");
            sb.AppendLine($"{b.BeerId},\"{name}\",{b.BrandId}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "beers.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf()
    {
        var client = _httpFactory.CreateClient();
        var response = await client.GetAsync($"{_apiBase}/api/beers");
        if (!response.IsSuccessStatusCode) return View("Error");

        var json = await response.Content.ReadAsStringAsync();
        var list = JsonConvert.DeserializeObject<List<Beer>>(json) ?? new();

        QuestPDF.Settings.License = LicenseType.Community;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Reporte de Cervezas").Bold().FontSize(18);
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(60);
                        cols.RelativeColumn(3);
                        cols.ConstantColumn(80);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Id").Bold();
                        header.Cell().Text("Nombre").Bold();
                        header.Cell().Text("BrandId").Bold();
                    });

                    foreach (var b in list)
                    {
                        table.Cell().Text(b.BeerId.ToString());
                        table.Cell().Text(b.Name ?? "");
                        table.Cell().Text(b.BrandId.ToString());
                    }
                });
                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();

        return File(bytes, "application/pdf", "beers.pdf");
    }
}
