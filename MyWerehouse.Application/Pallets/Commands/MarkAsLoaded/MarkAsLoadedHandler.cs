using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.MarkAsLoaded
{
	public class MarkAsLoadedHandler :IRequestHandler<MarkAsLoadedCommand, IssueResult>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IMediator _mediator;
		public MarkAsLoadedHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_mediator = mediator;
		}
		public async Task<IssueResult> Handle(MarkAsLoadedCommand request, CancellationToken ct)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				var pallet = await _palletRepo.GetPalletByIdAsync(request.PalletId) ??
				throw new PalletException($"Paleta {request.PalletId} nie istnieje");
				if (pallet.Status == PalletStatus.Loaded)
					throw new PalletException($"Paleta {request.PalletId} jest już załadowana.");

				//if (!(pallet.Status == PalletStatus.ToIssue || pallet.Status == PalletStatus.InTransit || pallet.Status == PalletStatus.Available ||
				//	pallet.Status == PalletStatus.InStock)) throw new PalletException("Paleta nie ma statusu do załadowania");
				var allowedStatuses = new[]
				{
					PalletStatus.ToIssue,
					PalletStatus.InTransit,
					PalletStatus.Available,
					PalletStatus.InStock
				};
				if (!allowedStatuses.Contains(pallet.Status))
					throw new PalletException("Paleta nie ma statusu do załadowania");

				pallet.Status = PalletStatus.Loaded;
				
				await _mediator.Publish(new CreatePalletOperationNotification(pallet.Id, pallet.LocationId,
						ReasonMovement.Loaded, request.UserId, PalletStatus.Loaded, null), ct);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return IssueResult.Ok($"Paleta {request.PalletId} została załadowana.");
			}
			catch(PalletException ep)
			{
				await transaction.RollbackAsync(ct);
				return IssueResult.Fail(ep.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				return IssueResult.Fail("Wystąpił nieoczekiwany błąd podczas operacji.");
			}
		}
	}
}
