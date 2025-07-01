using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Utils;
using MyWerehouse.Application.ViewModels.PalletModels;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

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
		private readonly IValidator<UpdatePalletDTO> _updateValidator;
		private readonly WerehouseDbContext _werehouseDbContext;

		public PalletService(
			IPalletRepo palletRepo,
			IPalletMovementRepo? palletMovementRepo,
			IProductOnPalletRepo productOnPalletRepo,
			IMapper mapper,
			IValidator<CreatePalletReceiptDTO>? receiptValidator,
			IValidator<CreatePalletPickingDTO>? pickingValidator,
			IValidator<UpdatePalletDTO>? updateValidator,
			WerehouseDbContext werehouseDbContext)
		{
			_palletRepo = palletRepo;
			_palletMovementRepo = palletMovementRepo;
			_productOnPalletRepo = productOnPalletRepo;
			_mapper = mapper;
			_receiptValidator = receiptValidator;
			_pickingValidator = pickingValidator;
			_updateValidator = updateValidator;
			_werehouseDbContext = werehouseDbContext;
		}

		public PalletService(
			IPalletRepo palletRepo,
			IMapper mapper,
			IValidator<UpdatePalletDTO> updateValidator)
		{
			_palletRepo = palletRepo;
			_mapper = mapper;
			_updateValidator = updateValidator;
		}

		public string AddPalletReceipt(CreatePalletReceiptDTO addPalletDTO)
		{
			addPalletDTO.Id = _palletRepo.GetNextPalletId();//kolejny numer palety
			addPalletDTO.LocationId = 0;//lokalizacja początkowa
			addPalletDTO.DateReceived = DateTime.Now;
			var validationResult = _receiptValidator.Validate(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = _palletRepo.AddPallet(pallet);

			var listRecordsPMD = new List<PalletMovementDetails>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetails
				{
					ProductId = product.Id,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = id,
				LocationId = pallet.LocationId,
				Reason = ReasonMovement.Received,
				PerformedBy = addPalletDTO.UserId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			_palletMovementRepo.AddPalletMovement(recordPM);
			//foreach (var product in pallet.ProductsOnPallet)
			//{
			//	var record = new PalletMovement
			//	{
			//		PalletId = id,
			//		ProductId = product.ProductId,
			//		LocationId = pallet.LocationId,
			//		Reason = ReasonMovement.Received,
			//		PerformedBy = addPalletDTO.UserId,
			//		Quantity = product.Quantity,
			//		MovementDate = DateTime.Now,
			//	};
			//	_palletMovementRepo.AddPalletMovement(record);
			//}
			_werehouseDbContext.SaveChanges();
			return id;
		}
		public async Task<string> AddPalletReceiptAsync(CreatePalletReceiptDTO addPalletDTO)
		{
			addPalletDTO.Id = await _palletRepo.GetNextPalletIdAsync();
			addPalletDTO.LocationId = 0;//lokalizacja początkowa
			addPalletDTO.DateReceived = DateTime.Now;
			var validationResult = await _receiptValidator.ValidateAsync(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = await _palletRepo.AddPalletAsync(pallet);

			var listRecordsPMD = new List<PalletMovementDetails>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetails
				{
					ProductId = product.Id,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = id,
				LocationId = pallet.LocationId,
				Reason = ReasonMovement.Received,
				PerformedBy = addPalletDTO.UserId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			await _palletMovementRepo.AddPalletMovementAsync(recordPM);
			//foreach (var product in pallet.ProductsOnPallet)
			//{
			//	var record = new PalletMovement
			//	{
			//		PalletId = id,
			//		ProductId = product.ProductId,
			//		LocationId = pallet.LocationId,
			//		Reason = ReasonMovement.Received,
			//		PerformedBy = addPalletDTO.UserId,
			//		Quantity = product.Quantity,
			//		MovementDate = DateTime.Now,
			//	};
			//	await _palletMovementRepo.AddPalletMovementAsync(record);
			//}
			await _werehouseDbContext.SaveChangesAsync();
			return id;
		}
		public string CreatePickingPallet(CreatePalletPickingDTO addPalletDTO)
		{
			addPalletDTO.Id = _palletRepo.GetNextPalletId();
			addPalletDTO.DateCreated = DateTime.Now;
			var validationResult = _pickingValidator.Validate(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = _palletRepo.AddPallet(pallet);

			var listRecordsPMD = new List<PalletMovementDetails>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetails
				{
					ProductId = product.Id,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = id,
				LocationId = pallet.LocationId,
				Reason = ReasonMovement.Picking,
				PerformedBy = addPalletDTO.UserId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			_palletMovementRepo.AddPalletMovement(recordPM);
			//foreach (var product in pallet.ProductsOnPallet)
			//{
			//	var record = new PalletMovement
			//	{
			//		PalletId = id,
			//		ProductId = product.ProductId,
			//		LocationId = pallet.LocationId,
			//		Reason = ReasonMovement.Picking,
			//		PerformedBy = addPalletDTO.UserId,
			//		Quantity = product.Quantity,
			//		MovementDate = DateTime.Now,
			//	};
			//	_palletMovementRepo.AddPalletMovement(record);
			//}
			_werehouseDbContext.SaveChanges();
			return id;
		}
		public async Task<string> CreatePickingPalletAsync(CreatePalletPickingDTO addPalletDTO)
		{
			addPalletDTO.Id = await _palletRepo.GetNextPalletIdAsync();
			addPalletDTO.DateCreated = DateTime.Now;
			var validationResult = await _pickingValidator.ValidateAsync(addPalletDTO);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			var pallet = _mapper.Map<Pallet>(addPalletDTO);
			var id = await _palletRepo.AddPalletAsync(pallet);

			var listRecordsPMD = new List<PalletMovementDetails>();
			foreach (var product in pallet.ProductsOnPallet)
			{
				var recordPalletMovementDetails = new PalletMovementDetails
				{
					ProductId = product.Id,
					Quantity = product.Quantity,
				};
				listRecordsPMD.Add(recordPalletMovementDetails);
			}
			var recordPM = new PalletMovement
			{
				PalletId = id,
				LocationId = pallet.LocationId,
				Reason = ReasonMovement.Picking,
				PerformedBy = addPalletDTO.UserId,
				MovementDate = DateTime.Now,
				PalletMovementDetails = listRecordsPMD
			};
			await _palletMovementRepo.AddPalletMovementAsync(recordPM);

			//foreach (var product in pallet.ProductsOnPallet)
			//{
			//	var record = new PalletMovement
			//	{
			//		PalletId = pallet.Id,
			//		ProductId = product.ProductId,
			//		LocationId = pallet.LocationId,
			//		Reason = ReasonMovement.Picking,
			//		PerformedBy = addPalletDTO.UserId,
			//		Quantity = product.Quantity,
			//		MovementDate = DateTime.Now,
			//	};
			//	await _palletMovementRepo.AddPalletMovementAsync(record);
			//}
			await _werehouseDbContext.SaveChangesAsync();
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
		public UpdatePalletDTO GetPalletToEdit(string id)
		{
			var pallet = _palletRepo.GetPalletById(id);
			var palletDTO = _mapper.Map<UpdatePalletDTO>(pallet);
			return palletDTO;
		}
		public void UpdatePallet(UpdatePalletDTO updatingPallet)
		{
			var existingPallet = _palletRepo.GetPalletById(updatingPallet.Id);
			var a = existingPallet.ProductsOnPallet.Count();
			var validationResult = _updateValidator.Validate(updatingPallet);
			if (!validationResult.IsValid)
			{
				throw new ValidationException(validationResult.Errors);
			}
			_mapper.Map(updatingPallet, existingPallet);
			CollectionSynchronizer.SynchronizeCollection(
				existingPallet.ProductsOnPallet,
				updatingPallet.ProductsOnPallet,
				a => a.Id,
				a => a.Id,
				dto =>
				{
					var newProduct = _mapper.Map<ProductOnPallet>(dto);
					newProduct.PalletId = existingPallet.Id;
					return newProduct;
				},
				(dto, entity) => _mapper.Map(dto, entity));
			_palletRepo.UpdatePallet(existingPallet);
		}
	}
}
