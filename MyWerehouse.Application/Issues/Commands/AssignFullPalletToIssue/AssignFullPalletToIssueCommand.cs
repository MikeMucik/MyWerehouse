using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MyWerehouse.Domain.Models;

namespace MyWerehouse.Application.Issues.Commands.AssignFullPalletToIssue
{
	public record AssignFullPalletToIssueCommand(Issue Issue,List<Pallet> Pallets, int FullPalletCount) :IRequest<List<Pallet>>;	
}
