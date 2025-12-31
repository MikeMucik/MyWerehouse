using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.ViewModels.AllocationModels;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.PickingPallets.Queries.ShowTaskToDo
{
	public record ShowTaskToDoQuery(string PalletSourceScannedId, DateTime PickingDate):IRequest<List<AllocationDTO>>;	
}
