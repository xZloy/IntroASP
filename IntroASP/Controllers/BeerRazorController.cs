using System.Text;
using System.Text.Json;
using IntroASP.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

public class BeerRazorController : Controller
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<BeerRazorController> _logger;
    private readonly string? _apiBase; 

    private static readonly JsonSerializerOptions JsonOpts = new()
    { PropertyNameCaseInsensitive = true };

    public BeerRazorController(IHttpClientFactory http, ILogger<BeerRazorController> logger, IConfiguration cfg)
    {
        _http = http; 
        _apiBase = cfg["ApiBaseUrl"];
        _logger = logger;
    }
    private string Api(string path)
    {
        var baseUrl = string.IsNullOrWhiteSpace(_apiBase)
            ? $"{Request.Scheme}://{Request.Host}"
            : _apiBase!;
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    // LISTAR
    public async Task<IActionResult> Index()
    {
        var url = Api("/api/beers");
        _logger.LogInformation($"[UI] GET {url}");
        var cli = _http.CreateClient();
        var res = await cli.GetAsync(Api("/api/beers"));
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError($"[UI] Falló GET {url}: {(int)res.StatusCode}");
            TempData["Error"] = $"No se pudo obtener el listado (HTTP {(int)res.StatusCode}).";
            return View(new List<BeerDto>());
        }
        var beers = JsonSerializer.Deserialize<List<BeerDto>>(
            await res.Content.ReadAsByteArrayAsync(), JsonOpts) ?? new();

        _logger.LogInformation($"[UI] Listado OK: {beers.Count} items");
        return View(beers);
    }

    // CREATE GET
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var countries = await GetCountryAsync();
        ViewData["Countries"] = countries;
        
        ViewData["Brands"] = new SelectList(await GetBrandsAsync(), "BrandId", "Name");
        return View(new BeerDto());
    }

    // CREATE POST -> llama API POST
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BeerDto model)
    {
        _logger.LogInformation($"[UI] POST Create Name='{model.Name}', BrandId={model.BrandId}");
        if (!ModelState.IsValid)
        {
            ViewData["Brands"] = new SelectList(await GetBrandsAsync(), "BrandId", "Name", model.BrandId);
            ViewData["Countries"] = await GetCountryAsync();
            return View(model);
        }
        var cli = _http.CreateClient();
        var payload = JsonSerializer.Serialize(new
        {
            name = model.Name,
            brandId = model.BrandId,
            countryCode = model.CountryCode,
            flagUrl = model.FlagUrl
        });
        var res = await cli.PostAsync(Api("/api/beers"),
            new StringContent(payload, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            TempData["Error"] = "No se pudo crear.";
            ViewData["Brands"] = new SelectList(await GetBrandsAsync(), "BrandId", "Name", model.BrandId);
            return View(model);
        }
        _logger.LogInformation($"[UI] API POST /api/beers → {(int)res.StatusCode}");
        TempData["Ok"] = "Cerveza creada.";
        return RedirectToAction(nameof(Index));
    }

    // EDIT GET
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        _logger.LogInformation($"[UI] GET Edit {id}");
        var cli = _http.CreateClient();
        var res = await cli.GetAsync(Api($"/api/beers/{id}"));
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return NotFound();
        if (!res.IsSuccessStatusCode) { _logger.LogError($"[UI] GET beer {id} → {(int)res.StatusCode}"); return View("Error"); }

        var beer = JsonSerializer.Deserialize<BeerDto>(
            await res.Content.ReadAsByteArrayAsync(), JsonOpts);
        ViewData["Brands"] = new SelectList(await GetBrandsAsync(), "BrandId", "Name", beer!.BrandId);
        
        ViewData["Countries"] = await GetCountryAsync();

        return View(beer);
    }

    // EDIT POST -> llama API PUT
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BeerDto model)
    {
        _logger.LogInformation($"[UI] POST Edit {id} Name='{model.Name}', BrandId={model.BrandId}");
        if (id != model.BeerId)
            ModelState.AddModelError("", "Id inconsistente entre ruta y formulario.");
        if (!ModelState.IsValid)
        {
            var errors = ModelState
        .Where(kv => kv.Value!.Errors.Count > 0)
        .Select(kv => new { Field = kv.Key, Messages = kv.Value!.Errors.Select(e => e.ErrorMessage) });

            _logger.LogWarning("ModelState inválido en Edit({Id}): {Errors}", id, JsonSerializer.Serialize(errors));

            ViewData["Countries"] = await GetCountryAsync();
            ViewData["Brands"] = new SelectList(await GetBrandsAsync(), "BrandId", "Name", model.BrandId);
            return View(model);
        }

        var cli = _http.CreateClient();
        var payload = JsonSerializer.Serialize(new
        {
            name = model.Name,
            brandId = model.BrandId,
            countryCode = model.CountryCode,
            flagUrl = model.FlagUrl
        });
        
        var res = await cli.PutAsync(
            Api($"/api/beers/{id}"),
            new StringContent(payload, Encoding.UTF8, "application/json"));
        _logger.LogInformation($"[UI] API PUT /api/beers/{id} → {(int)res.StatusCode}");

        TempData[res.IsSuccessStatusCode ? "Ok" : "Error"] =
            res.IsSuccessStatusCode ? "Actualizado." : "No se pudo actualizar.";
        return RedirectToAction(nameof(Index));
    }

    // DELETE POST -> llama API DELETE
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation($"[UI] POST Delete {id}");
        var cli = _http.CreateClient();
        var res = await cli.DeleteAsync(Api($"/api/beers/{id}"));
        _logger.LogInformation($"[UI] API DELETE /api/beers/{id} → {(int)res.StatusCode}");
        TempData[res.IsSuccessStatusCode ? "Ok" : "Error"] =
            res.IsSuccessStatusCode ? "Eliminado." : "No se pudo eliminar.";
        return RedirectToAction(nameof(Index));
    }

    // helper marcas
    private async Task<List<BrandDto>> GetBrandsAsync()
    {
        var url = Api("/api/brands");
        _logger.LogInformation($"[UI] GET {url}");
        var cli = _http.CreateClient();
        var res = await cli.GetAsync(Api("/api/brand")); 
        if (!res.IsSuccessStatusCode) { _logger.LogError($"[UI] Falló GET brands: {(int)res.StatusCode}"); return new(); }
        return JsonSerializer.Deserialize<List<BrandDto>>(
            await res.Content.ReadAsByteArrayAsync(), JsonOpts) ?? new();
    }
    private async Task<List<CountryDto>> GetCountryAsync(string? query = null)
    {
        var url = string.IsNullOrWhiteSpace(query)
        ? "https://restcountries.com/v3.1/all?fields=name,flags,cca2"
        : $"https://restcountries.com/v3.1/name/{Uri.EscapeDataString(query)}?fields=name,flags,cca2";
        var cli = _http.CreateClient();
        var res = await cli.GetAsync(url);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError($"[UI] Falló GET países: {(int)res.StatusCode} {res.ReasonPhrase}");
            return new();
        }
        var bytes = await res.Content.ReadAsByteArrayAsync();
        var apiList = JsonSerializer.Deserialize<List<CountryApiModel>>(bytes, JsonOpts) ?? new();

        
        return apiList
            .Where(c => !string.IsNullOrWhiteSpace(c.Cca2) && !string.IsNullOrWhiteSpace(c?.Name?.Common))
            .Select(c => new CountryDto
            {
                Code = c.Cca2!,
                Name = c.Name!.Common!,
                FlagPng = c.Flags?.Png,
                FlagSvg = c.Flags?.Svg
            })
            .OrderBy(c => c.Name)
            .ToList();
    }
}
