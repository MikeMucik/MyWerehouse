using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking
{
	public class ExecutiveReversePickingHandler(WerehouseDbContext werehouseDbContext,
		IReversePickingRepo reversePickingRepo,
		IAddProductsToPalletService addProductsToPalletService,
		IPalletRepo palletRepo,
		IProductRepo productRepo) : IRequestHandler<ExecutiveReversePickingCommand, AppResult<ReversePickingResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IAddProductsToPalletService _addProductsToPalletService = addProductsToPalletService;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IProductRepo _productRepo = productRepo;
		public async Task<AppResult<ReversePickingResult>> Handle(ExecutiveReversePickingCommand command, CancellationToken ct)
		{
			var result = new ReversePickingResult();
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(ct);
			try
			{
				//walidacja
				if (command.Strategy == ReversePickingStrategy.AddToExistingPallet)
				{
					if (command.Pallets.Count == 0)
					{
						return AppResult<ReversePickingResult>.Fail("Lista pusta palet do których możną dołączyć towar.");
					}
				}
				var reversePicking = await _reversePickingRepo.GetReversePickingAsync(command.TaskReversedId);
				if (reversePicking is null)
				{
					return AppResult<ReversePickingResult>.Fail("Brak zadania do dekompletacji", ErrorType.NotFound);
				}
				if (!await _productRepo.IsExistProduct(reversePicking.ProductId))
					return AppResult<ReversePickingResult>.Fail($"Produkt o numerze {reversePicking.ProductId} nie istnieje.", ErrorType.NotFound);
				var pickingPallet = await _palletRepo.GetPalletByIdAsync(command.PickingPalletId);
				if (pickingPallet == null)
				{
					return AppResult<ReversePickingResult>.Fail("Brak palety do dekompletacji", ErrorType.NotFound);
				}
				
				//Do zmiany
				var issueId = reversePicking.PickingTask.IssueId;
				var issueNumber = reversePicking.PickingTask.Issue.IssueNumber;
				if (issueId == Guid.Empty)
				{
					return AppResult<ReversePickingResult>.Fail($"Zamówienie o numerze {issueId} nie zostało znalezione.", ErrorType.NotFound);
				}
				//
				//produkt
				var productOnPallet = pickingPallet.ProductsOnPallet.SingleOrDefault(p => p.ProductId == reversePicking.ProductId);//
				reversePicking.ChangeStatus(ReversePickingStatus.InProgress);
				//string? sourcePalletId = null;
				//string? destinationPalletId = null;
				switch (command.Strategy)
				{
					case ReversePickingStrategy.ReturnToSource:
						result = _addProductsToPalletService.AddProductsToSourcePallet(reversePicking, command.UserId);
						if (!result.Success) return AppResult<ReversePickingResult>.Fail(result.Message, ErrorType.Conflict);
						var virtualPalletPickingTasks = reversePicking.PickingTask.VirtualPallet.PickingTasks;
						var palletFromSource = virtualPalletPickingTasks.First().VirtualPallet.Pallet;
						var hasAnyAllocated = virtualPalletPickingTasks.Any(t => t.PickingStatus == PickingStatus.Allocated);
						if (!hasAnyAllocated)
						{
							palletFromSource.ChangeStatus(PalletStatus.Available);
						}
						break;
					case ReversePickingStrategy.AddToExistingPallet:
						if (command.Pallets.Count == 0)
						{
							return AppResult<ReversePickingResult>.Fail("Brak palet do których można dodać towar.", ErrorType.NotFound);
						}
						result = await _addProductsToPalletService.AddToExistingPallet(reversePicking, command.Pallets, command.UserId);

						if (!result.Success) return AppResult<ReversePickingResult>.Fail(result.Message, ErrorType.Conflict);
						//TODO front co potrzebuje						
						break;
					case ReversePickingStrategy.AddToNewPallet:
						if (command.RampNumber == null) return AppResult<ReversePickingResult>.Fail("Brak lokalizacji dekompletacji", ErrorType.Validation);
						result = await _addProductsToPalletService.AddToNewPallet(reversePicking, command.UserId, command.RampNumber.Value);
						break;

						//default
				}
				//productOnPallet.Quantity = 0;//
				productOnPallet.SetQuantity(0);
				if (pickingPallet.ProductsOnPallet.All(x => x.Quantity == 0))
				{
					pickingPallet.ToArchive(command.UserId, ReasonMovement.ReversePicking, pickingPallet.Location.ToSnopShot());
				}
				else
				{
					pickingPallet.ChangeStatus(PalletStatus.ReversePicking);//do przemyślenia
				}
				reversePicking.ChangeStatus(ReversePickingStatus.Completed);
				reversePicking.AddHistory(pickingPallet.Id, command.UserId, issueId, issueNumber, ReversePickingStatus.InProgress, ReversePickingStatus.Completed);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);
				return AppResult<ReversePickingResult>.Success(result);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync(ct);
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas wykonywania dekompletacji.", ex);
			}
		}
	}
}
