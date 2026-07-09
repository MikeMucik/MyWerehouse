using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Commands.ChangeLocationPallet;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.Commands.MarkAsLoaded;
using MyWerehouse.Application.Pallets.Commands.UpdatePallet;
using MyWerehouse.Application.Pallets.Queries.FindPalletsByFilter;
using MyWerehouse.Application.Pallets.Queries.GetPallet;
using MyWerehouse.Application.Pallets.Queries.GetPalletByPalletNumber;
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
		[HttpPost]
		public async Task<IActionResult> Create(CreatePalletCommand command)
		{
			var result = await _mediator.Send(command);
			return result.ToActionResult();
		}
		// dane palety Guid
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id)
			=> (await _mediator.Send(new GetPalletQuery(id))).ToActionResult();
		
		// dane palety Palletnumber
		[HttpGet("by-number/{palletNumber}")]
		public async Task<IActionResult> GetByPalletNumber(string palletNumber)
			=> (await _mediator.Send(new GetPalletByPalletNumberQuery(palletNumber))).ToActionResult();
		
		// paleta do edycji
		[HttpGet("{id:guid}/edit")]
		public async Task<IActionResult> GetForEdit(Guid id)
			=> (await _mediator.Send(new GetPalletToEditQuery(id))).ToActionResult();		

		// update palety
		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, Application.Pallets.Commands.UpdatePallet.EditPalletDTO dto)
			=> (await _mediator.Send(new UpdatePalletCommand(id, dto))).ToActionResult();

		// zmiana lokacji
		[HttpPost("{id:guid}/change-location")]
		public async Task<IActionResult> ChangeLocation(Guid id, int destinationLocation, string userId, bool forced)
			=> (await _mediator.Send(new ChangeLocationPalletCommand(id, destinationLocation, userId, forced)))
			.ToActionResult();

		// oznacz jako załadowana i być może też zmień na id
		[HttpPost("{id:guid}/mark-loaded")]
		public async Task<IActionResult> MarkLoaded(Guid id, string userId)
			=> (await _mediator.Send(new MarkAsLoadedCommand(id, userId)))
			.ToActionResult();

		// filtr / lista
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] FindPalletsByFilterQuery query)
			=> (await _mediator.Send(query)).ToActionResult();
	}
}
