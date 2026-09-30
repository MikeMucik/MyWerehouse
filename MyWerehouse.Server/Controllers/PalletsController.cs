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
		// Create a pallet
		[HttpPost]
		public async Task<IActionResult> Create(CreatePalletCommand command, CancellationToken ct)
		{
			var result = await _mediator.Send(command, ct);
			return result.ToActionResult();
		}
		// Pallet data: Guid
		[HttpGet("{id:guid}")]
		public async Task<IActionResult> Get(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletQuery(id), ct)).ToActionResult();

		// Pallet data: PalletNumber
		[HttpGet("by-number/{palletNumber}")]
		public async Task<IActionResult> GetByPalletNumber(string palletNumber, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletByPalletNumberQuery(palletNumber), ct)).ToActionResult();

		// Pallet to edit
		[HttpGet("{id:guid}/edit")]
		public async Task<IActionResult> GetForEdit(Guid id, CancellationToken ct)
			=> (await _mediator.Send(new GetPalletToEditQuery(id), ct)).ToActionResult();

		// Update the pallet
		[HttpPut("{id:guid}")]
		public async Task<IActionResult> Update(Guid id, Application.Pallets.Commands.UpdatePallet.UpdatePalletDTO dto, CancellationToken ct)
			=> (await _mediator.Send(new UpdatePalletCommand(id, dto), ct)).ToActionResult();

		// Change the location
		[HttpPost("{id:guid}/change-location")]
		public async Task<IActionResult> ChangeLocation(Guid id, int destinationLocation, string userId, bool forced, CancellationToken ct)
			=> (await _mediator.Send(new ChangeLocationPalletCommand(id, destinationLocation, userId, forced), ct))
			.ToActionResult();

		// Mark as loaded; consider switching to an ID as well
		[HttpPost("{id:guid}/mark-loaded")]
		public async Task<IActionResult> MarkLoaded(Guid id, string userId, CancellationToken ct)
			=> (await _mediator.Send(new MarkAsLoadedCommand(id, userId), ct))
			.ToActionResult();

		// Filter / list
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] FindPalletsByFilterQuery query, CancellationToken ct)
			=> (await _mediator.Send(query, ct)).ToActionResult();
	}
}
