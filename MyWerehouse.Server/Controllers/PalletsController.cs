using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFiltr;
using MyWerehouse.Application.Pallets.Queries.GetPalletToEdit;
using MyWerehouse.Server.Extensions;

namespace MyWerehouse.Server.Controllers
{
	[ApiController]
	[Route("api/pallets")]
	public class PalletsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public PalletsController(IMediator mediator)
		{
			_mediator = mediator;
		}
		// stworzenie palety
		[HttpPost("add")]
		public async Task<IActionResult> Create(CreateNewPalletCommand command)
		{
			var result = await _mediator.Send(command);
			return result.ToActionResult();

			//if (!result.IsSuccess)
			//{
			//	return result.ErrorType switch
			//	{
			//		ErrorType.NotFound => NotFound(result.Error),
			//		ErrorType.Conflict => Conflict(result.Error),
			//		ErrorType.Validation => BadRequest(result.Error),
			//		_ => BadRequest(result.Error)
			//	};
			//}
			//return result.ToActionResult();
			//return Ok(result.Value);
		}
		// paleta do edycji
		[HttpGet("{id}")]
		public async Task<IActionResult> GetForEdit(Guid id)
			=> (await _mediator.Send(new GetPalletToEditQuery(id))).ToActionResult();		

		// update palety
		[HttpPut("update")]
		public async Task<IActionResult> Update(UpdatePalletCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//// anulowanie/usunięcie
		//[HttpDelete("{id}")]
		//public async Task<IActionResult> Cancel(Guid id)
		//	=> (await _mediator.Send(new CancelPalletCommand(id))).ToActionResult();

		// zmiana lokacji
		[HttpPost("chnageLocation")]
		public async Task<IActionResult> ChangeLocation(ChangeLocationPalletCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		// oznacz jako załadowana
		[HttpPost("markLoaded")]
		public async Task<IActionResult> MarkLoaded(MarkAsLoadedCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		// filtr / lista
		[HttpGet("byFilter")]
		public async Task<IActionResult> Find([FromQuery] FindPalletsByFiltrQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
