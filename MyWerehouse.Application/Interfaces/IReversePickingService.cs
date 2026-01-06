using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Application.ReversePickings.DTOs;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.Interfaces
{
	public interface IReversePickingService
	{
		Task<List<ReversePicking>> CreateTaskToReversePickingAsync(string palletId, string userId);
		Task<ReversePickingResult> ExecutiveReversePickingAsync(int taskReverseId, ReversePickingStrategy strategy, string userId);
		Task<ReversePickingDetails> GetReversePickingAsync(int reversePickingId);
		Task<ListReversePickingDTO> GetListReversePickingToDo(int pageSize, int pageNumber);
	}
}
