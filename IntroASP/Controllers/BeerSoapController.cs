using BeerSoap;
using Microsoft.AspNetCore.Mvc;
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
    }
}
