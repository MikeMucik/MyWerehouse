using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/locations")]
	public class LocationsController : ControllerBase
	{
		private readonly ILocationService _locationService;
		public LocationsController(ILocationService locationService)
		{
			_locationService = locationService;
		}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> Get(int id)
			=> (await _locationService.GetLocationServiceAsync(id))
			.ToActionResult();

		[HttpPost]
		public async Task<IActionResult> Create(LocationDTO locationDto)
			=> (await _locationService.AddLocationServiceAsync(locationDto))
			.ToActionResult();

		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
			=> (await _locationService.DeleteLocationServiceAsync(id))
			.ToActionResult();

		[HttpPost("bulk")]//zatwierdzenie prepare
		public async Task<IActionResult> Bulk(List<LocationDTO> locations)
			=> (await _locationService.CreateManyLocation(locations))
			.ToActionResult();

		[HttpPost("preview")] //ile regałów alejek etc
		public IActionResult Preview(int bay, int startAisle, int endAisle, int amountPosition, int numberOfLevels)
			=> (_locationService.PrepareLocations(bay, startAisle, endAisle, amountPosition, numberOfLevels))
			.ToActionResult();

		[HttpGet("search")]
		public async Task<IActionResult> Search(int bay, int aisle, int position, int height)
			=> (await _locationService.FindLocationAsync(bay, aisle, position, height))
			.ToActionResult();
	}
}
