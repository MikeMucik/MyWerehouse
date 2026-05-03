using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo
{
	public class GetListReversePickingToDoHandler(IReversePickingRepo reversePickingRepo,
		IMapper mapper) : IRequestHandler<GetListReversePickingToDoQuery, AppResult<PagedResult<ReversePickingDTO>>>
	{
		private readonly IReversePickingRepo _reversePickingRepo = reversePickingRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<PagedResult<ReversePickingDTO>>> Handle (GetListReversePickingToDoQuery query, CancellationToken ct)
		{
			var listReversePickingTasks = _reversePickingRepo.GetReversePickings()
				.Where(r => r.Status == ReversePickingStatus.Pending && r.DateMade >= query.Start && r.DateMade <= query.End);
			var reversePickingOrdered = listReversePickingTasks.OrderBy(r => r.Id);

			var result = await reversePickingOrdered.ToPagedResultAsync<ReversePicking, ReversePickingDTO>(
				_mapper.ConfigurationProvider,
				query.PageNumber,
				query.PageSize,
				ct);

			if(result.TotalCount == 0)return AppResult<PagedResult<ReversePickingDTO>>.Fail("Nie ma zadań dekompletacji", ErrorType.NotFound);
			return AppResult<PagedResult<ReversePickingDTO>>.Success(result); 
		}
	}
}