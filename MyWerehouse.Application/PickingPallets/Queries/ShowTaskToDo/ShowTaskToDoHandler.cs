using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public class ShowTaskToDoHandler:IRequestHandler<ShowTaskToDoQuery, AppResult< List<PickingTaskDTO>>>
	{
		private readonly IPickingPalletRepo _pickingPalletRepo;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IMapper _mapper;
		public ShowTaskToDoHandler(IPickingPalletRepo pickingPalletRepo,
			IPickingTaskRepo pickingTaskRepo, IMapper mapper)
		{
			_pickingPalletRepo = pickingPalletRepo;
			_pickingTaskRepo = pickingTaskRepo;
			_mapper = mapper;
		}
		public async Task<AppResult<List<PickingTaskDTO>>> Handle (ShowTaskToDoQuery request, CancellationToken ct)
		{
			var palletVirtualId = await _pickingPalletRepo.GetVirtualPalletIdFromPalletIdAsync(request.PalletSourceScannedId);
			var pickingTasks = await _pickingTaskRepo.GetPickingTaskListAsync(palletVirtualId, request.PickingDate);			
			var result =  pickingTasks.Select(pickingTask => _mapper.Map<PickingTaskDTO>(pickingTask)).ToList();
			return AppResult<List<PickingTaskDTO>>.Success(result);
		}
	}
	
}
