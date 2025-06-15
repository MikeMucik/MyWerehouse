using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.IssueModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class IssueService : IIssueService
	{
		private readonly IIssueRepo _issueRepo;
		private readonly IMapper _mapper;
		private readonly IInventoryRepo _inventoryRepo;
		private readonly IPalletRepo _palletRepo;

		public IssueService(
			IIssueRepo issueRepo,
			IMapper mapper,
			IInventoryRepo inventoryRepo,
			IPalletRepo palletRepo)
		{
			_issueRepo = issueRepo;
			_mapper = mapper;
			_inventoryRepo = inventoryRepo;
			_palletRepo = palletRepo;
		}

		public void AddIssue(int clientId, string perfomedBy, List<IssueItemDTO> values)
		{
			var palletsAddedToIssue = new List<Pallet>();
			foreach (var product in values)
			{
				var isEnough = _inventoryRepo.HasStock(product.ProductId, product.Quantity);
				if (isEnough)
				{					
					var pallets = _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore);
					var palletsToIssue =  SelectPalletsForIssue(pallets, product.Quantity);
					palletsAddedToIssue.AddRange(palletsToIssue);//tu jeszcze trezba dodać zmianę statusu palety
				}
			}
			var issue = new Issue
			{
				ClientId = clientId,
				IssueDateTime = DateTime.Now,
				PerformedBy = perfomedBy,
				Pallets = palletsAddedToIssue
			};
			_issueRepo.AddIssue(issue);
		}
		public async Task AddIssueAsync(int clientId, string perfomedBy, List<IssueItemDTO> values)
		{
			var palletsAddedToIsue = new List<Pallet>();
			foreach (var product in values)
			{
				var isEnough = _inventoryRepo.HasStock(product.ProductId, product.Quantity);
				if (isEnough)
				{
					var pallets = _palletRepo.GetAvailablePallets(product.ProductId, product.BestBefore);
					var palletsToIssue =await SelectPalletsForIssueAsync(pallets, product.Quantity);
					palletsAddedToIsue.AddRange(palletsToIssue);
				}
			}
			var issue = new Issue
			{
				ClientId = clientId,
				IssueDateTime = DateTime.Now,
				PerformedBy = perfomedBy,
				Pallets = palletsAddedToIsue
			};
			await _issueRepo.AddIssueAsync(issue);
		}
		public void UpdateIssue(int clientId, string perfomedBy, List<IssueItemDTO> values)
		{
			throw new NotImplementedException();
		}

		public void DeleteIssue(int issueId)
		{
			throw new NotImplementedException();
		}

		public void LoadingIssue(int clientId, int issueId, string sendedBy)
		{
			//zebrać palety po wysyłki i przy pomocy zatwierdzania zamieniać załadowane
			// czyli paleta zmienia status na loaded - updatepallet
			//zmiana lokalizacji
			//gdy wszystkie zatwierdzone mają dobry status loaded
			//to zmiana statusu Issue na isShipped 
			throw new NotImplementedException();
		}
		public void CompletedIssue(int clientId, int issueId, string confirmedBy)
		{
			//podobnie jak wyżej ale paleta na archived
			// ?czy wykasowanie lokalizacji palety czy jakaś wirtualna
			//gdy wszystkie zatwierdzone mają dobry status archived
			//to zmiana statusu Issue Archived
			throw new NotImplementedException();
		}
		public List<Pallet> SelectPalletsForIssue(IQueryable<Pallet> pallets, int quantity)
		{
			var result = new List<Pallet>();
			int collected = 0;
			foreach (var pallet in pallets)
			{
				var productOnPallet = pallet.ProductsOnPallet.FirstOrDefault();
				if (productOnPallet == null)
					continue;
				collected += productOnPallet.Quantity;
				result.Add(pallet);
				if (collected >= quantity)
				{
					if (collected > quantity)
					{
						pallet.Status = PalletStatus.ToPicking;
					}
					break;
				}
			}
			return result;
		}
		public async Task<List<Pallet>> SelectPalletsForIssueAsync(IQueryable<Pallet> pallets, int quantity)
		{
			var palletsToIssue =await pallets.ToListAsync();
			var result = new List<Pallet>();
			int collected = 0;

			foreach (var pallet in palletsToIssue)
			{
				var productOnPallet = pallet.ProductsOnPallet.FirstOrDefault();
				if (productOnPallet == null)
					continue;
				collected += productOnPallet.Quantity;
				result.Add(pallet);
				if (collected >= quantity)
				{
					if (collected > quantity)
					{
						pallet.Status = PalletStatus.ToPicking;
					}
					break;
				}
			}
			return result;
		}

		
	}
}
