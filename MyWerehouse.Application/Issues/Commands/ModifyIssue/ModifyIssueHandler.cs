using System.Data;
using System.Linq;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public class ModifyIssueHandler(
		IIssueRepo issueRepo,
		IMediator mediator,
		WerehouseDbContext werehouseDbContext,
		IAssignProductToIssueService assignProductToIssueAsync,
		IVirtualPalletRepo virtualPalletRepo,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<ModifyIssueCommand, AppResult<List<AssignProductToIssueResult>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMediator _mediator = mediator;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IAssignProductToIssueService _assignProductToIssueAsync = assignProductToIssueAsync;
		private readonly IVirtualPalletRepo _virtualRepo = virtualPalletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task<AppResult<List<AssignProductToIssueResult>>> Handle(ModifyIssueCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var resultList = new List<AssignProductToIssueResult>();
			var issue = await _issueRepo.GetIssueByIdForModifyAsync(request.Id);
			if (issue == null)
				return AppResult<List<AssignProductToIssueResult>>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);			
			var mode = issue.DetremineModificationMode();
			
			if (mode == IssueModificationMode.Reallocation)
			{
				return await ReallocateIssue(issue, request, now, ct);
			}
			if (mode == IssueModificationMode.SupplementaryIssue)
			{
				return await CreateSupplementaryIssue(issue, request, now, ct);
			}
			else
			{
				return AppResult<List<AssignProductToIssueResult>>.Fail($"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}", ErrorType.Conflict);
			}
		}

		private async Task<AppResult<List<AssignProductToIssueResult>>> ReallocateIssue(Issue issue, ModifyIssueCommand request, DateTime now, CancellationToken ct)
		{
			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
			var oldPallets = issue.PrepareForReallocation(request.DTO.ClientId, request.DTO.PerformedBy, now);
			await _werehouseDbContext.SaveChangesAsync(ct);

			var anyFailure = false;
			var anySuccess = false;
			var resultList = new List<AssignProductToIssueResult>();
			foreach (var product in request.DTO.IssueItems)
			{
				var reusablePalletsForProduct = oldPallets.Pallets.Where(p => p.ContainsProduct(product.ProductId)).ToList();
				var savePointName = $"BeforeProduct_{product.ProductId}_{Guid.NewGuid()}";
				await transaction.CreateSavepointAsync(savePointName, ct);
				try
				{
					var result = await _assignProductToIssueAsync.AssignProductToIssue(issue, product,
						IssueAllocationPolicy.FullPalletFirst, reusablePalletsForProduct, request.DTO.PerformedBy);

					if (!result.Success) //niepowodzenie biznesowe
					{
						await transaction.RollbackToSavepointAsync(savePointName, ct);
						await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
						await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
						await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);

						resultList.Add(result);
						anyFailure = true;
						continue;
					}
					var palletAssigned = result.AssignedPallets.ToList();
					issue.CompleteReallocation(palletAssigned, reusablePalletsForProduct);
					anySuccess = true;
					resultList.Add(result);
				}

				catch (DomainException ex)
				{
					await transaction.RollbackToSavepointAsync(savePointName, ct);
					await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
					await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
					await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);

					resultList.Add(AssignProductToIssueResult.Fail($"Wystąpił błąd {ex.Message}", product.ProductId));
					anyFailure = true;
				}
			}
			if (oldPallets.ListPalletsIds.Count != 0)
			{
				// Usuwamy tylko puste VirtualPallets; fizyczne palety wracają do dostępnych.
				foreach (var item in oldPallets.ListPalletsIds)
				{
					var vp = await _virtualRepo.GetVirtualPalletByIdAsync(item);
					if (vp!.CanBeDeletedAfterReallocation())
					{
						vp.Pallet?.ChangeStatus(PalletStatus.Available);
						_virtualRepo.DeleteVirtualPalletPicking(vp);
					}
				}
			}
			if (anySuccess)
			{
				issue.MarkAllocationCompleted(request.DTO.PerformedBy);
			}
			if (anyFailure)
			{
				issue.MarkAllocationNotCompleted(issue.PerformedBy);
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			await transaction.CommitAsync(ct);
			return AppResult<List<AssignProductToIssueResult>>.Success(resultList);
		}

		private async Task<AppResult<List<AssignProductToIssueResult>>> CreateSupplementaryIssue(Issue issue, ModifyIssueCommand request, DateTime now, CancellationToken ct)
		{
			var newQuantities = new List<IssueItemDTO>();
			var hasNegativeDiff = false;
			var errorMessage = new List<string>();
			
			foreach (var product in request.DTO.IssueItems)
			{
				var productId = product.ProductId;
				var oldQuantity = issue.GetQuantityForProduct(productId);

				var newQuantity = product.Quantity - oldQuantity;
				if (newQuantity < 0)
				{
					hasNegativeDiff = true;
					errorMessage.Add($"Produkt {productId}: Nie można zmniejszyć z {oldQuantity} do {product.Quantity} (różnica : {newQuantity}). Zlecenie jest już zatwierdzone do załadunku");
					continue;
				}
				if (newQuantity > 0)
				{
					var newItem = new IssueItemDTO
					{
						ProductId = productId,
						Quantity = newQuantity,
						BestBefore = product.BestBefore
					};
					newQuantities.Add(newItem);
				}
			}
			if (hasNegativeDiff)
			{
				return AppResult<List<AssignProductToIssueResult>>.Fail(
					  string.Join(";", errorMessage),
				   ErrorType.Conflict
			   );
			}
			if (newQuantities.Count == 0)
			{
				var resultListNoQuantitesChange = new List<AssignProductToIssueResult>
					{
						AssignProductToIssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.")
					};
				return AppResult<List<AssignProductToIssueResult>>.Success(resultListNoQuantitesChange);
			}
			var dataForNewIssue = new CreateIssueDTO
			{
				ClientId = request.DTO.ClientId,
				Items = newQuantities,
				PerformedBy = request.DTO.PerformedBy,
			};
			var receiverFromCreate = await _mediator.Send(new CreateIssueCommand(dataForNewIssue, request.DateToSend), ct);
			if (receiverFromCreate is null || receiverFromCreate.IsSuccess is false || receiverFromCreate.Result is null)
				return AppResult<List<AssignProductToIssueResult>>.Fail("Nie udało się utworzyć nowego zlecenia.", ErrorType.Conflict);
		var	resultList = receiverFromCreate.Result;
			foreach (var result in resultList)
			{
				if (result.Success)
				{
					result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
				}
			}
			return AppResult<List<AssignProductToIssueResult>>.Success(resultList);
		}
	}
}
