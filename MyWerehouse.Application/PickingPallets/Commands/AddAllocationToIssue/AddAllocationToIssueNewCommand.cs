using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Application.Common.Results;
using MyWerehouse.Domain.Issuing.Models;
using MyWerehouse.Domain.Pallets.Models;
using MyWerehouse.Domain.Picking.Models;

namespace MyWerehouse.Application.PickingPallets.Commands.AddAllocationToIssue
{
	public record AddAllocationToIssueNewCommand(List<Pallet> Pallets,
		List<VirtualPallet> VirtualPallets, Issue Issue, int ProductId,
		int Rest, DateOnly BestBefore, string PerfomedBy) : IRequest<AddAllocationToIssueResult>;
}
