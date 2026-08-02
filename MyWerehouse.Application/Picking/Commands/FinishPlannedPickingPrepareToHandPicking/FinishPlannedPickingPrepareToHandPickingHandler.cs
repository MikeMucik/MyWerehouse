using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Picking.Models;
using AutoMapper;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Infrastructure.Persistence;
using MyWerehouse.Domain.Receiving.Filters;
using MyWerehouse.Domain.Common;
using MyWerehouse.Domain.Services;

namespace MyWerehouse.Application.Picking.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public class FinishPlannedPickingPrepareToHandPickingHandler(
		WerehouseDbContext werehouseDbContext,
		IPickingTaskRepo pickingTaskRepo,
		IIssueRepo issueRepo,
		IPickingDomainService pickingDomainService,
		IMapper mapper,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<FinishPlannedPickingPrepareToHandPickingCommand, AppResult<List<PickingTaskDTO>>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;
		private readonly IMapper _mapper = mapper;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<List<PickingTaskDTO>>> Handle(FinishPlannedPickingPrepareToHandPickingCommand command, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var listToDoTasks = new List<PickingTaskDTO>();
			
			var filtr = new IssueReceiptSearchFilter
			{
				SendDateStart = command.Start ?? _dateTimeProvider.Today,
				SendDateEnd = command.End ?? _dateTimeProvider.Today.AddDays(1)				
			};
			var listOfIssues = await _issueRepo.GetIssuesByFilter(filtr).ToListAsync(ct);
			foreach (var issue in listOfIssues)
			{
				var reducedList =	await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issue.Id);
				var listHandTasks = _pickingDomainService.PrepareHandPickingTasks(reducedList, issue.Id, command.UserId, now, _dateTimeProvider.Today);
				
				foreach (var handTask in listHandTasks)
				{
					_pickingTaskRepo.AddPickingTask(handTask);
					var handTaskDTO = _mapper.Map<PickingTaskDTO>(handTask);
					listToDoTasks.Add(handTaskDTO);
				}
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<List<PickingTaskDTO>>.Success(listToDoTasks);
		}
	}
}
