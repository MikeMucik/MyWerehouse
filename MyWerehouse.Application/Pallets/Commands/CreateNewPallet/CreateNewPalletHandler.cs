using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Pallets.Events.CreateOperation;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.Pallets.Commands.CreateNewPallet
{
	public class CreateNewPalletHandler : IRequestHandler<CreateNewPalletCommand, Pallet>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPalletRepo _palletRepo;
		private readonly IMapper _mapper;
		private readonly IMediator _mediator;

		public CreateNewPalletHandler(WerehouseDbContext werehouseDbContext,
			IPalletRepo palletRepo,
			IMapper mapper,
			IMediator mediator)
		{
			_werehouseDbContext = werehouseDbContext;
			_palletRepo = palletRepo;
			_mapper = mapper;
			_mediator = mediator;
		}
		public async Task<Pallet> Handle(CreateNewPalletCommand request, CancellationToken ct)
		{
			var newIdForPallet = await _palletRepo.GetNextPalletIdAsync();

			var pallet = _mapper.Map<Pallet>(request.DTO);
			//pallet.Status = PalletStatus.InStock;
			pallet.Id = newIdForPallet;
			pallet.LocationId = 1;
			pallet.Status = PalletStatus.InStock;

			_palletRepo.AddPallet(pallet);
			await _werehouseDbContext.SaveChangesAsync(ct);
			//_historyService.CreateOperation(pallet, userId, PalletStatus.Available);
			await _mediator.Publish(new CreatePalletOperationNotification(
							pallet.Id,
							pallet.LocationId,
							ReasonMovement.New,
							request.UserId,
							PalletStatus.Available,//?
							null
						),ct);
			//await _werehouseDbContext.SaveChangesAsync(ct);
			return pallet;
		}
	}
}
