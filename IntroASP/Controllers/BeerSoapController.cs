using BeerSoap;
using Microsoft.AspNetCore.Mvc;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IntroASP.Controllers
{
    public class BeerSoapController : Controller
    {


        [HttpGet]
        public IActionResult Create()
        {
            return View(new BeerSoap.BeerCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BeerSoap.BeerCreateDto model)
        {
            if (!ModelState.IsValid) return View(model);

            var client = new BeerServiceClient(); // o ServiceClient
            await client.CreateBeerAsync(model);
            await client.CloseAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var client = new BeerServiceClient();
            var beers = await client.GetBeersAsync(); // BeerDto[]
            await client.CloseAsync();                
            return View(beers);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var client = new BeerServiceClient();
            await client.DeleteBeerAsync(id);
            await client.CloseAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = new BeerServiceClient();

            // trae todos los registros (no hay GetBeerById)
            var beers = await client.GetBeersAsync();
            await client.CloseAsync();

            var b = beers?.FirstOrDefault(x => x.Id == id);
            if (b == null) return NotFound();

            // Mapeamos a BeerUpdateDto 
            var model = new BeerSoap.BeerUpdateDto
            {
                Id = b.Id,
                Name = b.Name,
                CountryCode = b.CountryCode,
                FlagUrl = b.FlagUrl
                // BrandId se ingresa en el formulario
            };

            ViewBag.CurrentBrand = b.Brand; // para mostrar la marca actual
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BeerSoap.BeerUpdateDto model)
        {
            if (!ModelState.IsValid)
            {
                
                return View(model);
            }

            try
            {
                var client = new BeerSoap.BeerServiceClient();
                await client.UpdateBeerAsync(new BeerSoap.BeerUpdateDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    BrandId = model.BrandId,
                    CountryCode = model.CountryCode,
                    FlagUrl = model.FlagUrl
                });

                await client.CloseAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (FaultException ex)
            {
                // Mensaje que envía el WCF (por ejemplo BrandId no existe)
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }



    }
}
