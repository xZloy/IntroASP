using System.ComponentModel.DataAnnotations;
using IntroASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public record BeerReadDto(
    int BeerId,
    string Name,
    int BrandId,
    string? CountryCode,
    string? FlagUrl
);

public class BeerCreateDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = default!;

    [Range(1, int.MaxValue)]
    public int BrandId { get; set; }

    [StringLength(5)]
    public string? CountryCode { get; set; }   

    [StringLength(300)]
    public string? FlagUrl { get; set; }       
}

public class BeerUpdateDto : BeerCreateDto { }

[ApiController]
[Route("api/[controller]")] 
public class BeersController : ControllerBase
{
    private readonly PubContext _ctx;
    private readonly ILogger<BeersController> _logger;

    public BeersController(PubContext ctx, ILogger<BeersController> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BeerReadDto>>> GetAll(CancellationToken ct)
    {
        _logger.LogInformation($"[API] GET /api/beers solicitado a las {DateTime.Now:HH:mm:ss}");

        var list = await _ctx.Beers
            .AsNoTracking()
            .OrderBy(b => b.BeerId)
            .Select(b => new BeerReadDto(
                b.BeerId,
                b.Name,
                b.BrandId,
                b.CountryCode,   
                b.FlagUrl        
            ))
            .ToListAsync(ct);

        _logger.LogInformation($"[API] Se retornaron {list.Count} beers");
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BeerReadDto>> Get(int id, CancellationToken ct)
    {
        _logger.LogInformation($"[API] GET /api/beers/{id}");

        var b = await _ctx.Beers
            .AsNoTracking()
            .Where(x => x.BeerId == id)
            .Select(x => new BeerReadDto(
                x.BeerId,
                x.Name,
                x.BrandId,
                x.CountryCode,   
                x.FlagUrl        
            ))
            .FirstOrDefaultAsync(ct);

        if (b is null) { _logger.LogWarning($"[API] No encontrada Id={id}"); return NotFound(); }
        return Ok(b);
    }

    [HttpPost]
    public async Task<ActionResult<BeerReadDto>> Create([FromBody] BeerCreateDto input, CancellationToken ct)
    {
        // Validación de marca existente
        var brandExists = await _ctx.Brands.AsNoTracking()
            .AnyAsync(br => br.BrandId == input.BrandId, ct);
        if (!brandExists)
            return BadRequest(Problem(detail: $"BrandId {input.BrandId} no existe.", statusCode: 400));

        var entity = new Beer
        {
            Name = input.Name,
            BrandId = input.BrandId,
            CountryCode = input.CountryCode, 
            FlagUrl = input.FlagUrl      
        };

        _logger.LogInformation($"[API] Creando beer: Name='{input.Name}', BrandId={input.BrandId}, Country='{input.CountryCode}'");
        _ctx.Beers.Add(entity);

        try
        {
            await _ctx.SaveChangesAsync(ct);
            _logger.LogInformation($"[API] Beer creada con Id={entity.BeerId}");
        }
        catch (DbUpdateException ex)
        {
            return Problem(title: "Error al guardar", detail: ex.Message, statusCode: 500);
        }

        var read = new BeerReadDto(entity.BeerId, entity.Name, entity.BrandId, entity.CountryCode, entity.FlagUrl);
        return CreatedAtAction(nameof(Get), new { id = read.BeerId }, read);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BeerUpdateDto input, CancellationToken ct)
    {
        _logger.LogInformation($"[API] Actualizando beer Id={id}");

        var entity = await _ctx.Beers.FindAsync(new object?[] { id }, ct); // tracked
        if (entity is null) { _logger.LogWarning($"[API] No encontrada Id={id}"); return NotFound(); }

        // Validación de marca existente
        var brandExists = await _ctx.Brands.AsNoTracking()
            .AnyAsync(br => br.BrandId == input.BrandId, ct);
        if (!brandExists)
            return BadRequest(Problem(detail: $"BrandId {input.BrandId} no existe.", statusCode: 400));

        entity.Name = input.Name;
        entity.BrandId = input.BrandId;
        entity.CountryCode = input.CountryCode; 
        entity.FlagUrl = input.FlagUrl;     

        try
        {
            await _ctx.SaveChangesAsync(ct);
            _logger.LogInformation($"[API] Beer Id={id} actualizada");
        }
        catch (DbUpdateException ex)
        {
            return Problem(title: "Error al actualizar", detail: ex.Message, statusCode: 500);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        _logger.LogInformation($"[API] Eliminando beer Id={id}");

        var entity = await _ctx.Beers.FindAsync(new object?[] { id }, ct);
        if (entity is null) { _logger.LogWarning($"[API] No encontrada Id={id}"); return NotFound(); }

        _ctx.Beers.Remove(entity);
        try
        {
            await _ctx.SaveChangesAsync(ct);
            _logger.LogInformation($"[API] Beer Id={id} eliminada");
        }
        catch (DbUpdateException ex)
        {
            return Problem(title: "Error al eliminar", detail: ex.Message, statusCode: 500);
        }

        return NoContent();
    }
}
