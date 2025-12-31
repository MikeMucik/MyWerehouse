using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Inventories.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Inventories.Queries.GetInventory
{
	public class GetInventoryHandler : IRequestHandler<GetInventoryQuery, InventoryDTO>
	{
		public readonly IInventoryRepo _inventoryRepo;
		public readonly IMapper _mapper;
		public GetInventoryHandler(IInventoryRepo inventoryRepo,
			IMapper mapper)
		{
			_inventoryRepo = inventoryRepo;
			_mapper = mapper;
		}
		public async Task<InventoryDTO> Handle (GetInventoryQuery request, CancellationToken ct)
		{
			var inventory = await _inventoryRepo.GetInventoryForProductAsync(request.ProductId);
			var inventoryDTO = _mapper.Map<InventoryDTO>(inventory);
			return inventoryDTO;
		}
	}
}
