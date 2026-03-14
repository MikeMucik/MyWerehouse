using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.LocationModels;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/location")]
	public class LocationController : ControllerBase
	{
		private readonly ILocationService _locationService;
		public LocationController(ILocationService locationService)
		{
			_locationService = locationService;
		}
		[HttpGet("{id}")]
		public async Task<IActionResult> Get(int id)
		{
			var result = await _locationService.GetLocationServiceAsync(id);
			return Ok(result);
		}
		[HttpPost]
		public async Task<IActionResult> Add(LocationDTO locationDTO)
		{
			var result = await _locationService.AddLocationServiceAsync(locationDTO);
			return Ok(result);
		}
		[HttpPost("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _locationService.DeleteLocationServiceAsync(id);
			return Ok(result);
		}
		[HttpPost("addMany")]//zatwierdzenie prepare
		public async Task<IActionResult> AddMany(List<LocationDTO> dTOs)
		{
			var result = await _locationService.CreateManyLocation(dTOs);
			return Ok(result);
		}
		[HttpPost("prepare")] //ile regałów alejek etc
		public  IActionResult PrepareLocation(int bay, int startAisle, int endAisle, int amountPosition, int amountHeigt)
		{
			var result = _locationService.PrepareLocationsAsync(bay, startAisle, endAisle, amountPosition, amountHeigt);
			return Ok(result);
		}
		[HttpGet]
		public async Task<IActionResult> GetByFilter(int bay, int aisle, int position, int height)
		{
			var result = await _locationService.FindLocationAsync(bay, aisle, position, height);
			return Ok(result);
		}
	}
}
