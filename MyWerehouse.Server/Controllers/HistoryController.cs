using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Interfaces;

namespace MyWerehouse.Server.Controllers
{	
	[ApiController]
	[Route("api/[controller]")]
	public class HistoryController : ControllerBase
	{
		private readonly IHistoryService _historyService;
		public HistoryController(IHistoryService historyService)
		{
			_historyService = historyService;
		}
		[HttpGet("Pallet")]
		public async Task<IActionResult> Pallet(Guid id) //a może jedna string
		{
			var result = await _historyService.GetHistoryPalletByIdAsync(id);
			return Ok(result);
		}
		//TODO
	}
}
