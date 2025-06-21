using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Services
{
	public class PalletService : IPalletService
	{
		private readonly IPalletRepo _palletRepo;
		private readonly IPalletMovementRepo _palletMovementRepo;
		private readonly IProductOnPalletRepo _productOnPalletRepo;
		private readonly IMapper _mapper;
		private readonly IValidator<CreatePalletReceiptDTO> _receiptValidator;
		private readonly IValidator<CreatePalletPickingDTO> _pickingValidator;

		public PalletService(
			IPalletRepo palletRepo,
			IPalletMovementRepo? palletMovementRepo,
			IProductOnPalletRepo productOnPalletRepo,
			IMapper mapper,
			IValidator<CreatePalletReceiptDTO>? receiptValidator,
			IValidator<CreatePalletPickingDTO>? pickingValidator)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_productOnPalletRepo = productOnPalletRepo;
			_mapper = mapper;
			_receiptValidator = receiptValidator;
			_pickingValidator = pickingValidator;
		}

		public string AddPalletReceipt(CreatePalletReceiptDTO addPalletDTO)
		{
			addPalletDTO.Id = _palletRepo.GetNextPalletId();//kolejny numer palety
			addPalletDTO.LocationId = 0;//lokalizacja początkowa
			addPalletDTO.DateReceived = DateTime.Now;
			addPalletDTO.ReceiptId = 1;//zaślepka - to będzie przychodzić z serwisu receipt

			var validationResult = _receiptValidator.Validate(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = _palletRepo.AddPallet(pallet);
			if (addPalletDTO.ProductsOnPallet != null)
			{
				foreach (var item in addPalletDTO.ProductsOnPallet)
				{
					var product = new ProductOnPallet
					{
						PalletId = id,
						ProductId = item.Id,
						BestBefore = item.BestBefore,
						DateAdded = DateTime.Now,
						Quantity = item.Quantity,
					};
					_productOnPalletRepo.AddProductToPallet(product);
				}
				return id;
			}
			else { throw new InvalidDataException($"Brak produktów na palecie {id}"); }
		}
		public async Task<string> AddPalletReceiptAsync(CreatePalletReceiptDTO addPalletDTO)
		{
			addPalletDTO.Id = await _palletRepo.GetNextPalletIdAsync();
			addPalletDTO.LocationId = 0;//lokalizacja początkowa
			addPalletDTO.DateReceived = DateTime.Now;
			addPalletDTO.ReceiptId = 1;//zaślepka - to będzie przychodzić z serwisu receipt

			var validationResult = await _receiptValidator.ValidateAsync(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}

			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = await _palletRepo.AddPalletAsync(pallet);
			foreach (var item in addPalletDTO.ProductsOnPallet)
			{
				var product = new ProductOnPallet
				{
					PalletId = id,
					ProductId = item.Id,
					BestBefore = item.BestBefore,
					DateAdded = DateTime.Now,
					Quantity = item.Quantity,
				};
				await _productOnPalletRepo.AddProductToPalletAsync(product);
			}
			return id;
		}
		public string AddPalletPicking(CreatePalletPickingDTO addPalletDTO)
		{
			addPalletDTO.Id = _palletRepo.GetNextPalletId();
			//addPalletDTO.LocationId = 0;
			addPalletDTO.DateCreated = DateTime.Now;
			addPalletDTO.IssueId = 1;//zaślepka 

			var validationResult = _pickingValidator.Validate(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}

			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = _palletRepo.AddPallet(pallet);
			foreach (var item in addPalletDTO.ProductsOnPallet)
			{
				var product = new ProductOnPallet
				{
					PalletId = id,
					ProductId = item.Id,
					BestBefore = item.BestBefore,
					DateAdded = DateTime.Now,
					Quantity = item.Quantity,
				};
				_productOnPalletRepo.AddProductToPallet(product);
			}
			return id;
		}
		public async Task<string> AddPalletPickingAsync(CreatePalletPickingDTO addPalletDTO)
		{
			addPalletDTO.Id = await _palletRepo.GetNextPalletIdAsync();
			//addPalletDTO.LocationId = 0;
			addPalletDTO.DateCreated = DateTime.Now;
			addPalletDTO.IssueId = 1;//zaślepka 

			var validationResult = await _pickingValidator.ValidateAsync(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}

			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = await _palletRepo.AddPalletAsync(pallet);
			foreach (var item in addPalletDTO.ProductsOnPallet)
			{
				var product = new ProductOnPallet
				{
					PalletId = id,
					ProductId = item.Id,
					BestBefore = item.BestBefore,
					DateAdded = DateTime.Now,
					Quantity = item.Quantity,
				};
				await _productOnPalletRepo.AddProductToPalletAsync(product);
			}
			return id;
		}

		public void DeletePallet(string id)
		{
			var pallet = _palletRepo.GetPalletById(id);
			if (pallet == null)
				throw new ArgumentException("Nie ma palety o tym numerze");
			bool canDelete = _palletMovementRepo.CanDeletePallet(id);
			if (!canDelete) 
			throw new InvalidOperationException($"Palety o numerze {id} nie można usunąć"); 
				
					_palletRepo.DeletePallet(id);		
		}
		public async Task DeletePalletAsync(string id)
		{
			var pallet = await _palletRepo.GetPalletByIdAsync(id);
			if (pallet == null)
				throw new ArgumentException($"Nie ma palety o numerze {id}");

			var canDelete = await _palletMovementRepo.CanDeletePalletAsync(id);
			if (!canDelete)
				throw new InvalidOperationException($"Palety o numerze {id} nie można usunąć");

			await _palletRepo.DeletePalletAsync(id);
		}

		public CreatePalletReceiptDTO GetPalletToEdit(string id)
		{
			var pallet = _palletRepo.GetPalletById(id);
			var palletDTO = _mapper.Map<CreatePalletReceiptDTO>(pallet);
			return palletDTO;
		}

		public void UpdatePallet(CreatePalletReceiptDTO updatingPallet)
		{
			throw new NotImplementedException();
		}
	}
}
