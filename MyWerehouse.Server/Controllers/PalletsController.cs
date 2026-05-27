using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.DTOs;
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
		}
		// paleta do edycji
		[HttpGet("{id}toEdit")]
		public async Task<IActionResult> GetForEdit(Guid id)
			=> (await _mediator.Send(new GetPalletToEditQuery(id))).ToActionResult();		

		// update palety
		[HttpPut("{id}update")]
		public async Task<IActionResult> Update(Guid id, EditPalletDTO dto)
			=> (await _mediator.Send(new UpdatePalletCommand(id, dto))).ToActionResult();

		// zmiana lokacji
		[HttpPost("changeLocation")]
		public async Task<IActionResult> ChangeLocation(ChangeLocationPalletCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		// oznacz jako załadowana i być może też zmień na id
		[HttpPost("markLoaded")]
		public async Task<IActionResult> MarkLoaded(MarkAsLoadedCommand command)
			=> (await _mediator.Send(command)).ToActionResult();

		// filtr / lista
		[HttpGet("byFilter")]
		public async Task<IActionResult> Find([FromQuery] FindPalletsByFiltrQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
