using MediatR;
using MyWerehouse.Application.Picking.DTOs;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Services;
using MyWerehouse.Application.Interfaces;
using MyWerehouse.Application.Common.Interfaces.Persistence;
using MyWerehouse.Application.Common.Interfaces;

namespace MyWerehouse.Application.Picking.Commands.FinishPlannedPickingPrepareToHandPicking
{
	public class FinishPlannedPickingPrepareToHandPickingHandler(
		IUnitOfWork unitOfWork,
		IPickingTaskRepo pickingTaskRepo,
		IIssueRepo issueRepo,
		IPickingDomainService pickingDomainService,
		IDateTimeProvider dateTimeProvider) : IRequestHandler<FinishPlannedPickingPrepareToHandPickingCommand, AppResult<List<PickingTaskDTO>>>
	{
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly IPickingTaskRepo _pickingTaskRepo = pickingTaskRepo;
		private readonly IIssueRepo _issueRepo = issueRepo;
		private readonly IPickingDomainService _pickingDomainService = pickingDomainService;
		private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

		public async Task<AppResult<List<PickingTaskDTO>>> Handle(FinishPlannedPickingPrepareToHandPickingCommand command, CancellationToken ct)
		{
			var now = _dateTimeProvider.UtcNow;
			var listToDoTasks = new List<PickingTaskDTO>();

			var sendDateStart = command.Start ?? _dateTimeProvider.Today;
			var sendDateEnd = command.End ?? _dateTimeProvider.Today.AddDays(1);
			var listOfIssues = await _issueRepo.GetIssuesByDates(sendDateStart, sendDateEnd, ct);
			
			foreach (var issue in listOfIssues)
			{
				var reducedList = await _pickingTaskRepo.GetPickingTasksByIssueIdAsync(issue.Id, ct);
				var listHandTasks = _pickingDomainService.PrepareHandPickingTasks(reducedList, issue.Id, command.UserId, now, _dateTimeProvider.Today);

				foreach (var handTask in listHandTasks)
				{
					_pickingTaskRepo.AddPickingTask(handTask);
					var handTaskDTO = new PickingTaskDTO
					{
						Id = handTask.Id,
						IssueId = handTask.IssueId,
						IssueNumber = issue.IssueNumber,
						ProductId = handTask.ProductId,
						SKU = reducedList.First(x=>x.ProductId == handTask.ProductId).Product.SKU,
						RequestedQuantity = handTask.RequestedQuantity,
						PickingStatus = handTask.PickingStatus,
						BestBefore = handTask.BestBefore
					};
					listToDoTasks.Add(handTaskDTO);
				}
			}
			await _unitOfWork.SaveChangesAsync(ct);
			return AppResult<List<PickingTaskDTO>>.Success(listToDoTasks);
		}
	}
}
