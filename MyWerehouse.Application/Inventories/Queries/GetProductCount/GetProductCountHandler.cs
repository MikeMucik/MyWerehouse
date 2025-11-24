using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Inventories.Queries.GetProductCount
{
	public class GetProductCountHandler(IInventoryRepo inventoryRepo) : IRequestHandler<GetProductCountQuery, int>
	{
		private readonly IInventoryRepo _inventoryRepo = inventoryRepo;
		public async Task<int> Handle(GetProductCountQuery command, CancellationToken cancellationToken)
		{
			var totalProductByDate = await _inventoryRepo.GetQuantityForProductAsync(command.ProductId, command.BestBefore);
			var totalProductReservedToIssues = await _inventoryRepo.GetQuantityProductReservedForIssueAsync(command.ProductId, command.BestBefore);
			var totalProductReservedToPicking = await _inventoryRepo.GetQuantityProductReservedForPickingAsync(command.ProductId, command.BestBefore);
			return totalProductByDate - totalProductReservedToIssues - totalProductReservedToPicking;
		}
	}
}
