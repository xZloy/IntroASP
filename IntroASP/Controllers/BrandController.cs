using IntroASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace IntroASP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly PubContext _ctx;
        public BrandController(PubContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<IEnumerable<object>> Get() =>
            await _ctx.Brands.AsNoTracking()
                .Select(b => new { b.BrandId, b.Name })
                .ToListAsync();
    }

}
