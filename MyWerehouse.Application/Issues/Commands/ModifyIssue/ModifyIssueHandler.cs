using System.Data;
using System.Linq;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssueServices;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;

namespace MyWerehouse.Application.Issues.Commands.ModifyIssue
{
	public class ModifyIssueHandler(
		IIssueRepo issueRepo,
		IMediator mediator,
		IUnitOfWork unitOfWork,
		IAssignProductToIssueService assignProductToIssueAsync,
		IVirtualPalletRepo virtualPalletRepo,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<ModifyIssueCommand, AppResult<IssueCreateModifyResult>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMediator _mediator = mediator;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IAssignProductToIssueService _assignProductToIssueAsync = assignProductToIssueAsync;
		private readonly IVirtualPalletRepo _virtualRepo = virtualPalletRepo;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
		public async Task<AppResult<IssueCreateModifyResult>> Handle(ModifyIssueCommand request, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var issueCheck = await _issueRepo.GetIssueByIdAsync(request.Id, ct);
			if (issueCheck == null)
				return AppResult<IssueCreateModifyResult>.Fail("Issue was not found.");
			var mode = issueCheck.DetremineModificationMode();

			if (mode == IssueModificationMode.Reallocation)
			{
				return await ReallocateIssue(request, now, ct);
			}
			if (mode == IssueModificationMode.SupplementaryIssue)
			{
				return await CreateSupplementaryIssue(request, ct);
			}
			else
			{
				return AppResult<IssueCreateModifyResult>.Fail($"Issue {issueCheck.Id} cannot be updated in status {issueCheck.IssueStatus}.", ErrorType.Conflict);
			}
		}

		private async Task<AppResult<IssueCreateModifyResult>> ReallocateIssue(ModifyIssueCommand request, DateTime now, CancellationToken ct)
		{
			return await _unitOfWork.ExecuteInTransactionAsync(
				async transactionCt =>
				{
					var issue = await _issueRepo.GetIssueByIdForModifyAsync(request.Id, transactionCt);
					if (issue == null)
					{
						return AppResult<IssueCreateModifyResult>.Fail("Issue was not found.");
					}
					var oldPallets = issue.PrepareForReallocation(request.DTO.ClientId, request.DTO.PerformedBy, now);
					await _unitOfWork.SaveChangesAsync(transactionCt);
					var results = new List<AssignProductToIssueResult>();
					var anyFailure = false;
					var anySuccess = false;
					foreach (var product in request.DTO.IssueItems)
					{
						var reusablePalletsForProduct = oldPallets.Pallets.Where(p => p.ContainsProduct(product.ProductId)).ToList();

						var result = await _assignProductToIssueAsync.AssignGoodsToIssue(issue, product,
							IssueAllocationPolicy.FullPalletFirst, reusablePalletsForProduct, request.DTO.PerformedBy, transactionCt);

						if (!result.Success) //niepowodzenie biznesowe
						{
							results.Add(result);
							anyFailure = true;
							continue;
						}
						var palletAssigned = result.AssignedPallets?.ToList() ?? [];
						issue.CompleteReallocation(palletAssigned, reusablePalletsForProduct);
						anySuccess = true;
						results.Add(result);
					}
					if (oldPallets.ListPalletsIds.Count != 0)
					{
						// Usuwamy tylko puste VirtualPallets; fizyczne palety wracają do dostępnych.
						foreach (var item in oldPallets.ListPalletsIds)
						{
							var vp = await _virtualRepo.GetVirtualPalletByIdAsync(item, transactionCt);
							if (vp!.CanBeDeletedAfterReallocation())
							{
								vp.Pallet.ChangeStatus(PalletStatus.Available);
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
					await _unitOfWork.SaveChangesAsync(transactionCt);

					var response = new IssueCreateModifyResult(
						issue.Id,
						issue.IssueNumber,
						"Issue was modified.",
						results);

					return AppResult<IssueCreateModifyResult>.Success(response);

				}, IsolationLevel.Serializable, ct);
		}

		private async Task<AppResult<IssueCreateModifyResult>> CreateSupplementaryIssue(
			ModifyIssueCommand request,
			CancellationToken ct)
		{
			var newQuantities = new List<IssueItemDTO>();
			var hasNegativeDiff = false;
			var errorMessage = new List<string>();

			var issue = await _issueRepo.GetIssueByIdForModifyAsync(request.Id, ct);
			if (issue == null)
			{
				return AppResult<IssueCreateModifyResult>.Fail("Issue was not found.");
			}
			foreach (var product in request.DTO.IssueItems)
			{
				var productId = product.ProductId;
				var oldQuantity = issue.GetQuantityForProduct(productId);

				var newQuantity = product.Quantity - oldQuantity;
				if (newQuantity < 0)
				{
					hasNegativeDiff = true;
					errorMessage.Add($"Product {productId}: quantity cannot be decreased from {oldQuantity} to {product.Quantity} (difference: {newQuantity}). The issue has already been approved for loading.");
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
				return AppResult<IssueCreateModifyResult>.Fail(
					  string.Join(";", errorMessage),
				   ErrorType.Conflict
			   );
			}
			if (newQuantities.Count == 0)
			{
				var responseNotChange = new IssueCreateModifyResult(
					issue.Id,
					issue.IssueNumber,
					"No quantity changes were detected; the issue was not modified.");

				return AppResult<IssueCreateModifyResult>.Success(responseNotChange);
			}
			var dataForNewIssue = new CreateIssueDTO
			{
				ClientId = request.DTO.ClientId,
				Items = newQuantities,
				PerformedBy = request.DTO.PerformedBy,
			};
			var receiverFromCreate = await _mediator.Send(new CreateIssueCommand(dataForNewIssue, request.DateToSend), ct);
			if (!receiverFromCreate.IsSuccess || receiverFromCreate.Result is null)
			{
				return AppResult<IssueCreateModifyResult>.Fail(
					receiverFromCreate.Error ?? "The supplementary issue could not be created.",
					ErrorType.Conflict);
			}
			var cretedIssue = receiverFromCreate.Result;
			var response = new IssueCreateModifyResult
			(
				cretedIssue.IssueId,
				cretedIssue.IssueNumber,
				"A last-minute supplementary issue was created because the original issue is already in progress.",
				cretedIssue.Results
			);
			return AppResult<IssueCreateModifyResult>.Success(response);
		}
	}
}
