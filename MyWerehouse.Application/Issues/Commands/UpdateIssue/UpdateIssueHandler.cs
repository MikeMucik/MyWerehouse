using System.Data;
using System.Linq;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Issues.Commands.CreateIssue;
using MyWerehouse.Application.Issues.DTOs;
using MyWerehouse.Application.Issues.IssuesServices;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.Issues.Commands.UpdateIssue
{
	public class UpdateIssueHandler(
		IIssueRepo issueRepo,
		IPalletRepo palletRepo,
		IMediator mediator,
		WerehouseDbContext werehouseDbContext,
		IAssignProductToIssueService assignProductToIssueAsync) : IRequestHandler<UpdateIssueCommand, AppResult<List<IssueResult>>>
	{
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPalletRepo _palletRepo = palletRepo;
		private readonly IMediator _mediator = mediator;
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IAssignProductToIssueService _assignProductToIssueAsync = assignProductToIssueAsync;
		public async Task<AppResult<List<IssueResult>>> Handle(UpdateIssueCommand request, CancellationToken ct)
		{
			var resultList = new List<IssueResult>();
			var issue = await _issueRepo.GetIssueByIdAsync(request.DTO.Id);
			if (issue == null)
				return AppResult<List<IssueResult>>.Fail("Zamówienie nie zostało znalezione.", ErrorType.NotFound);

			// Nowe zlecenie można podmienić wszystkie palety i nie zatwierdzone, nie zaczęty picking
			if (issue.IssueStatus == IssueStatus.New ||
				issue.IssueStatus == IssueStatus.Pending ||
				issue.IssueStatus == IssueStatus.NotComplete)
			{
				await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
				//Kasowanie starych palet, zatrzymanie palet do tego samego zlecenia, kasowanie pickingu
				
				var reusablePallets = new List<Pallet>();//
				var listOldPallets = issue.Pallets.ToList();//
				foreach (var pallet in listOldPallets)
				{
					issue.DetachPallet(pallet);
					pallet.DetachToIssue(request.DTO.PerformedBy, pallet.Location.ToSnapshot(), Domain.Histories.Models.ReasonMovement.Correction);
					pallet.ChangeStatus(PalletStatus.LockedForIssue);//potrzebne by palety nie były dostępne w innych miejscach
					reusablePallets.Add(pallet);
				}
				var listOldPickingTask = issue.PickingTasks.ToList();
				//remove old PickingTask 
				foreach (var pickingTask in listOldPickingTask)
				{
					var sourcePallet = await _palletRepo.GetPalletByIdAsync(pickingTask.VirtualPallet.PalletId);
					issue.RemovePickingTask(pickingTask);
					pickingTask.Cancel(request.DTO.PerformedBy, issue.IssueNumber);

				}
				await _werehouseDbContext.SaveChangesAsync(ct);
				var anyFailure = false; // Flaga, czy wystąpił jakikolwiek błąd
				var anySuccess = false; // Flaga, czy cokolwiek się udało
				foreach (var product in request.DTO.Items) //bo dla każdego osobno i memo na koniec
				{
					// dla dwóch takich samych produktów - zabezpieczenie, nie powinno się zdarzyć
					var savepointName = $"BeforeProduct_{product.ProductId}_{Guid.NewGuid()}";
					await transaction.CreateSavepointAsync(savepointName, ct);
					try
					{
						var reusablePalletsForProduct = reusablePallets.Where(p => p.ContainsProduct(product.ProductId)).ToList();
						var result = await _assignProductToIssueAsync.AssignProductToIssue(issue, product, IssueAllocationPolicy.FullPalletFirst, reusablePalletsForProduct, request.DTO.PerformedBy);
						if (!result.Success) //niepowodzenie biznesowe
						{
							await transaction.RollbackToSavepointAsync(savepointName, ct);
							await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
							await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
							await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);

							resultList.Add(IssueResult.Fail(result.Message, product.ProductId));
							anyFailure = true;
							continue;
						}
						var palletAssigned = result.AssignedPallets.ToList();

						var assignedIds = palletAssigned.Select(p => p.Id).ToHashSet();
						var returnPallets = reusablePalletsForProduct
							.Where(p => !assignedIds.Contains(p.Id))
							.ToList();

						foreach (var returnPallet in returnPallets)
						{
							returnPallet.ChangeStatus(PalletStatus.Available);
						}
						await _werehouseDbContext.SaveChangesAsync(ct);
						resultList.Add(IssueResult.Ok("Towar dołączono do wydania", product.ProductId));
						anySuccess = true;
					}
					catch (Exception ex) // Łapiemy tutaj wyjątki domenowe, żeby obsłużyć logikę czyszczenia, częsciowy wynik da odpowiedz co jest nie tak z załadunkiem  
					{
						await transaction.RollbackToSavepointAsync(savepointName, ct);
						await _werehouseDbContext.Entry(issue).ReloadAsync(ct);
						await _werehouseDbContext.Entry(issue).Collection(i => i.Pallets).LoadAsync(ct);
						await _werehouseDbContext.Entry(issue).Collection(i => i.PickingTasks).LoadAsync(ct);
						// Logowanie krytyczne
						// _logger.LogError(ex, ...)
						resultList.Add(IssueResult.Fail("Wystąpił nieoczekiwany błąd", product.ProductId));
						anyFailure = true;
					}
				}
				var touchedVirtualPalletIds = listOldPickingTask
					.Select(a => a.VirtualPalletId)
					.Distinct()
					.ToList();
				// Pobieramy kandydatów do usunięcia WRAZ z ich fizycznymi paletami - usuwamy tylko virtualPallet
				var virtualPalletsToCheck = await _werehouseDbContext.VirtualPallets
					.Include(vp => vp.Pallet) // Ważne!
					.Where(vp =>
					 //touchedVirtualPalletIds != null && //niepotrzebne do przemyślenia
					 touchedVirtualPalletIds.Contains(vp.Id))
					.ToListAsync(ct);

				var emptyVirtualPallets = virtualPalletsToCheck
					.Where(vp => vp.PickingTasks.Count == 0) // Sprawdzamy w pamięci (EF powinien widzieć zmiany z pętli)
					.ToList();

				foreach (var vp in emptyVirtualPallets)
				{
					if (vp.Pallet != null)
					{
						vp.Pallet.ChangeStatus(PalletStatus.Available);
					}
					_werehouseDbContext.VirtualPallets.Remove(vp);
				}
				if (anySuccess)
				{
					issue.ChangeUser(request.DTO.PerformedBy);
					issue.ChangeStatus(IssueStatus.Pending);
				}
				if (anyFailure)
				{
					issue.ChangeStatus(IssueStatus.NotComplete);
				}
				issue.AddHistory(request.DTO.PerformedBy);
				await _werehouseDbContext.SaveChangesAsync(ct);
				await transaction.CommitAsync(ct);				
				return AppResult<List<IssueResult>>.Success(resultList);
			}
			else if (issue.IssueStatus == IssueStatus.ConfirmedToLoad)
			{
				var newQuantities = new List<IssueItemDTO>();
				var hasNegativeDiff = false;
				var errorMessage = new List<string>();
				foreach (var product in request.DTO.Items)
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
					return AppResult<List<IssueResult>>.Fail(
						  string.Join(";", errorMessage),
					   ErrorType.Conflict // lub Validation, zależnie od semantyki
				   );
				}
				if (newQuantities.Count == 0)
				{
					var resultListNoQuantitesChange = new List<IssueResult>
					{
						IssueResult.Ok("Brak zmian w ilościach - zlecenie bez modyfikacji.")
					};
					return AppResult<List<IssueResult>>.Success(resultListNoQuantitesChange);
				}
				var dataForNewIssue = new CreateIssueDTO
				{
					ClientId = issue.ClientId,
					Items = newQuantities,
					PerformedBy = request.DTO.PerformedBy,
				};
				var receiverFromCreate = await _mediator.Send(new CreateIssueCommand(dataForNewIssue, request.DateToSend), ct);
				if (receiverFromCreate.IsSuccess is false || receiverFromCreate is null)
					return AppResult<List<IssueResult>>.Fail("Nie udało się utworzyć nowego zlecenia.", ErrorType.Conflict); //Technical??
				resultList = receiverFromCreate.Result.ToList();
				foreach (var result in resultList)
				{
					if (result.Success)
					{
						result.Message += " (Dodatkowe zlecenie na ostatnią chwilę - stare jest w realizacji).";
						//dodatkowy towar do zlecenia w nowym zleceniu
					}
				}
				return AppResult<List<IssueResult>>.Success(resultList);
			}
			else
			{
				resultList.Add(IssueResult.Fail(
					$"Nie można zaktualizować zlecenia {issue.Id}, status: {issue.IssueStatus}"));
				return AppResult<List<IssueResult>>.Success(resultList);
			}
		}

	}
}
