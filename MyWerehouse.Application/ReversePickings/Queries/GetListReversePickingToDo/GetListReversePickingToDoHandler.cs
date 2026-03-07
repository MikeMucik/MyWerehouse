using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.ReversePickings.Queries.GetListReversePickingToDo
{
	public class GetListReversePickingToDoHandler : IRequestHandler<GetListReversePickingToDoQuery, AppResult< ListReversePickingDTO>>
	{
		private readonly IReversePickingRepo _reversePickingRepo;
		private readonly IMapper _mapper;
		public GetListReversePickingToDoHandler(IReversePickingRepo reversePickingRepo,
			IMapper mapper)
		{
			_reversePickingRepo = reversePickingRepo;
			_mapper = mapper;
		}
		public async Task<AppResult<ListReversePickingDTO>> Handle (GetListReversePickingToDoQuery query, CancellationToken ct)
		{
			var listReversePickingTasks = _reversePickingRepo.GetReversePickings()
				.Where(r => r.Status == ReversePickingStatus.Pending && r.DateMade >= query.Start && r.DateMade <= query.End)
				.ProjectTo<ReversePickingDTO>(_mapper.ConfigurationProvider);
			if (!listReversePickingTasks.Any()) return AppResult<ListReversePickingDTO>.Fail("Nie ma zadań dekompletacji", ErrorType.NotFound);
			var listToShow = await listReversePickingTasks
				.Skip(query.PageSize * (query.PageNumber - 1))
				.Take(query.PageSize)
				.ToListAsync(ct);
			var listReversePickingDTO = new ListReversePickingDTO
			{
				DTOs = listToShow,
				PageSize = query.PageSize,
				CurrentPage = query.PageNumber,
				Count = await listReversePickingTasks.CountAsync(ct)
			};
			return AppResult<ListReversePickingDTO>.Success( listReversePickingDTO); 
		}
	}
}
