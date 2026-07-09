using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.ReversePickings.Command.ExecutiveReversePicking
{
	public class ExecuteReversePickingHandler(WerehouseDbContext werehouseDbContext,
		IReversePickingRepo reversePickingRepo,
		IAddProductsToPalletService addProductsToPalletService,
		IPalletRepo palletRepo,
		ILocationRepo locationRepo
		) : IRequestHandler<ExecuteReversePickingCommand, AppResult<ReversePickingResult>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IAddProductsToPalletService _addProductsToPalletService = addProductsToPalletService;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly ILocationRepo _locationRepo = locationRepo;
		public async Task<AppResult<ReversePickingResult>> Handle(ExecuteReversePickingCommand command, CancellationToken ct)
		{			
			var reversePicking = await _reversePickingRepo.GetReversePickingAsync(command.TaskReversedId);
			if (reversePicking is null)
			{
				return AppResult<ReversePickingResult>.Fail("Brak zadania do dekompletacji", ErrorType.NotFound);
			}
			var pickingPallet = await _palletRepo.GetPalletByIdAsync(command.PickingPalletId);
			if (pickingPallet == null)
			{
				return AppResult<ReversePickingResult>.Fail("Brak palety do dekompletacji", ErrorType.NotFound);
			}
			if (reversePicking?.PickingTask?.Issue == null)
			{
				return AppResult<ReversePickingResult>.Fail("Nie załadowano pełnych danych", ErrorType.NotFound);
			}
			var issueId = reversePicking.PickingTask.IssueId;
			var issueNumber = reversePicking.PickingTask.Issue.IssueNumber;
			if (issueId == Guid.Empty)
			{
				return AppResult<ReversePickingResult>.Fail($"Zamówienie o numerze {issueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			//produkt na palecie kompletacyjnej - product on pickingPallet
			var productOnPallet = pickingPallet.GetProductAggregate(reversePicking.ProductId);

			reversePicking.ChangeStatus(ReversePickingStatus.InProgress);
			ReversePickingResult result;
			static AppResult<ReversePickingResult> Fail(string message)
			=> AppResult<ReversePickingResult>.Fail(message, ErrorType.Conflict);
			switch (command.Strategy)
			{
				case ReversePickingStrategy.ReturnToSource:
					result = await _addProductsToPalletService.AddProductsToSourcePallet(reversePicking, command.UserId);
					if (!result.Success) return Fail(result.Message);
					break;
				case ReversePickingStrategy.AddToExistingPallet:
					if (command.Pallets == null || command.Pallets.Count == 0)
					{
						return AppResult<ReversePickingResult>.Fail("Lista pusta palet do których możną dołączyć towar.", ErrorType.NotFound);
					}
					result = await _addProductsToPalletService.AddToExistingPallet(reversePicking, command.Pallets, command.UserId);
					if (!result.Success) return Fail(result.Message);
					//TODO front co potrzebuje						
					break;
				case ReversePickingStrategy.AddToNewPallet:
					
					if (command.RampNumber == null)
					{
						return AppResult<ReversePickingResult>.Fail("Brak lokalizacji dekompletacji", ErrorType.Validation);
					}
					var location =await _locationRepo.GetLocationByIdAsync(command.RampNumber.Value);
					if (location == null)
					{
						return AppResult<ReversePickingResult>.Fail("Podana lokalizacja jest nieprawidłowa.", ErrorType.Validation);
					}
					var snapShot = location.ToSnapshot();
					result = await _addProductsToPalletService.AddToNewPallet(reversePicking, command.UserId, command.RampNumber!.Value, snapShot);
					if (!result.Success) return Fail(result.Message);
					break;
				default:
					return AppResult<ReversePickingResult>.Fail($"Nieobsługiwana strategia: {command.Strategy}");					
			}
			//paleta dekompletowana
			productOnPallet.SetQuantity(0);
			pickingPallet.CkeckIfToArchive(command.UserId, ReasonForPallet.ReversePicking, pickingPallet.Location.ToSnapshot());
			//zadanie dekompletacyjne
			reversePicking.ChangeStatus(ReversePickingStatus.Completed);
			reversePicking.AddHistory(pickingPallet.Id, command.UserId, issueId, issueNumber, ReversePickingStatus.InProgress, ReversePickingStatus.Completed);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<ReversePickingResult>.Success(result);
		}		
	}
}