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
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.Picking.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler(IVirtualPalletRepo virtualPalletRepo,
		IPickingTaskRepo pickingTaskRepo, IMapper mapper, IDateTimeProvider dateTimeProvider)
		: IRequestHandler<ShowTaskToDoQuery, AppResult<PagedResult<PickingTaskDTO>>>
	{
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IMapper _mapper = mapper;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<PagedResult<PickingTaskDTO>>> Handle(ShowTaskToDoQuery request, CancellationToken ct)
		{
			var palletVirtualId = await _virtualPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletSourceScannedId, ct);
			if (palletVirtualId == Guid.Empty)
			{
				return AppResult<PagedResult<PickingTaskDTO>>.Fail("Source pallet was not found.");
			}

			var pickingDate = request.PickingDate ?? _dateTimeProvider.Today;
			var pickingTasks =  _pickingTaskRepo.GetPickingTaskList(palletVirtualId, pickingDate)
				.AsNoTracking();
			var pickingTaskOrdered = pickingTasks.OrderBy(t => t.Id);
			var result = await pickingTaskOrdered
				.ProjectTo<PickingTaskDTO>(_mapper.ConfigurationProvider)
				.ToPagedResultAsync(request.CurrentPage,request.PageSize,ct);
			return AppResult<PagedResult<PickingTaskDTO>>.Success(result);
		}
	}
}
