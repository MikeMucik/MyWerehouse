using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyWerehouse.Application.PickingPallets.DTOs;
using MyWerehouse.Domain.Interfaces;
using MyWerehouse.Domain.Receviving.Filters;
using MyWerehouse.Domain.Picking.Models;
using AutoMapper;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Infrastructure.Persistence;

namespace MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public class FinishPlannedPickingPrepareToHandPickingHandle(
		WerehouseDbContext werehouseDbContext,
		IPickingTaskRepo pickingTaskRepo,
		IIssueRepo issueRepo,
		IMapper mapper) : IRequestHandler<FinishPlannedPickingPrepareToHandPickingCommand, AppResult< List<PickingTaskDTO>>>
	{
		private readonly WerehouseDbContext _werehouseDbContext = werehouseDbContext;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IMapper _mapper = mapper;

		public async Task<AppResult<List<PickingTaskDTO>>> Handle(FinishPlannedPickingPrepareToHandPickingCommand command, CancellationToken ct)
		{
			var listToDoTasks = new List<PickingTaskDTO>();
			var filtr = new IssueReceiptSearchFilter
			{
				DateTimeStart = DateTime.UtcNow.AddDays(-1),
				DateTimeEnd = DateTime.UtcNow,
			};
			var listOfIssues = await _issueRepo.GetIssuesByFilter(filtr).ToListAsync(ct);
			foreach (var issue in listOfIssues)
			{
				var listOfPickTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issue.Id);
				var reducedList = listOfPickTasks.Where(t => t.PickingStatus == PickingStatus.Allocated ||
				t.PickingStatus == PickingStatus.Correction).ToList(); //biorę pod uwagę tylko aktywne taski do wykonania
				var listByProductAndDate = reducedList
					.GroupBy(p => new {p.IssueId, p.ProductId, p.BestBefore })
					.Select(g => new
					{
						g.Key.ProductId,
						g.Key.BestBefore,
						TotalQuantity = g.Sum(p => p.RequestedQuantity - p.PickedQuantity)
					}).ToList();
				foreach (var task in listByProductAndDate)
				{
					var taskToDo = PickingTask.Create(null, issue.Id, task.TotalQuantity, PickingStatus.Available, task.ProductId,
						task.BestBefore, null, DateOnly.FromDateTime(DateTime.UtcNow), 0);
									 
					 _pickingTaskRepo.AddPickingTask(taskToDo);
					var handTaskDTO = _mapper.Map<PickingTaskDTO>(taskToDo);
					
					listToDoTasks.Add(handTaskDTO);
				}
				foreach (var task in reducedList)
				{
					task.Cancel(command.UserId, issue.IssueNumber);
				}
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			return AppResult<List<PickingTaskDTO>>.Success(listToDoTasks);
		}
	}
}
