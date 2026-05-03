using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.Services;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Services;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.ExecuteHandPicking
{
	public class ExecuteHandPickingHandler(IPalletRepo palletRepo,
		IVirtualPalletRepo virtualPalletRepo,
		WerehouseDbContext werehouseDbContext,
		IIssueRepo issueRepo,
		IProcessPickingActionService processPickingActionService,
		IPickingDomainService pickingDomainService
			) : IRequestHandler<ExecuteHandPickingCommand, AppResult<Unit>>
	{
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IProcessPickingActionService _processPickingActionService = processPickingActionService;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;

		public async Task<AppResult<Unit>> Handle(ExecuteHandPickingCommand command, CancellationToken ct)
		{
			if (command.Quanitity <= 0)
			{
				return AppResult<Unit>.Fail("Nie możesz pobrać ujemnej wartości.", ErrorType.Conflict);
			}
			var issue = await _issueRepo.GetIssueByIdAsync(command.IssueId);
			if (issue == null)
			{
				return AppResult<Unit>.Fail($"Zamówienie o numerze {command.IssueId} nie zostało znalezione.", ErrorType.NotFound);
			}
			var pallet = await _palletRepo.GetPalletByIdAsync(command.PalletIdSource);//ustalić warunek biznesowy, wskazuje biuro które palety pobrać - emergency
			if (pallet == null)
			{
				return AppResult<Unit>.Fail($"Paleta o numerze {command.PalletIdSource} nie istnieje.", ErrorType.NotFound);
			}
			if (pallet.ProductsOnPallet.Count > 1)
			{
				return AppResult<Unit>.Fail("Zadania nie można zrealizować, paleta nie nadaje się do pobrań.", ErrorType.Conflict);
			}
			var product = pallet.ProductsOnPallet.FirstOrDefault();//
			if (product == null || product.Quantity == 0)
			{
				return AppResult<Unit>.Fail($"Paleta {command.PalletIdSource} jest pusta.", ErrorType.Conflict);
			}
			var pickingHandTask = await _pickingDomainService.GetSingleHandPickingTask(command.IssueId, product.ProductId);
			
			if (command.Quanitity > (pickingHandTask.RequestedQuantity - pickingHandTask.PickedQuantity))
			{
				return AppResult<Unit>.Fail("Chcesz pobrać więcej niż potrzeba.", ErrorType.Conflict);
			}
			if (pickingHandTask.PickingStatus == PickingStatus.Picked)
			{
				return AppResult<Unit>.Fail("Zapotrzebowania na ten asortyment już zrealizowane", ErrorType.Conflict);
			}
			var virtualPallet = await _virtualPalletRepo.GetVirtualPalletByPalletIdAsync(command.PalletIdSource);
			if (virtualPallet == null)
			{
				virtualPallet = VirtualPallet.Create(pallet.Id, product.Quantity, pallet.LocationId);
				pallet.AssignToPicking(command.UserId, pallet.Location.ToSnapshot());
				_virtualPalletRepo.AddPalletToPicking(virtualPallet);
			}
			var availableQuantity = virtualPallet?.RemainingQuantity ?? product.Quantity;
			if (command.Quanitity > availableQuantity)
			{
				return AppResult<Unit>.Fail("Zadania nie można zrealizować, mniej dostępnego towaru na palecie niż chęć pobrania", ErrorType.Conflict);
			}
			pickingHandTask.SetVirtualPallet(virtualPallet.Id);
			var completion = PickingCompletion.Full;
			if (command.Quanitity < pickingHandTask.RequestedQuantity - pickingHandTask.PickedQuantity)
			{
				completion = PickingCompletion.Partial;
			}
			var resultProcessPicking = await _processPickingActionService.ProcessPicking(pallet, issue, product.ProductId, command.Quanitity, command.UserId, pickingHandTask, completion, command.NumberRamp);
			if (!resultProcessPicking.Success)
			{
				return AppResult<Unit>.Fail(resultProcessPicking.Message, ErrorType.Conflict);//
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<Unit>.Success(Unit.Value, "Towar dołączono do zlecenia");
		}
	}

}
