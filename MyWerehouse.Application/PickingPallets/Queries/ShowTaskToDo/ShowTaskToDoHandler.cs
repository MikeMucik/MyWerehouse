using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Pagination;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler(IVirtualPalletRepo virtualPalletRepo,
		IPickingTaskRepo pickingTaskRepo, IMapper mapper) : IRequestHandler<ShowTaskToDoQuery, AppResult<PagedResult<PickingTaskDTO>>>
	{
		private readonly IVirtualPalletRepo _virtualPalletRepo = virtualPalletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<PagedResult<PickingTaskDTO>>> Handle(ShowTaskToDoQuery request, CancellationToken ct)
		{
			var palletVirtualId = await _virtualPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletSourceScannedId);
			var pickingTasks =  _pickingTaskRepo.GetPickingTaskList(palletVirtualId, request.PickingDate);
			var pickingTaskOrdered = pickingTasks.OrderBy(t => t.Id);
			var result = await pickingTaskOrdered.ToPagedResultAsync<PickingTask, PickingTaskDTO>(
				_mapper.ConfigurationProvider,
				request.CurrentPage,
				request.PageSize,
				ct);
			if (result.TotalCount == 0) return AppResult<PagedResult<PickingTaskDTO>>.Fail("Brak zadań kompletacyjnych dla palety", ErrorType.NotFound);

			return AppResult<PagedResult<PickingTaskDTO>>.Success(result);
		}
	}
}