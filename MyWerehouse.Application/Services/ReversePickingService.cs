using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Events;
using MyWerehouse.Application.Common.Exceptions;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Pallets.Commands.CreateNewPallet;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Application.ReversePickings.Events.CreateHistoryReversePicking;
using MyWerehouse.Application.Pallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Infrastructure;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Picking.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Pallets.Filters;
using MyWerehouse.Domain.Histories.Models;
using MyWerehouse.Application.Common.Exceptions.NotFoundException;

namespace MyWerehouse.Application.Services
{
	public class ReversePickingService : IReversePickingService
	{
		private readonly IAllocationRepo _allocationRepo;
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IProductRepo _productRepo;
		private readonly IMediator _mediator;
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IMapper _mapper;
		private readonly IEventCollector _eventCollector;
		public ReversePickingService(
			IAllocationRepo allocationRepo,
			WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IProductRepo productRepo,
			IMediator mediator,
			IReversePickingRepo reversePickingRepo,
			IMapper mapper,
			IEventCollector eventCollector)
		{
			_allocationRepo = allocationRepo;
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_productRepo = productRepo;
			_mediator = mediator;
			_reversePickingRepo = reversePickingRepo;
			_mapper = mapper;
			_eventCollector = eventCollector;
		}

		public async Task<List<ReversePicking>> CreateTaskToReversePickingAsync(string palletId, string userId)//paleta kompletacyjna różne asortymenty
		{

			await using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			var listTasks = new List<ReversePicking>();
			var pallet = await _palletRepo.GetPalletByIdAsync(palletId)
				?? throw new PalletException(palletId);
			var issue = pallet.Issue
				?? throw new NotFoundIssueException("Brak zlecenia wydania.");
			foreach (var residue in pallet.ProductsOnPallet)
			{
				var allocations = await _allocationRepo.GetAllocationsByIssueIdProductIdAsync(issue.Id, residue.ProductId);
				foreach (var allocation in allocations)
				//Dla każdej wykonanej alokacji stwórz zadanie odwrotne
				{
					listTasks.Add(new ReversePicking
					{
						PickingPalletId = palletId,
						Quantity = allocation.Quantity,
						ProductId = residue.ProductId,
						BestBefore = residue.BestBefore,
						Status = ReversePickingStatus.Pending,
						AllocationId = allocation.Id,
						UserId = userId,
					});
				}
				foreach (var task in listTasks)
				{
					 _reversePickingRepo.AddReversePicking(task);
				}
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				foreach (var task in listTasks)
				{
					var itemHistory = new HistoryReversePickingItem
					(
						task.Id,
						task.SourcePalletId,
						task.DestinationPalletId,
						issue.Id,
						task.ProductId,
						task.Quantity,
						null,
						task.Status
					);
					await _mediator.Publish(new CreateHistoryReversePickingNotification(itemHistory, userId));
				}
			}
			return listTasks;
		}
		public async Task<ReversePickingResult> ExecutiveReversePickingAsync(int taskReverseId, ReversePickingStrategy strategy, string userId)
		{
			using var transaction = await _werehouseDbContext.Database.BeginTransactionAsync();
			try
			{
				var result = new ReversePickingResult();
				var reversePicking = await _reversePickingRepo.GetReversePickingAsync(taskReverseId) ??
					throw new ReversePickingException(taskReverseId);
				if (strategy == ReversePickingStrategy.AddToExistingPallet) 
				{
					var filter = new PalletSearchFilter
					{
						ProductId = reversePicking.ProductId,
						BestBefore = reversePicking.BestBefore,
					};
					var addingPallets = _palletRepo.GetPalletsByFilter(filter);
					var palletToAdded = await addingPallets
							.Where(p => p.ReceiptId != null)//paleta z przyjęcia ma numer przyjęcia
							.OrderBy(q => q.ProductsOnPallet.First().Quantity)//paleta z przyjęcia ma tylko jeden asortyment
							.FirstOrDefaultAsync();

					if (palletToAdded != null)
					{
						var product = await _productRepo.GetProductByIdAsync(reversePicking.ProductId)
							?? throw new ProductException($"Produkt {reversePicking.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
						var numberOfCartoons = product.CartonsPerPallet;
						if ((palletToAdded.ProductsOnPallet.First().Quantity + reversePicking.Quantity) > numberOfCartoons)
						{
							strategy = ReversePickingStrategy.AddToNewPallet;
						}
						else
						{
							reversePicking.DestinationPalletId = palletToAdded.Id;
						}
					}
					else throw new PalletException("Brak palety do której można dodać.");
				}
				
				reversePicking.Status = ReversePickingStatus.InProgress;
				string? sourcePalletId = null;
				string? destinationPalletId = null;
				var issueId = reversePicking.Allocation.IssueId;
				if (issueId == 0)
					throw new NotFoundIssueException(reversePicking.Allocation.IssueId);
				switch (strategy)
				{
					case ReversePickingStrategy.ReturnToSource:
						sourcePalletId = reversePicking.SourcePalletId;
						if (sourcePalletId == null) throw new PalletException("Brak palety do której można dodać.");//problem bo id string
						result = AddProductsToSourcePallet(reversePicking, userId);
						break;
					case ReversePickingStrategy.AddToExistingPallet:
						result = await AddToExistingPallet(reversePicking, userId);
						destinationPalletId = reversePicking.DestinationPalletId;
						break;
					case ReversePickingStrategy.AddToNewPallet:
						result = AddToNewPallet(reversePicking, userId);
						break;
				}
				reversePicking.Status = ReversePickingStatus.Completed;
				await _werehouseDbContext.SaveChangesAsync();
				await transaction.CommitAsync();
				var history = new HistoryReversePickingItem(reversePicking.Id,
					sourcePalletId,
					destinationPalletId,
					issueId,
					reversePicking.Quantity,
					reversePicking.ProductId,
					ReversePickingStatus.InProgress,
					ReversePickingStatus.Completed);
				await _mediator.Publish(new CreateHistoryReversePickingNotification(history, userId));
				foreach (var evn in _eventCollector.Events)
				{
					await _mediator.Publish(evn);
				}
				foreach (var factory in _eventCollector.DeferredEvents)
				{
					await _mediator.Publish(await factory());
				}
				//_eventCollector.Clear();
				return result;
			}
			catch (NotFoundIssueException ie)
			{
				await transaction.RollbackAsync();
				return ReversePickingResult.Fail(ie.Message);
			}
			catch (PalletException pe)
			{
				await transaction.RollbackAsync();
				return ReversePickingResult.Fail(pe.Message);
			}
			catch (ProductException proe)
			{
				await transaction.RollbackAsync();
				return ReversePickingResult.Fail(proe.Message);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Loguj ex dla developera!
				//_logger.LogError(ex, "Błąd podczas ręcznej kompletacji");	
				throw new InvalidOperationException("Wystąpił błąd podczas usuwania zlecenia.", ex);
			}
			finally
			{
				_eventCollector.Clear();
			}
		}

		private ReversePickingResult AddProductsToSourcePallet(ReversePicking task, string userId)
		{
			var sourcePallet = task.Allocation.VirtualPallet.Pallet;
			if (sourcePallet.Status != PalletStatus.Archived)
			{
				sourcePallet.ProductsOnPallet.First().Quantity += task.Quantity;
			}
			_eventCollector.Add(new CreatePalletOperationNotification(
				sourcePallet.Id,
				sourcePallet.LocationId,
				ReasonMovement.Correction,
				userId,
				PalletStatus.Available,
				null));
			return ReversePickingResult.Ok("Dodano towar do palety źródłowej", task.ProductId, task.SourcePalletId);
		}
		private async Task<ReversePickingResult> AddToExistingPallet(ReversePicking task, string userId)
		{
			var palletToAdd = await _palletRepo.GetPalletByIdAsync(task.DestinationPalletId);
			if (palletToAdd != null)
			{
				palletToAdd.ProductsOnPallet.First().Quantity += task.Quantity;
			}
			else throw new PalletException("Brak palety do dodania");
			_eventCollector.Add(new CreatePalletOperationNotification(
				palletToAdd.Id,
				palletToAdd.LocationId,
				ReasonMovement.Correction,
				userId,
				PalletStatus.Available,
				null));
			return ReversePickingResult.Ok("Dodano towar do palety niepełnej", task.ProductId, task.DestinationPalletId);
		}
		private ReversePickingResult AddToNewPallet(ReversePicking task, string userId)
		{
			var newPallet = new Pallet
			{
				DateReceived = DateTime.UtcNow,
				LocationId = 1,
				Status = PalletStatus.InStock,
				ReceiptId = 1000,//to trzeba poprawić żeby taka nowa paleta miała jakieś przyjęcie tylko palety kompletacyjne nie mają ReceiptId
				ProductsOnPallet = new List<ProductOnPallet>
				{new ProductOnPallet
					{
						ProductId = task.ProductId,
						DateAdded = DateTime.UtcNow,
						Quantity = task.Quantity,
					 },
				},
			};
			_palletRepo.AddPallet(newPallet);
			_eventCollector.AddDeferred(async () =>
			new CreatePalletOperationNotification(
				newPallet.Id,
				1, ReasonMovement.New,
				userId,
				PalletStatus.InStock,
				null));
			return ReversePickingResult.Ok("Dodano towar do nowej palety.", task.ProductId, newPallet.Id);
		}
		public async Task<ReversePickingDetails> GetReversePickingAsync(int reversePickingId)
		{
			var reversePicking = await _reversePickingRepo.GetReversePickingAsync(reversePickingId) ??
				throw new ReversePickingException(reversePickingId);
			var reversePickingDTO = _mapper.Map<ReversePickingDTO>(reversePicking);
			var sourcePallet = reversePicking?.Allocation.VirtualPallet.Pallet;
			var exsitingPickingPallet = false;
			var existingPalletWithProduct = false;
			if (sourcePallet != null)
			{
				if (sourcePallet.Status != PalletStatus.Archived)
				{
					exsitingPickingPallet = true;
				}
			}
			var filter = new PalletSearchFilter
			{
				ProductId = reversePicking.ProductId,
				BestBefore = reversePicking.BestBefore,
			};
			var addingPallets = _palletRepo.GetPalletsByFilter(filter);
			var product = await _productRepo.GetProductByIdAsync(reversePicking.ProductId)
			?? throw new ProductException($"Produkt {reversePicking.ProductId} nie ma ustawionej ilosci kartonów na paletę. Popraw produkt");
			var numberOfCartoons = product.CartonsPerPallet;
			var palletToAdded = await addingPallets
				.Where(p => p.ReceiptId != null && p.Status == PalletStatus.Available
				&& p.ProductsOnPallet.First().Quantity < numberOfCartoons)//paleta z przyjęcia ma numer przyjęcia				
				.OrderBy(q => q.ProductsOnPallet.First().Quantity)//paleta z przyjęcia ma tylko jeden asortyment
				.FirstOrDefaultAsync();
			if (palletToAdded != null)
			{
				existingPalletWithProduct = true;
			}
			var result = new ReversePickingDetails
			{
				ReversePickingDTO = reversePickingDTO,
				CanReturnToSource = exsitingPickingPallet,
				CanAddToExistingPallet = existingPalletWithProduct,
			};
			return result;
		}

		public async Task<ListReversePickingDTO> GetListReversePickingToDo(int pageSize, int pageNumber)
		{
			//var listReverse = await _reversePickingRepo.GetReversePickings()

			var listReversePicking = _reversePickingRepo.GetReversePickings()
				.Where(r => r.Status == ReversePickingStatus.Pending)
				.ProjectTo<ReversePickingDTO>(_mapper.ConfigurationProvider);
			var listToShow = await listReversePicking
				.Skip(pageSize * (pageNumber - 1))
				.Take(pageSize)
				.ToListAsync();
			var listReversePickingDTO = new ListReversePickingDTO()
			{
				DTOs = listToShow,
				PageSize = pageSize,
				CurrentPage = pageNumber,
				Count = await listReversePicking.CountAsync()
			};
			return listReversePickingDTO;
		}
	}
}