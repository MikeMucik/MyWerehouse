using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Services;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.ReversePickings.Models;
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
				return AppResult<ReversePickingResult>.Fail("Reverse picking task was not found.");
			}
			var pickingPallet = await _palletRepo.GetPalletByIdAsync(command.PickingPalletId);
			if (pickingPallet == null)
			{
				return AppResult<ReversePickingResult>.Fail("Pallet for reverse picking was not found.");
			}
			if (reversePicking?.PickingTask?.Issue == null)
			{
				return AppResult<ReversePickingResult>.Fail("Required data was not loaded.");
			}
			var issueId = reversePicking.PickingTask.IssueId;
			var issueNumber = reversePicking.PickingTask.Issue.IssueNumber;
			if (issueId == Guid.Empty)
			{
				return AppResult<ReversePickingResult>.Fail($"Issue {issueId} was not found.");
			}
			//produkt na palecie kompletacyjnej - product on pickingPallet
			var productOnPallet = pickingPallet.GetProductOnPalletForReverse(reversePicking.ProductId, reversePicking.BestBefore);
			reversePicking.Start();
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
					if (command.PalletsIds == null || command.PalletsIds.Count == 0)
					{
						return AppResult<ReversePickingResult>.Fail("No pallets were provided for receiving the product.");
					}
					result = await _addProductsToPalletService.AddToExistingPallet(reversePicking, command.PalletsIds, command.UserId);
					if (!result.Success) return Fail(result.Message);
										
					break;
				case ReversePickingStrategy.AddToNewPallet:
					
					if (command.RampNumber == null)
					{
						return AppResult<ReversePickingResult>.Fail("Reverse picking location was not provided.", ErrorType.Validation);
					}
					var location =await _locationRepo.GetLocationByIdAsync(command.RampNumber.Value);
					if (location == null)
					{
						return AppResult<ReversePickingResult>.Fail("The specified location is invalid.");
					}
					var snapShot = location.ToSnapshot();
					result = await _addProductsToPalletService.AddToNewPallet(reversePicking, command.UserId, command.RampNumber!.Value, snapShot);
					if (!result.Success) return Fail(result.Message);
					break;
				default:
					return AppResult<ReversePickingResult>.Fail($"Unsupported strategy: {command.Strategy}.", ErrorType.Conflict);
			}
			//paleta dekompletowana
			productOnPallet.DecreaseQuantity(reversePicking.Quantity);
			pickingPallet.CkeckIfToArchive(command.UserId, ReasonForPallet.ReversePicking, pickingPallet.Location.ToSnapshot());
			//zadanie dekompletacyjne
			reversePicking.Complete();
			reversePicking.AddHistory(command.UserId, issueId, issueNumber, ReversePickingStatus.InProgress, ReversePickingStatus.Completed);
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<ReversePickingResult>.Success(result);
		}		
	}
}
