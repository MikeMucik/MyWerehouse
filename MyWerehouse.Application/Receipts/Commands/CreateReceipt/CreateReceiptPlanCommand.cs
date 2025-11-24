using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using MyWerehouse.Application.Receipts.DTOs;
using MyWerehouse.Application.Results;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt

{
	public record CreateReceiptPlanCommand(CreateReceiptPlanDTO DTO) : IRequest<ReceiptResult>;
}
