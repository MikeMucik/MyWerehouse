using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Inventories.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Inventories.Queries.GetInventories
{
	public class GetInventoriesHandler(IInventoryRepo inventoryRepo,
		IMapper mapper) : IRequestHandler<GetInventoriesQuery, ListOfInventoryDTO>
	{		
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<ListOfInventoryDTO> Handle (GetInventoriesQuery request, CancellationToken ct)
		{
			var inventories = _inventoryRepo.GetAllInventory()
				.OrderBy(i => i.ProductId)
				.ProjectTo<InventoryDTO>(_mapper.ConfigurationProvider);
			var inventoriesToShow = await inventories
				.Skip(request.PageSize * (request.PageNumber - 1))
				.Take(request.PageSize)
				.ToListAsync(ct);
			return new ListOfInventoryDTO()
			{
				InventoryDTOs = inventoriesToShow,
				PageSize = request.PageSize,
				PageNumber = request.PageNumber,
				Count =await inventories.CountAsync(ct)
			};
		}
	}
}
