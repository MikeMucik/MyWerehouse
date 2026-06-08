using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MyWerehouse.Application.Common.Results;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt

{
	public record CreateReceiptPlanCommand(CreateReceiptPlanDTO DTO) : IRequest<AppResult<Unit>>;
}
