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
	public class GetInvetoriesHandler(IInventoryRepo inventoryRepo,
		IMapper mapper) : IRequestHandler<GetInvetoriesQuery, ListOfInventoryDTO>
	{		
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<ListOfInventoryDTO> Handle (GetInvetoriesQuery request, CancellationToken ct)
		{
			var invetories = _inventoryRepo.GetAllInventory()
				.OrderBy(i => i.ProductId)
				.ProjectTo<InventoryDTO>(_mapper.ConfigurationProvider);
			var inventoriesToShow = await invetories
				.Skip(request.PageSize * (request.PageNumber - 1))
				.Take(request.PageSize)
				.ToListAsync(ct);
			return new ListOfInventoryDTO()
			{
				inventoryDTOs = inventoriesToShow,
				PageSize = request.PageSize,
				PageNumber = request.PageNumber,
				Count =await invetories.CountAsync(ct)
			};
		}
	}
}
