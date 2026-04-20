using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking;
using MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.GetReversePickingToDo;
using MyWerehouse.Application.ReversePickings.Queries.ListPalletsToReservePicking;
using MyWerehouse.Server.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyWerehouse.Server.Controllers
{
	
	[ApiController]
	[Route("api/reversePicking")]
	public class ReversePickingController : ControllerBase
	{
		private readonly IMediator _mediator;
		public ReversePickingController(IMediator mediator)
		{
			_mediator = mediator;
		}

		//Wykonaj dekompletacje
		[HttpPost("execute")]
		public async Task<IActionResult> Disassembly(ExecutiveReversePickingCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		//Pokaż zadania dekompletacyjne listę
		[HttpGet("list")]
		public async Task<IActionResult> Tasks ([FromQuery] GetListReversePickingToDoQuery query)
			=> (await _mediator.Send(query)).ToActionResult();

		//Pokaż zadanie z możliwymi opcjami dekompletacji
		[HttpGet("{id}")]
		public async Task<IActionResult> TaskOption(Guid id)
			=> (await _mediator.Send(new GetReversePickingToDoQuery(id))).ToActionResult();

		//Lista palet do dekompletacji z lokalizacją
		[HttpGet("listPalletToUnpicking")]
		public async Task<IActionResult> PalletsForReservePicking([FromQuery] ListPalletsToReservePickingQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
