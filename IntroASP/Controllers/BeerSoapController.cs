using BeerSoap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ServiceModel;
using System.Threading.Tasks;

namespace IntroASP.Controllers
{
    public class BeerSoapController : Controller
    {
        private async Task<List<SelectListItem>> LoadBrandItemsAsync(int? selectedBrandId = null)
        {
            var client = new BeerServiceClient();
            var brands = await client.GetBrandsAsync();
            await client.CloseAsync();

            var items = brands
                .Select(b => new SelectListItem
                {
                    Value = b.BrandId.ToString(),
                    Text = b.Name,
                    Selected = selectedBrandId.HasValue && b.BrandId == selectedBrandId.Value
                })
                .ToList();

            return items;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Brands = await LoadBrandItemsAsync(); 
            return View(new BeerSoap.BeerCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(BeerSoap.BeerCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Brands = await LoadBrandItemsAsync(model.BrandId);
                return View(model);
            }
            try
            {
                var client = new BeerServiceClient();
                await client.CreateBeerAsync(model);
                await client.CloseAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (FaultException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Brands = await LoadBrandItemsAsync(model.BrandId);
                return View(model);
            }
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
            var dto = await client.GetBeerForEditAsync(id); //con brandId
            await client.CloseAsync();

            if (dto == null) return NotFound();

            ViewBag.Brands = await LoadBrandItemsAsync(dto.BrandId);
            return View(dto); 
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BeerSoap.BeerUpdateDto model)
        {
            if (!ModelState.IsValid)
            {

                ViewBag.Brands = await LoadBrandItemsAsync(model.BrandId);
                return View(model);
            }

            try
            {
                var client = new BeerServiceClient();

                // Asegura que envías el tipo del proxy
                var dto = new BeerSoap.BeerUpdateDto
                {
                    Id = model.Id,
                    Name = model.Name,
                    BrandId = model.BrandId,
                    CountryCode = model.CountryCode,
                    FlagUrl = model.FlagUrl
                };

                await client.UpdateBeerAsync(dto);
                await client.CloseAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (FaultException ex)
            {
                // Mensaje que envía el WCF (por ejemplo BrandId no existe)
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.Brands = await LoadBrandItemsAsync(model.BrandId);
                return View(model);
            }
        }



    }
}
