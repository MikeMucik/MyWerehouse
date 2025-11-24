using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public record AddAllocationToIssueCommand(List<Pallet> Pallets, List<VirtualPallet> VirtualPallets, Issue Issue, int ProductId, int Rest, DateOnly BestBefore, string PerfomedBy):IRequest<List<Allocation>>;	
}
