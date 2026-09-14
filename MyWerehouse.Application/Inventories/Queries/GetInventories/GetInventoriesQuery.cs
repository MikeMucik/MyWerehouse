using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.Inventories.DTOs;

namespace MyWerehouse.Application.Inventories.Queries.GetInventories
{
	public record GetInventoriesQuery(int PageNumber, int PageSize) : IRequest<AppResult<PagedResult<InventoryDTO>>>;
}
