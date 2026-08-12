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
		public async Task<IActionResult> Create(CreatePalletCommand command, CancellationToken ct)
		{
			var result = await _mediator.Send(command, ct);
			return result.ToActionResult();
		}
		// dane palety Guid
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletQuery(id), ct)).ToActionResult();

		// dane palety Palletnumber
		[HttpGet("by-number/{palletNumber}")]
		public async Task<IActionResult> GetByPalletNumber(string palletNumber, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletByPalletNumberQuery(palletNumber), ct)).ToActionResult();

		// paleta do edycji
		[HttpGet("{id:guid}/edit")]
		public async Task<IActionResult> GetForEdit(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletToEditQuery(id), ct)).ToActionResult();

		// update palety
		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, Application.Pallets.Commands.UpdatePallet.EditPalletDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new UpdatePalletCommand(id, dto), ct)).ToActionResult();

		// zmiana lokacji
		[HttpPost("{id:guid}/change-location")]
		public async Task<IActionResult> ChangeLocation(Guid id, int destinationLocation, string userId, bool forced, CancellationToken ct)
			=> (await _mediator.Send(new ChangeLocationPalletCommand(id, destinationLocation, userId, forced), ct))
			.ToActionResult();

		// oznacz jako załadowana i być może też zmień na id
		[HttpPost("{id:guid}/mark-loaded")]
		public async Task<IActionResult> MarkLoaded(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new MarkAsLoadedCommand(id, userId), ct))
			.ToActionResult();

		// filtr / lista
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] FindPalletsByFilterQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();
	}
}
