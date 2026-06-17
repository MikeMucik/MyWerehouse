using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MyWerehouse.Application.Common.Mapping;
using MyWerehouse.Domain.Receviving.Models;

namespace MyWerehouse.Application.Receipts.Commands.CreateReceipt
{
	public class CreateReceiptPlanDTO : IMapFrom<Receipt>
	{
		public int ClientId { get; init; }
		public DateTime ReceiptDateTime { get; init; }
		public string PerformedBy { get; init; }
		public ReceiptStatus ReceiptStatus { get; init; }
		public int RampNumber { get; init; }
	}
}
