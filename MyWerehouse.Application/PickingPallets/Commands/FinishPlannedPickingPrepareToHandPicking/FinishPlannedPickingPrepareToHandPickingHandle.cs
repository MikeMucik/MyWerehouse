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
using MyWerehouse.Infrastructure;

namespace MyWerehouse.Application.PickingPallets.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public class FinishPlannedPickingPrepareToHandPickingHandle : IRequestHandler<FinishPlannedPickingPrepareToHandPickingCommand, List<HandPickingDTO>>
	{
		private readonly WerehouseDbContext _werehouseDbContext;
		private readonly IPickingTaskRepo _pickingTaskRepo;
		private readonly IIssueRepo _issueRepo;
		private readonly IHandPickingTaskRepo _handPickingTaskRepo;
		private readonly IMapper _mapper;
		public FinishPlannedPickingPrepareToHandPickingHandle(
			WerehouseDbContext werehouseDbContext,
			IPickingTaskRepo pickingTaskRepo,
			IIssueRepo issueRepo,
			IHandPickingTaskRepo handPickingTaskRepo,
			IMapper mapper)
		{
			_werehouseDbContext = werehouseDbContext;
			_pickingTaskRepo = pickingTaskRepo;
			_issueRepo = issueRepo;
			_handPickingTaskRepo = handPickingTaskRepo;
			_mapper = mapper;
		}
		public async Task<List<HandPickingDTO>> Handle(FinishPlannedPickingPrepareToHandPickingCommand command, CancellationToken ct)
		{
			var listToDoTasks = new List<HandPickingDTO>();
			var filtr = new IssueReceiptSearchFilter
			{
				DateTimeStart = DateTime.UtcNow.AddDays(-1),
				DateTimeEnd = DateTime.UtcNow,
			};
			var listOfIsses = await _issueRepo.GetIssuesByFilter(filtr).ToListAsync(ct);
			foreach (var issue in listOfIsses)
			{
				var listOfPickTasks = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issue.Id);
				var listByProductAndDate = listOfPickTasks
					.GroupBy(p => new {p.IssueId, p.ProductId, p.BestBefore })
					.Select(g => new
					{
						g.Key.ProductId,
						g.Key.BestBefore,
						TotalQuantity = g.Sum(p => p.RequestedQuantity - p.PickedQuantity)
					}).ToList();
				foreach (var task in listByProductAndDate)
				{
					var taskToDo = new HandPickingTask
					{
						IssueId = issue.Id,
						ProductId = task.ProductId,
						Quantity = task.TotalQuantity,
						PickingStatus = PickingStatus.Allocated,
						BestBefore = task.BestBefore,
						CreateDate = DateTime.UtcNow,
					};
					 _handPickingTaskRepo.AddHandPickingTask(taskToDo);
					var handTaskDTO = _mapper.Map<HandPickingDTO>(taskToDo);
					
					listToDoTasks.Add(handTaskDTO);
				}
				foreach (var task in listOfPickTasks)
				{
					task.PickingStatus = PickingStatus.Cancelled;
				}
			}
			await _werehouseDbContext.SaveChangesAsync(ct);
			return listToDoTasks;
		}
	}
}
