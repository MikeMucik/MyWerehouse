using Microsoft.AspNetCore.Mvc;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/health")]
	public class ApiHealthController : ControllerBase
	{
		[HttpGet]
		public IActionResult Health()
		{
			return Ok();
		}
	}
}
